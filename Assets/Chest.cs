using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;



public class Chest : NetworkBehaviour
{

    public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);
    public int slotCount = 10;
    [HideInInspector] public GameObject interactingPlayer;
    [HideInInspector] public List<GameObject> visibleSlots;
    public List<ServerSlot> serverSlots = new List<ServerSlot>();
    public GameObject serverSlot;

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            SetUpServerSlotsServerRpc();
        }
        
    }

    [ServerRpc]
    void SetUpServerSlotsServerRpc()
    {
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(serverSlot);
            slot.GetComponent<NetworkObject>().Spawn();
            slot.transform.SetParent(transform);
            serverSlots.Add(slot.GetComponent<ServerSlot>());
        }

        SetupServerSlotsClientRpc();
    }

    [ClientRpc]
    void SetupServerSlotsClientRpc()
    {
        if (!IsServer)
        {
            for (int i = 0; i < slotCount; i++)
            {
                serverSlots.Add(transform.GetChild(i).GetComponent<ServerSlot>());
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddItem(Item item, int specificSlotIndex = -1)
    {

        if (specificSlotIndex > -1)
        {

            // Check if a specific slot is provided
            if (visibleSlots[specificSlotIndex] != null)
            {
                // Add to the specific slot if it's empty or can stack the item
                if (visibleSlots[specificSlotIndex].GetComponent<ItemSlot>().item == null ||
                    (visibleSlots[specificSlotIndex].GetComponent<ItemSlot>().item == item && visibleSlots[specificSlotIndex].GetComponent<ItemSlot>().itemCount < item.maxStackCount))
                {
                    visibleSlots[specificSlotIndex].GetComponent<ItemSlot>().OnItemGained(item);
                    return;
                }
            }
        }


        if (visibleSlots.Count > 0)
        {
            //if item is found adds to count/adds to new slot if overflow
            foreach (var slot in visibleSlots)
            {
                var itemSlot = slot.GetComponent<ItemSlot>();
                if (itemSlot.item == item && itemSlot.itemCount < item.maxStackCount)
                {
                    slot.GetComponent<ItemSlot>().OnItemGained(item);
                    return;
                }

            }

            //adds to empty slots
            foreach (var slot in visibleSlots)
            {
                var itemSlot = slot.GetComponent<ItemSlot>();

                if (itemSlot.item == null)
                {
                    itemSlot.OnItemGained(item);
                    return;
                }
            }
        }

    }

    public void AddItemToSlot()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenChestServerRpc(ulong clientId)
    {
        isOpen.Value = true;
        OpenedChestClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void CloseChestServerRpc()
    {
        isOpen.Value = false;
    }


    [ClientRpc]
    void OpenedChestClientRpc(ClientRpcParams clientRpcParams = default)
    {
        var player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);

        Debug.Log(player + " has opened a chest");


        foreach (var slot in visibleSlots)
        {
            if(slot.GetComponent<ItemSlot>().item != null)
            {
                slot.GetComponent<ItemSlot>().itemCount = 0;
                slot.GetComponent<ItemSlot>().OnItemRemoved();

            }
        }

        int index = 0;

        foreach (var slot in serverSlots)
        {
            index++;
            if (slot.hasItem.Value)
            {

                StartCoroutine(AddItemCoroutine(index, slot));
                
            }
        }
    }

    IEnumerator AddItemCoroutine(int index, ServerSlot slot) 
    {
        for (int i = 0; i < slot.itemCount.Value; i++)
        {
            AddItem(ItemHolder.Instance.GetItemFromId(slot.itemId.Value), index - 1);
            yield return new WaitForSeconds(0.01f);

        }

    }
}
