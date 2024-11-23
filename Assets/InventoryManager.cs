using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
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



    // Start is called before the first frame update
    void Start()
    {
        backpackTabButton.onClick.AddListener(OpenBackpackTab);
        equipmentTabButton.onClick.AddListener(OpenEquipmentTab);
        inventory.SetActive(false);
        OpenBackpackTab();
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
}
