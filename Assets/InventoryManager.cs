using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class InventoryManager : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public float woodCount;
    public float stoneCount;

    [Header("UI Control")]

    public GameObject inventory;
    public GameObject chestInterface;

    public Button backpackTabButton;
    public Button equipmentTabButton;
    public Button craftingTabButton;
    public Button chestTabButton;

    public GameObject backpackTab;
    public GameObject equipmentTab;
    public GameObject craftingTab;

    public TextMeshProUGUI backpackButtonText;
    public TextMeshProUGUI equipmentButtonText;
    public TextMeshProUGUI craftingButtonText;

    [Header("Toolbelt Control")]

    public Backpack backpackSlot;
    public Backpack toolbeltSlot;
    public Backpack weaponSlot;

    public Transform toolBeltParent;
    public Transform slotsParent;
    public Transform weaponSlotsParent;

    public Transform toolBeltInvPos;
    public Transform toolBeltHudPos;

    public Transform dropPoint;

    [HideInInspector] public Transform activeParent;

    [HideInInspector] public List<GameObject> visibleBackpackSlots = new List<GameObject>();
    List<GameObject> visibleToolbeltSlots = new List<GameObject>();
    List<GameObject> visibleWeaponSlots = new List<GameObject>();

    GameObject spawnedTool;


    public KeyCode inventoryKey = KeyCode.I;

    GameObject dragItem;
    [HideInInspector] public GameObject originalSlot;

    ItemSlot hoverSlot;
    int currentSelectedIndex = 0;

    Transform currentSelectedToolbeltSlot;

    public Transform activeBelt;

    PlayerInterfaceManager playerInterfaceManager;
    BuildingManager buildingManager;
    Transform toolHoldSlot;
    CraftingManager craftingManager;
    InteractionManager interactionManager;

    // Start is called before the first frame update
    void Start()
    {
        interactionManager = GetComponent<InteractionManager>();
        craftingManager = GetComponent<CraftingManager>();
        buildingManager = GetComponent<BuildingManager>();
        playerInterfaceManager = GetComponent<PlayerInterfaceManager>();
        
        backpackTabButton.onClick.AddListener(OpenBackpackTab);
        equipmentTabButton.onClick.AddListener(OpenEquipmentTab);
        craftingTabButton.onClick.AddListener(OpenCraftingTab);

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

        for (int i = 0; i < weaponSlotsParent.childCount; i++)
        {
            weaponSlotsParent.GetChild(i).GetComponent<ItemSlot>().slotIndex = i;
            weaponSlotsParent.GetChild(i).GetComponent<ItemSlot>().inventoryManager = this;
        }

        toolBeltParent.SetParent(inventory.transform.parent);
        toolBeltParent.position = toolBeltHudPos.position;


        weaponSlotsParent.SetParent(inventory.transform.parent);
        weaponSlotsParent.position = toolBeltHudPos.position;
        activeBelt = toolBeltParent;
    }

    private void Update()
    {

        if(toolHoldSlot == null)
        {
            toolHoldSlot = PlayerManager.Instance.GetClientHolder(NetworkManager.Singleton.LocalClientId).transform;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            var slot = toolBeltParent.GetChild(currentSelectedIndex).GetComponent<ToolbeltSlot>();
            if (slot.activeItem.GetComponent<Weapon>())
            {
                Debug.Log(slot.activeItem + " has been added to the weapon slots");
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && toolBeltParent.gameObject.activeSelf)
        {
            toolBeltParent.gameObject.SetActive(false);
            weaponSlotsParent.gameObject.SetActive(true);
            for (int i = 0; i < toolBeltParent.childCount; i++)
            {
                if(toolBeltParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    toolBeltParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem.SetActive(false);
                    EnableOrDisableToolbeltItemServerRpc(false, toolBeltParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem.GetComponent<NetworkObject>().NetworkObjectId,
                        NetworkManager.Singleton.LocalClientId, i);
                }
            }
            activeBelt = weaponSlotsParent;
        }
        else if (Input.GetKeyDown(KeyCode.LeftAlt) && !toolBeltParent.gameObject.activeSelf)
        {
            activeBelt = toolBeltParent;
            toolBeltParent.gameObject.SetActive(true);
            weaponSlotsParent.gameObject.SetActive(false);
            for (int i = 0; i < weaponSlotsParent.childCount; i++)
            {
                if (weaponSlotsParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    weaponSlotsParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem.SetActive(false);
                    EnableOrDisableToolbeltItemServerRpc(false, weaponSlotsParent.GetChild(i).GetComponent<ToolbeltSlot>().activeItem.GetComponent<NetworkObject>().NetworkObjectId,
                        NetworkManager.Singleton.LocalClientId, i);
                }
            }
        }


        if (!interactionManager.isInChest)
        {
            if (Input.GetKeyDown(inventoryKey) && inventory.activeSelf)
            {
                
                toolBeltParent.SetParent(inventory.transform.parent);
                toolBeltParent.transform.localPosition = toolBeltHudPos.localPosition;
                weaponSlotsParent.SetParent(inventory.transform.parent);
                weaponSlotsParent.transform.localPosition = toolBeltHudPos.localPosition;
                inventory.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                GetComponent<PlayerLook>().enabled = true;
                OpenBackpackTab();
            }
            else if (Input.GetKeyDown(inventoryKey) && !inventory.activeSelf)
            {
                activeParent = inventory.transform;
                activeBelt.gameObject.SetActive(true);
                toolBeltParent.SetParent(backpackTab.transform);
                toolBeltParent.transform.localPosition = toolBeltInvPos.localPosition;
                weaponSlotsParent.SetParent(backpackTab.transform);
                weaponSlotsParent.transform.localPosition = toolBeltInvPos.localPosition;
                inventory.SetActive(true);
                currentSelectedIndex = -1;
                for (int i = 0; i < toolBeltParent.childCount; i++)
                {
                    toolBeltParent.GetChild(i).GetChild(2).gameObject.SetActive(false);
                }
                Cursor.lockState = CursorLockMode.None;
                GetComponent<PlayerLook>().enabled = false;
            }
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

        if(Input.GetKeyDown(KeyCode.Q) && currentSelectedToolbeltSlot != null)
        {
            if(currentSelectedToolbeltSlot.gameObject.GetComponent<ToolbeltSlot>().activeItem != null)
            {
                if(buildingManager.mode == BuildingManager.Mode.BUILD)
                {
                    buildingManager.mode = BuildingManager.Mode.NORMAL;
                }
                DropItemServerRpc(currentSelectedToolbeltSlot.gameObject.GetComponent<ToolbeltSlot>().activeItem.GetComponent<Item>().id, 1);
                currentSelectedToolbeltSlot.GetComponent<ItemSlot>().OnItemRemoved();
            }
        }

    }

    void ToolbeltSlotSelection()
    {
        if (buildingManager.mode == BuildingManager.Mode.NORMAL)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            int totalSlots = activeBelt.childCount;

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
                while (!activeBelt.GetChild(currentSelectedIndex).gameObject.activeSelf);

                SelectToolSlot(activeBelt.GetChild(currentSelectedIndex).gameObject, currentSelectedIndex);
            }

            for (int i = 0; i < activeBelt.childCount; i++)
            {
                if (activeBelt.GetChild(i).gameObject.activeSelf)
                {
                    // Check if any key from 1 to 9 is pressed
                    for (int key = 1; key <= 9; key++)
                    {
                        if (Input.GetKeyDown(key.ToString()))
                        {
                            int inputIndex = key - 1; // Map key 1-9 to index 0-8

                            if (i == inputIndex) // Match index with active slot
                            {
                                SelectToolSlot(activeBelt.GetChild(i).gameObject, i);
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
        for (int i = 0; i < activeBelt.childCount; i++)
        {
            GameObject slot = activeBelt.GetChild(i).gameObject;

            if (i == selectedIndex)
            {
                //activate selection graphic
                slot.transform.GetChild(2).gameObject.SetActive(true);
                currentSelectedToolbeltSlot = slot.transform;

                //activate item
                if (slot.GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    slot.GetComponent<ToolbeltSlot>().activeItem.SetActive(true);
                    slot.GetComponent<ToolbeltSlot>().activeItem.GetComponent<Collider>().enabled = false;
                    EnableOrDisableToolbeltItemServerRpc(true, slot.GetComponent<ToolbeltSlot>().activeItem.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId, i);
                }
            }
            else
            {
                // Deactivate the 2nd child for all other slots
                slot.transform.GetChild(2).gameObject.SetActive(false);
                if (slot.GetComponent<ToolbeltSlot>().activeItem != null)
                {
                    slot.GetComponent<ToolbeltSlot>().activeItem.SetActive(false);
                    EnableOrDisableToolbeltItemServerRpc(false, slot.GetComponent<ToolbeltSlot>().activeItem.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId, i);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void EnableOrDisableToolbeltItemServerRpc(bool isEnable, ulong itemId, ulong clientId, int slotIndex)
    {
        EnableOrDisableToolbeltItemClientRpc(isEnable, itemId, clientId, slotIndex);
    }

    [ClientRpc]
    void EnableOrDisableToolbeltItemClientRpc(bool isEnable, ulong itemId, ulong clientId, int slotIndex)
    {

        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        {
            if (isEnable)
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
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

    void ShowWeaponSlots()
    {
        visibleWeaponSlots.Clear();
        if (weaponSlot != null)
        {
            for (int i = 0; i < weaponSlot.slotCount; i++)
            {
                weaponSlotsParent.GetChild(i).gameObject.SetActive(true);
                visibleWeaponSlots.Add(weaponSlotsParent.GetChild(i).gameObject);

            }
        }
        else
        {
            for (int i = 0; i < weaponSlotsParent.childCount; i++)
            {
                weaponSlotsParent.GetChild(i).gameObject.SetActive(false);

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
                return;
            }
        }
    }

    public void AddItem(Item item)
    {
        if(visibleBackpackSlots.Count > 0)
        {
            foreach (var slot in visibleBackpackSlots)
            {
                var itemSlot = slot.GetComponent<ItemSlot>();
                if (itemSlot.item == item && itemSlot.itemCount < item.maxStackCount)
                {
                    slot.GetComponent<ItemSlot>().OnItemGained(item);
                    CountMaterials(item);
                    return;
                }

            }

            foreach (var slot in visibleBackpackSlots)
            {
                var itemSlot = slot.GetComponent<ItemSlot>();

                if (itemSlot.item == null)
                {
                    itemSlot.OnItemGained(item);
                    CountMaterials(item);
                    return;
                }
            }
        }
        
    }


    void CountMaterials(Item item)
    {
        if (item.GetComponent<Wood>())
        {
            woodCount++;
        }
        else if (item.GetComponent<Stone>())
        {
            stoneCount++;
        }
    }

    void OpenEquipmentTab()
    {
        craftingTab.SetActive(false);
        backpackTab.SetActive(false);
        equipmentTab.SetActive(true);
        OpenTab(equipmentTab, equipmentTabButton, equipmentButtonText);
        CloseTab(backpackTab, backpackTabButton, backpackButtonText);
        CloseTab(craftingTab, craftingTabButton, craftingButtonText);
    }

    void OpenCraftingTab()
    {
        craftingTab.SetActive(true);
        backpackTab.SetActive(false);
        equipmentTab.SetActive(false);
        craftingManager.OnOpenCraftingMenu();
        OpenTab(craftingTab, craftingTabButton, craftingButtonText);
        CloseTab(backpackTab, backpackTabButton, backpackButtonText);
        CloseTab(equipmentTab, equipmentTabButton, equipmentButtonText);
    }

    public void OpenBackpackTab()
    {
        backpackTab.SetActive(true);
        equipmentTab.SetActive(false);
        craftingTab.SetActive(false);
        ShowBackpackSlots();
        ShowToolbeltSlots();
        ShowWeaponSlots();
        OpenTab(backpackTab, backpackTabButton, backpackButtonText);
        CloseTab(equipmentTab, equipmentTabButton, equipmentButtonText);
        CloseTab(craftingTab, craftingTabButton, craftingButtonText);
    }

    public void OpenTab(GameObject panel, Button button, TextMeshProUGUI text)
    {
        ColorBlock colourBlock = button.colors;
        colourBlock.normalColor = Color.white;
        text.color = Color.black;
        button.colors = colourBlock;
    }

    public void CloseTab(GameObject panel, Button button, TextMeshProUGUI text)
    {
        ColorBlock colourBlock = button.colors;
        colourBlock.normalColor = Color.black;
        text.color = Color.white;
        button.colors = colourBlock;
    }


    void Drag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemIcon>() && dragItem == null)
        {
            dragItem = eventData.pointerCurrentRaycast.gameObject;
            dragItem.transform.SetParent(activeParent.transform);
            dragItem.GetComponent<Image>().raycastTarget = false;
            if (dragItem.GetComponent<ItemIcon>().isInToolbelt)
            {
                originalSlot = activeBelt.GetChild(dragItem.GetComponent<ItemIcon>().slotIndex).gameObject;
                originalSlot.GetComponent<ItemSlot>().item = null;
            }
            else
            {
                if (!interactionManager.chestTab.activeSelf)
                {
                    originalSlot = slotsParent.GetChild(dragItem.GetComponent<ItemIcon>().slotIndex).gameObject;
                    originalSlot.GetComponent<ItemSlot>().item = null;
                }
                else
                {
                    originalSlot = interactionManager.chest.visibleSlots[dragItem.GetComponent<ItemIcon>().slotIndex];
                    originalSlot.GetComponent<ItemSlot>().item = null;
                }

            }

            

            dragItem.GetComponent<ItemIcon>().itemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().maxItemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().itemCountText.text = dragItem.GetComponent<ItemIcon>().itemCount.ToString();
            dragItem.GetComponent<ItemIcon>().ShowText();
            originalSlot.GetComponent<ItemSlot>().itemCount = 0;
            originalSlot.GetComponent<ItemSlot>().OnItemRemoved();

        }
    }

    void MoveToToolbelt(PointerEventData eventData, Item item, int count)
    {
        if(activeBelt == toolBeltParent)
        {
            foreach (var slot in visibleToolbeltSlots)
            {
                if (slot.GetComponent<ToolbeltSlot>().activeItem == null)
                {
                    if (item.itemObject != null)
                    {
                        SpawnItemObjectOnServerRpc(item.itemObject.GetComponent<Item>().id, NetworkManager.Singleton.LocalClientId, slot.GetComponent<ItemSlot>().slotIndex);
                        for (int i = 0; i < count; i++)
                        {
                            Debug.Log(item);
                            slot.GetComponent<ItemSlot>().OnItemGained(item);
                        }
                    }

                    return;
                }
            }
        }
        else if(activeBelt == weaponSlotsParent)
        {
            foreach (var slot in visibleWeaponSlots)
            {
                if (slot.GetComponent<ToolbeltSlot>().activeItem == null)
                {
                    if (item.itemObject != null)
                    {
                        SpawnItemObjectOnServerRpc(item.itemObject.GetComponent<Item>().id, NetworkManager.Singleton.LocalClientId, slot.GetComponent<ItemSlot>().slotIndex);
                        for (int i = 0; i < count; i++)
                        {
                            Debug.Log(item);
                            slot.GetComponent<ItemSlot>().OnItemGained(item);
                        }
                    }

                    return;
                }
            }
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                Drag(eventData);
                break;
            case PointerEventData.InputButton.Right:
                if (!interactionManager.isInChest)
                {
                    var item = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemIcon>();

                    if (item != null && !item.isInToolbelt)
                    {

                        int count = visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount;
                        if(activeBelt == weaponSlotsParent && !item.item.itemObject.GetComponent<Weapon>())
                        {

                            Debug.Log("Cannot add non weapon object to weapon slots");
                        }
                        else
                        {
                            MoveToToolbelt(eventData, item.item, count);
                            visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount = 0;
                            visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().OnItemRemoved();
                            Destroy(item.gameObject);
                        }

                    }
                    else if (item != null && item.isInToolbelt)
                    {
                        Debug.Log("in toolbelt");
                        if(activeBelt == toolBeltParent)
                        {
                            int count = visibleToolbeltSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount;
                            for (int i = 0; i < count; i++)
                            {
                                AddItem(item.item);
                            }
                            visibleToolbeltSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount = 0;
                            visibleToolbeltSlots[item.slotIndex].GetComponent<ItemSlot>().OnItemRemoved();
                        }
                        else if(activeBelt == weaponSlotsParent)
                        {
                            int count = visibleWeaponSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount;
                            for (int i = 0; i < count; i++)
                            {
                                AddItem(item.item);
                            }
                            visibleWeaponSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount = 0;
                            visibleWeaponSlots[item.slotIndex].GetComponent<ItemSlot>().OnItemRemoved();
                        }


                    }
                }
                else
                {
                    var item = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemIcon>();

                    if (!interactionManager.chestTab.activeSelf)
                    {
                        if (interactionManager.chest != null)
                        {
                            Debug.Log("Added item with id of " + item.item.id + " to " + interactionManager.chest);
                            for (int i = 0; i < visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount; i++)
                            {
                                interactionManager.chest.AddItem(item.item);
                            }
                            visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount = 0;
                            visibleBackpackSlots[item.slotIndex].GetComponent<ItemSlot>().OnItemRemoved();

                            
                        }
                    }
                    else
                    {
                        for (int i = 0; i < interactionManager.chest.visibleSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount; i++)
                        {
                            AddItem(item.item);
                        }
                        interactionManager.chest.visibleSlots[item.slotIndex].GetComponent<ItemSlot>().itemCount = 0;
                        interactionManager.chest.visibleSlots[item.slotIndex].GetComponent<ItemSlot>().OnItemRemoved();


                        
                    }

                }
                break;


        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemsFromChestServerRpc(int slotIndex, ulong chestId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(chestId, out var chestObj))
        {
            chestObj.GetComponent<Chest>().serverSlots[slotIndex].hasItem.Value = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateServerSlotsServerRpc(int index, int itemId, int count, ulong chestId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(chestId, out var chestObj))
        {
            chestObj.GetComponent<Chest>().serverSlots[index].hasItem.Value = true;
            chestObj.GetComponent<Chest>().serverSlots[index].itemId.Value = itemId;
            chestObj.GetComponent<Chest>().serverSlots[index].itemCount.Value = count;
        }

    }

    void OnItemAddedToToolbelt(Item item, int index)
    {
        if(item.itemObject.TryGetComponent(out Tool toolComponent))
        {
            spawnedTool = Instantiate(item.itemObject, toolHoldSlot);
            if (spawnedTool.GetComponent<Rigidbody>())
            {
                spawnedTool.GetComponent<Rigidbody>().isKinematic = true;
            }
            
            var oldName = spawnedTool.name;
            spawnedTool.name = "local" + oldName;
            spawnedTool.SetActive(false);
            spawnedTool.GetComponent<Tool>().player = gameObject;
            spawnedTool.transform.localPosition = toolComponent.holdPos;
            spawnedTool.transform.localEulerAngles = toolComponent.holdRot;
            Debug.Log(toolComponent.holdPos);
            if (spawnedTool.GetComponent<Collider>())
            {
                spawnedTool.GetComponent<Collider>().enabled = false;
            }
            activeBelt.GetChild(index).GetComponent<ToolbeltSlot>().activeItem = spawnedTool;
            SpawnItemObjectOnServerRpc(spawnedTool.GetComponent<Item>().id, NetworkManager.Singleton.LocalClientId, index);

        }

    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnItemObjectOnServerRpc(int itemId, ulong clientId, int index)
    {
        var holder = ItemHolder.Instance;
        var playerObj = PlayerManager.Instance.GetClientPlayer(clientId);
        var camHolder = PlayerManager.Instance.GetClientHolder(clientId);
        var itemObj = holder.GetItemObjectFromId(itemId);

        GameObject item = Instantiate(itemObj, camHolder.transform);
        if (item.GetComponent<Rigidbody>())
        {
            item.GetComponent<Rigidbody>().isKinematic = true;
        }
        
        var oldName = item.name;
        item.name = "server" + oldName;
        item.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        item.GetComponent<Collider>().enabled = false;
        if (item.GetComponent<Tool>())
        {
            item.GetComponent<Tool>().player = playerObj;
            item.GetComponent<Tool>().enabled = true;
        }

        item.transform.SetParent(PlayerManager.Instance.GetClientHolder(clientId).transform);
        item.gameObject.SetActive(false);
        Debug.Log(item.name + " spawned for player " + clientId.ToString());
        
        EnableItemForOtherClientRpc(clientId, item.GetComponent<NetworkObject>().NetworkObjectId, index);
    }

    [ClientRpc]
    void EnableItemForOtherClientRpc(ulong clientId, ulong itemId, int index)
    {
        if(NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var tool))
            {
                activeBelt.GetChild(index).GetComponent<ToolbeltSlot>().activeItem = tool.gameObject;
            }
                
            Destroy(spawnedTool);
        }
        
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        {
            if (item.GetComponent<Tool>())
            {
                item.GetComponent<Tool>().enabled = true;
            }
            item.gameObject.SetActive(false);
            item.transform.localPosition = item.GetComponent<Tool>().holdPos;
            item.transform.localEulerAngles = item.GetComponent<Tool>().holdRot;
            if (item.GetComponent<Rigidbody>())
            {
                item.GetComponent<Rigidbody>().isKinematic = true;
            }
            
        }
        //if(NetworkManager.Singleton.LocalClientId != clientId)
        //{
        //    if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        //    {
        //        item.gameObject.SetActive(true);
        //        item.transform.localPosition = item.GetComponent<Tool>().holdPos;
        //        item.transform.localEulerAngles = item.GetComponent<Tool>().holdRot;
        //    }
        //}
        //else
        //{
        //    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        //    {
        //        item.gameObject.SetActive(false);
        //    }
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemOnServerRpc(ulong itemId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        {
            item.GetComponent<NetworkObject>().Despawn();
        }
        
    }


    [ServerRpc(RequireOwnership = false)]
    void DropItemServerRpc(int itemId, int itemCount)
    {
        GameObject item = ItemHolder.Instance.GetItemObjectFromId(itemId);
        GameObject spawnedItem = Instantiate(item, dropPoint.transform.position, Quaternion.identity);
        var count = spawnedItem.GetComponent<ItemCount>();
        spawnedItem.GetComponent<NetworkObject>().Spawn();
        count.itemCount.Value = itemCount;
        EnableComponentsAfterDropClientRpc(spawnedItem.GetComponent<NetworkObject>().NetworkObjectId);
        Debug.Log(item);
    }

    [ClientRpc]
    void EnableComponentsAfterDropClientRpc(ulong itemId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out var item))
        {
            if (item.GetComponent<Collider>())
            {
                item.GetComponent<Collider>().enabled = true;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null)
            {
                if (buildingManager.mode == BuildingManager.Mode.BUILD)
                {
                    buildingManager.mode = BuildingManager.Mode.NORMAL;
                }
                DropItemServerRpc(dragItem.GetComponent<ItemIcon>().item.itemObject.GetComponent<Item>().id, dragItem.GetComponent<ItemIcon>().itemCount);
                Destroy(dragItem);
            }
            else if (eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>() && dragItem != null)
            {
                ItemSlot slot = eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>();

                bool val = slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);

                if (val && slot.transform.parent == activeBelt)
                {
                    OnItemAddedToToolbelt(dragItem.GetComponent<ItemIcon>().item, slot.slotIndex);
                }

                
                int addingItemCount = (dragItem.GetComponent<ItemIcon>().itemCount + slot.itemCount) - 1;
                if (val && addingItemCount <= dragItem.GetComponent<ItemIcon>().item.maxStackCount)
                {
                    for (int i = 0; i < dragItem.GetComponent<ItemIcon>().itemCount - 1; i++)
                    {
                        slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
                    }
                }
                else if (val && addingItemCount > dragItem.GetComponent<ItemIcon>().item.maxStackCount)
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
            else if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                GoToOriginalSlot(dragItem.GetComponent<ItemIcon>().itemCount);
            }
        }
       
    }

    void GoToOriginalSlot(int itemCount)
    {
        if(dragItem != null)
        {
            for (int i = 0; i < itemCount; i++)
            {
                originalSlot.GetComponent<ItemSlot>().OnItemGained(dragItem.GetComponent<ItemIcon>().item);
            }
            dragItem.GetComponent<ItemIcon>().HideText();
            Destroy(dragItem);
        }

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
