using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : NetworkBehaviour
{

    public float interactRange;
    float treeTimer;

    InventoryManager inventoryManager;
    NetworkObject networkObject;

    public GameObject chestTab;
    public Transform backpackTabParent;

    public Button chestButton;
    public Button backpackButton;

    [HideInInspector] public Chest chest;

    public bool isInChest;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
        networkObject = GetComponent<NetworkObject>();

        chestButton.onClick.AddListener(OpenChestTab);
        backpackButton.onClick.AddListener(OpenChestBackpackTab);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;


        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, interactRange) && !inventoryManager.inventory.activeSelf)
        {
            if (hit.collider.GetComponent<Tool>())
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hit.collider.gameObject.GetComponent<ItemCount>())
                    {
                        for (int i = 0; i < hit.collider.gameObject.GetComponent<ItemCount>().itemCount.Value; i++)
                        {
                            inventoryManager.AddItem(hit.collider.GetComponent<Tool>().item);
                        }
                    }
                    else
                    {
                        inventoryManager.AddItem(hit.collider.GetComponent<Tool>().item);
                    }
                    

                    DestroyInteractObjServerRpc(hit.collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            else if (hit.collider.GetComponent<Chest>())
            {
                chest = hit.collider.GetComponent<Chest>();

                if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.I)) && isInChest)
                {
                    if (chest.isOpen.Value)
                    {
                        GetComponent<PlayerMovement>().enabled = true;
                        GetComponent<PlayerLook>().enabled = true;
                        CloseChest(chest);
                        chest.CloseChestServerRpc();
                    }


                }
                else if (Input.GetKeyDown(KeyCode.E) && !isInChest)
                {
                    if (!chest.isOpen.Value)
                    {
                        inventoryManager.activeParent = inventoryManager.chestInterface.transform;
                        GetComponent<PlayerMovement>().enabled = false;
                        GetComponent<PlayerLook>().enabled = false;
                        OpenChest(chest);
                        chest.OpenChestServerRpc(NetworkManager.Singleton.LocalClientId);
                    }


                }
            }
        }
    }

    public void OpenChestTab()
    {
        backpackTabParent.GetChild(0).gameObject.SetActive(false);
        chestTab.SetActive(true);

        inventoryManager.OpenTab(null, chestButton, chestButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>());
        inventoryManager.CloseTab(null, backpackButton, backpackButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>());
    }

    public void OpenChestBackpackTab()
    {
        backpackTabParent.GetChild(0).gameObject.SetActive(true);
        chestTab.SetActive(false);

        inventoryManager.OpenTab(null, backpackButton, backpackButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>());
        inventoryManager.CloseTab(null, chestButton, chestButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>());
    }


    void OpenChest(Chest chest)
    {
        isInChest = true;
       
        inventoryManager.backpackTab.transform.GetChild(0).SetParent(GetComponent<InventoryManager>().chestInterface.transform.GetChild(0));
        inventoryManager.chestInterface.transform.GetChild(0).GetChild(GetComponent<InventoryManager>().chestInterface.transform.GetChild(0).childCount - 1).SetAsFirstSibling();
        inventoryManager.chestInterface.SetActive(true);


        OpenChestBackpackTab();

        for (int i = 0; i < chest.slotCount; i++)
        {
            chestTab.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
            
            chestTab.transform.GetChild(0).GetChild(i).gameObject.GetComponent<ItemSlot>().inventoryManager = inventoryManager;
            chestTab.transform.GetChild(0).GetChild(i).gameObject.GetComponent<ItemSlot>().slotIndex = i;

            chest.visibleSlots.Add(chestTab.transform.GetChild(0).GetChild(i).gameObject);
        }
        Cursor.lockState = CursorLockMode.None;
        chest.interactingPlayer = gameObject;
    }



    void CloseChest(Chest chest)
    {
        foreach (var slot in chest.visibleSlots)
        {
            if (slot.GetComponent<ItemSlot>().item != null)
            {
                inventoryManager.UpdateServerSlotsServerRpc(slot.GetComponent<ItemSlot>().slotIndex, slot.GetComponent<ItemSlot>().item.id, slot.GetComponent<ItemSlot>().itemCount, 
                    chest.GetComponent<NetworkObject>().NetworkObjectId);

            }
            else
            {
                inventoryManager.RemoveItemsFromChestServerRpc(slot.GetComponent<ItemSlot>().slotIndex, chest.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
        inventoryManager.chestInterface.transform.GetChild(0).GetChild(0).SetParent(GetComponent<InventoryManager>().backpackTab.transform);

       inventoryManager.backpackTab.transform.GetChild(GetComponent<InventoryManager>().backpackTab.transform.childCount - 1).SetAsFirstSibling();
        GetComponent<InventoryManager>().backpackTab.transform.GetChild(0).gameObject.SetActive(true);
        isInChest = false;
       inventoryManager.chestInterface.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        chest.interactingPlayer = null;

    }

    [ServerRpc]
    void DestroyInteractObjServerRpc(ulong objId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objId, out NetworkObject obj))
        {
            obj.Despawn();
        }
    }
}
