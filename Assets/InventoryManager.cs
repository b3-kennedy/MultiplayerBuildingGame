using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

    List<GameObject> visibleBackpackSlots = new List<GameObject>();
    List<GameObject> visibleToolbeltSlots = new List<GameObject>();

    public KeyCode inventoryKey = KeyCode.I;

    GameObject dragItem;
    GameObject originalSlot;

    ItemSlot hoverSlot;

    // Start is called before the first frame update
    void Start()
    {
        backpackTabButton.onClick.AddListener(OpenBackpackTab);
        equipmentTabButton.onClick.AddListener(OpenEquipmentTab);
        inventory.SetActive(false);
        OpenBackpackTab();

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            slotsParent.GetChild(i).GetComponent<ItemSlot>().slotIndex = i;
            slotsParent.GetChild(i).GetComponent<ItemSlot>().inventoryManager = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey) && inventory.activeSelf)
        {
            inventory.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            GetComponent<PlayerLook>().enabled = true;
        }
        else if(Input.GetKeyDown(inventoryKey) && !inventory.activeSelf)
        {
            inventory.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            GetComponent<PlayerLook>().enabled = false;
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
                if(slot.GetComponent<ItemSlot>().itemCount < item.maxStackCount)
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
        Debug.Log("Inventory Full");
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
            originalSlot = slotsParent.GetChild(dragItem.GetComponent<ItemIcon>().slotIndex).gameObject;
            originalSlot.GetComponent<ItemSlot>().item = null;
            dragItem.GetComponent<ItemIcon>().itemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().maxItemCount = originalSlot.GetComponent<ItemSlot>().itemCount;
            dragItem.GetComponent<ItemIcon>().itemCountText.text = dragItem.GetComponent<ItemIcon>().itemCount.ToString();
            dragItem.GetComponent<ItemIcon>().ShowText();
            originalSlot.GetComponent<ItemSlot>().itemCount = 0;
            originalSlot.GetComponent<ItemSlot>().OnItemRemoved();
            
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>() && dragItem != null)
        {
            ItemSlot slot = eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<ItemSlot>();
            bool val = slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
            int addingItemCount = (dragItem.GetComponent<ItemIcon>().itemCount + slot.itemCount)-1;
            if (val && addingItemCount <= dragItem.GetComponent<ItemIcon>().item.maxStackCount)
            {
                Debug.Log("1");
                for (int i = 0; i < dragItem.GetComponent<ItemIcon>().itemCount-1; i++)
                {
                    slot.OnItemGained(dragItem.GetComponent<ItemIcon>().item);
                }
            }
            else if(val && addingItemCount > dragItem.GetComponent<ItemIcon>().item.maxStackCount)
            {
                Debug.Log("2");
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
