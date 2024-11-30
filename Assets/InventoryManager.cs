using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Data.Common;
using System.Reflection;

public class InventoryManager : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public float woodCount;

    public GameObject inventory;

    public Button backpackTabButton;
    public Button equipmentTabButton;

    public GameObject backpackTab;
    public GameObject equipmentTab;

    public TextMeshProUGUI backpackButtonText;
    public TextMeshProUGUI equipmentButtonText;

    public Backpack backpackSlot;
    public Backpack toolbeltSlot;

    public Transform toolBeltParent;
    public Transform slotsParent;

    public Transform toolBeltInvPos;
    public Transform toolBeltHudPos;

    List<GameObject> visibleBackpackSlots = new List<GameObject>();
    List<GameObject> visibleToolbeltSlots = new List<GameObject>();


    public KeyCode inventoryKey = KeyCode.I;

    GameObject dragItem;
    GameObject originalSlot;

    ItemSlot hoverSlot;
    int currentSelectedIndex = 0;

    PlayerInterfaceManager playerInterfaceManager;
    BuildingManager buildingManager;
    Transform toolHoldSlot;

    // Start is called before the first frame update
    void Start()
    {

        buildingManager = GetComponent<BuildingManager>();
        playerInterfaceManager = GetComponent<PlayerInterfaceManager>();
        toolHoldSlot = playerInterfaceManager.holder.transform.GetChild(1);

        backpackTabButton.onClick.AddListener(OpenBackpackTab);
        equipmentTabButton.onClick.AddListener(OpenEquipmentTab);
        inventory.SetActive(false);
        OpenBackpackTab();

        currentSelectedIndex = -1;

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            slotsParent.GetChild(i).GetComponent<ItemSlot>().slotIndex = i;
            slotsParent.GetChild(i).GetComponent<ItemSlot>().inventoryManager = this;
        }

        for (int i = 0;i < toolBeltParent.childCount; i++)
        {
            toolBeltParent.GetChild(i).GetComponent<ItemSlot>().slotIndex = i;
            toolBeltParent.GetChild(i).GetComponent<ItemSlot>().inventoryManager = this;
        }

        toolBeltParent.SetParent(inventory.transform.parent);
        toolBeltParent.position = toolBeltHudPos.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey) && inventory.activeSelf)
        {
            toolBeltParent.SetParent(inventory.transform.parent);
            toolBeltParent.transform.localPosition = toolBeltHudPos.localPosition;
            inventory.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            GetComponent<PlayerLook>().enabled = true;
        }
        else if(Input.GetKeyDown(inventoryKey) && !inventory.activeSelf)
        {
            toolBeltParent.SetParent(backpackTab.transform);
            toolBeltParent.transform.localPosition = toolBeltInvPos.localPosition;
            inventory.SetActive(true);
            currentSelectedIndex = -1;
            for (int i = 0; i < toolBeltParent.childCount; i++)
            {
                toolBeltParent.GetChild(i).GetChild(2).gameObject.SetActive(false);
            }
            Cursor.lockState = CursorLockMode.None;
            GetComponent<PlayerLook>().enabled = false;
        }

        if (!inventory.activeSelf)
        {
            ToolbeltSlotSelection();
        }


        if(dragItem != null)
        {
            dragItem.transform.position = Input.mousePosition;

            
            if(Input.GetAxisRaw("Mouse ScrollWheel") < 0 && hoverSlot != null && dragItem.GetComponent<ItemIcon>().itemCount > 1)
            {
                if(dragItem.GetComponent<ItemIcon>().item == hoverSlot.item || hoverSlot.item == null)
                {
                    if (hoverSlot.itemCount < dragItem.GetComponent<ItemIcon>().item.maxStackCount)
                    {
                        dragItem.GetComponent<ItemIcon>().itemCount--;
                        dragItem.GetComponent<ItemIcon>().itemCountText.text = dragItem.GetComponent<ItemIcon>().itemCount.ToString();
                        hoverSlot.GetComponent<ItemSlot>().OnItemGained(dragItem.GetComponent<ItemIcon>().item);
                    }
                }

                
            }
            else if(Input.GetAxisRaw("Mouse ScrollWheel") > 0 && hoverSlot != null && hoverSlot.itemCount > 0 && dragItem.GetComponent<ItemIcon>().item == hoverSlot.item)
            {

                if(hoverSlot.itemCount > 1)
                {
                    dragItem.GetComponent<ItemIcon>().itemCount++;
                    dragItem.GetComponent<ItemIcon>().itemCountText.text = dragItem.GetComponent<ItemIcon>().itemCount.ToString();
                    hoverSlot.GetComponent<ItemSlot>().OnItemRemoved();
                }
                
            }

        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            AddItem(ItemHolder.Instance.stone);
        }
    }

    void ToolbeltSlotSelection()
    {
        if (buildingManager.mode == BuildingManager.Mode.NORMAL)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            int totalSlots = toolBeltParent.childCount;

            if (scrollInput != 0f)
            {
                do
                {
                    if (scrollInput > 0f) // Scroll up
                    {
                        currentSelectedIndex = (currentSelectedIndex - 1 + totalSlots) % totalSlots;
                    }
                    else if (scrollInput < 0f) // Scroll down
                    {
                        currentSelectedIndex = (currentSelectedIndex + 1) % totalSlots;
                    }
                }
                while (!toolBeltParent.GetChild(currentSelectedIndex).gameObject.activeSelf);

                SelectToolSlot(toolBeltParent.GetChild(currentSelectedIndex).gameObject, currentSelectedIndex);
            }

            for (int i = 0; i < toolBeltParent.childCount; i++)
            {
                if (toolBeltParent.GetChild(i).gameObject.activeSelf)
                {
                    // Check if any key from 1 to 9 is pressed
                    for (int key = 1; key <= 9; key++)
                    {
                        if (Input.GetKeyDown(key.ToString()))
                        {
                            int inputIndex = key - 1; // Map key 1-9 to index 0-8

                            if (i == inputIndex) // Match index with active slot
                            {
                                SelectToolSlot(toolBeltParent.GetChild(i).gameObject, i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    void SelectToolSlot(GameObject selectedSlot, int selectedIndex)
    {
        for (int i = 0; i < toolBeltParent.childCount; i++)
        {
            GameObject slot = toolBeltParent.GetChild(i).gameObject;

            if (i == selectedIndex)
            {
                // Activate the 2nd child for the selected slot
                slot.transform.GetChild(2).gameObject.SetActive(true);
                if(slot.GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    slot.GetComponent<ToolbeltSlot>().activeItem.SetActive(true);
                }
            }
            else
            {
                // Deactivate the 2nd child for all other slots
                slot.transform.GetChild(2).gameObject.SetActive(false);
                if (slot.GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    slot.GetComponent<ToolbeltSlot>().activeItem.SetActive(false);
                }
            }
        }
    }

    void ShowBackpackSlots()
    {
        visibleBackpackSlots.Clear();

        if(backpackSlot != null)
        {
            for (int i = 0; i < backpackSlot.slotCount; i++)
            {
                slotsParent.GetChild(i).gameObject.SetActive(true);
                visibleBackpackSlots.Add(slotsParent.GetChild(i).gameObject);
            }
        }
        else
        {
            for (int i = 0; i < slotsParent.childCount; i++)
            {
                slotsParent.GetChild(i).gameObject.SetActive(false);
            }
        }

    }

    void ShowToolbeltSlots()
    {
        visibleToolbeltSlots.Clear();

        if(toolbeltSlot != null)
        {
            for(int i = 0;i < toolbeltSlot.slotCount; i++)
            {
                toolBeltParent.GetChild(i).gameObject.SetActive(true);
                visibleToolbeltSlots.Add(toolBeltParent.GetChild(i).gameObject);

            }
        }
        else
        {
            for (int i = 0; i < toolBeltParent.childCount; i++)
            {
                toolBeltParent.GetChild(i).gameObject.SetActive(false);
                
            }
        }

    }

    public void RemoveItem(Item item)
    {
        foreach (var slot in visibleBackpackSlots)
        {
            if(slot.GetComponent<ItemSlot>().item != null && slot.GetComponent<ItemSlot>().item == item && slot.GetComponent<ItemSlot>().itemCount > 0)
            {
                slot.GetComponent<ItemSlot>().OnItemRemoved();
            }
        }
    }

    public void AddItem(Item item)
    {
        if(visibleBackpackSlots.Count > 0)
        {
            foreach (var slot in visibleBackpackSlots)
            {
                if(slot.GetComponent<ItemSlot>().item == null)
                {
                    slot.GetComponent<ItemSlot>().OnItemGained(item);
                    if (item.GetComponent<Wood>())
                    {
                        woodCount++;
                    }
                    return;
                }
                else if(slot.GetComponent<ItemSlot>().item == item && slot.GetComponent<ItemSlot>().itemCount < item.maxStackCount)
                {
                    slot.GetComponent<ItemSlot>().OnItemGained(item);
                    if (item.GetComponent<Wood>())
                    {
                        woodCount++;
                    }
                    return;
                }
            }
        }
        
    }

    void OpenEquipmentTab()
    {
        backpackTab.SetActive(false);
        equipmentTab.SetActive(true);
        CloseTab(backpackTab, backpackTabButton, backpackButtonText);
        OpenTab(equipmentTab, equipmentTabButton, equipmentButtonText);
    }

    void OpenTab(GameObject panel, Button button, TextMeshProUGUI text)
    {
        ColorBlock colourBlock = button.colors;
        colourBlock.normalColor = Color.white;
        text.color = Color.black;
        button.colors = colourBlock;
    }

    void CloseTab(GameObject panel, Button button, TextMeshProUGUI text)
    {
        ColorBlock colourBlock = button.colors;
        colourBlock.normalColor = Color.black;
        text.color = Color.white;
        button.colors = colourBlock;
    }

    void OpenBackpackTab()
    {
        backpackTab.SetActive(true);
        equipmentTab.SetActive(false);
        ShowBackpackSlots();
        ShowToolbeltSlots();
        OpenTab(backpackTab, backpackTabButton, backpackButtonText);
        CloseTab(equipmentTab, equipmentTabButton, equipmentButtonText);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemIcon>() && dragItem == null)
        {
            dragItem = eventData.pointerCurrentRaycast.gameObject;
            dragItem.transform.SetParent(inventory.transform);
            dragItem.GetComponent<Image>().raycastTarget = false;
            if (dragItem.GetComponent<ItemIcon>().isInToolbelt)
            {
                originalSlot = toolBeltParent.GetChild(dragItem.GetComponent<ItemIcon>().slotIndex).gameObject;
                originalSlot.GetComponent<ItemSlot>().item = null;
            }
            else
            {
                originalSlot = slotsParent.GetChild(dragItem.GetComponent<ItemIcon>().slotIndex).gameObject;
                originalSlot.GetComponent<ItemSlot>().item = null;
            }

            dragItem.GetComponent<ItemIcon>().itemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().maxItemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().itemCountText.text = dragItem.GetComponent<ItemIcon>().itemCount.ToString();
            dragItem.GetComponent<ItemIcon>().ShowText();
            originalSlot.GetComponent<ItemSlot>().itemCount = 0;
            originalSlot.GetComponent<ItemSlot>().OnItemRemoved();
            
        }

    }

    void OnItemAddedToToolbelt(Item item, int index)
    {
        if(item.itemObject.TryGetComponent(out Tool toolComponent))
        {
            GameObject spawnedTool = Instantiate(item.itemObject, toolHoldSlot);
            spawnedTool.GetComponent<Tool>().player = gameObject;
            spawnedTool.transform.localPosition = toolComponent.holdPos;
            spawnedTool.transform.localEulerAngles = toolComponent.holdRot;
            if (spawnedTool.GetComponent<Collider>())
            {
                spawnedTool.GetComponent<Collider>().enabled = false;
            }
            toolBeltParent.GetChild(index).GetComponent<ToolbeltSlot>().activeItem = spawnedTool;
            SpawnItemObjectOnServerRpc(spawnedTool.GetComponent<Item>().id, NetworkManager.Singleton.LocalClientId);

        }

    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnItemObjectOnServerRpc(int itemId, ulong clientId)
    {
        var holder = ItemHolder.Instance;
        var playerObj = PlayerManager.Instance.GetClientPlayer(clientId);
        var camHolder = PlayerManager.Instance.GetClientHolder(clientId);
        var itemObj = holder.GetItemObjectFromId(itemId);

        GameObject item = Instantiate(itemObj, camHolder.transform);
        item.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        item.gameObject.SetActive(false);
        Debug.Log(item.name + " spawned for player " + clientId.ToString());
        
        EnableItemForOtherClientRpc(clientId, item.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    void EnableItemForOtherClientRpc(ulong clientId, ulong itemId)
    {
        if(NetworkManager.Singleton.LocalClientId != clientId)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
            {
                item.gameObject.SetActive(true);
                item.transform.SetParent(PlayerManager.Instance.GetClientHolder(clientId).transform);
                item.transform.localPosition = item.GetComponent<Tool>().holdPos;
                item.transform.localEulerAngles = item.GetComponent<Tool>().holdRot;
            }
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>() && dragItem != null)
        {
            ItemSlot slot = eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>();

            if(slot.transform.parent == toolBeltParent)
            {
                OnItemAddedToToolbelt(dragItem.GetComponent<ItemIcon>().item, slot.slotIndex);
            }

            bool val = slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
            int addingItemCount = (dragItem.GetComponent<ItemIcon>().itemCount + slot.itemCount)-1;
            if (val && addingItemCount <= dragItem.GetComponent<ItemIcon>().item.maxStackCount)
            {
                for (int i = 0; i < dragItem.GetComponent<ItemIcon>().itemCount-1; i++)
                {
                    slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
                }
            }
            else if(val && addingItemCount > dragItem.GetComponent<ItemIcon>().item.maxStackCount)
            {
                int difference = addingItemCount - dragItem.GetComponent<ItemIcon>().item.maxStackCount;
                for (int i = 0; i < (dragItem.GetComponent<ItemIcon>().itemCount - 1) - difference; i++)
                {
                    slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
                }
                GoToOriginalSlot(difference);
            }
            else if (!val)
            {
                GoToOriginalSlot(dragItem.GetComponent<ItemIcon>().itemCount);
            }

            dragItem.GetComponent<ItemIcon>().HideText();
            Destroy(dragItem);
            
        }
        else
        {
            GoToOriginalSlot(dragItem.GetComponent<ItemIcon>().itemCount);
        }
    }

    void GoToOriginalSlot(int itemCount)
    {
        for (int i = 0; i < itemCount; i++)
        {
            originalSlot.GetComponent<ItemSlot>().OnItemGained(dragItem.GetComponent<ItemIcon>().item);
        }
        dragItem.GetComponent<ItemIcon>().HideText();
        Destroy(dragItem);
    }

    public void OnSlotHoverEnter(ItemSlot slot)
    {
        hoverSlot = slot;
    }

    public void OnSlotHoverExit(ItemSlot slot)
    {
        if (hoverSlot == slot)
        {
            hoverSlot = null;
        }
    }
}
