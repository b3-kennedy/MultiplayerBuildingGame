using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public TextMeshProUGUI numberText;
    GameObject spawnedIcon;
    public int itemCount;
    public int slotIndex;
    [HideInInspector] public InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        numberText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void OnItemRemoved()
    {
        itemCount--;
        numberText.text = itemCount.ToString();
        if(itemCount <= 0)
        {
            if (item != null)
            {
                Destroy(spawnedIcon);
            }
            item = null;
            numberText.gameObject.SetActive(false);

            
            itemCount = 0;
        }
    }
    

    public bool OnItemGained(Item itemToGain)
    {
        if(itemCount < itemToGain.maxStackCount)
        {
            if (item == null)
            {
                if(itemToGain.itemObject != null)
                {
                    if (!itemToGain.itemObject.GetComponent<Weapon>() && transform.parent == inventoryManager.weaponSlotsParent)
                    {
                        return false;
                    }
                }

                item = itemToGain;

                spawnedIcon = Instantiate(item.icon, transform);
                spawnedIcon.GetComponent<RectTransform>().localPosition = Vector3.zero;
                spawnedIcon.GetComponent<ItemIcon>().slotIndex = slotIndex;
                if(transform.parent == inventoryManager.activeBelt)
                {
                    spawnedIcon.GetComponent<ItemIcon>().isInToolbelt = true;
                }
                itemCount++;
                if (item.isStackable)
                {
                    numberText.text = itemCount.ToString();
                    numberText.gameObject.SetActive(true);

                }
                return true;
            }
            else if (itemToGain == item && item.isStackable)
            {
                itemCount++;
                numberText.text = itemCount.ToString();
                numberText.gameObject.SetActive(true);
                return true;

            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryManager.OnSlotHoverEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryManager.OnSlotHoverExit(this);
    }
}
