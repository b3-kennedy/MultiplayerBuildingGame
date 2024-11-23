using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Item item;
    public TextMeshProUGUI numberText;
    GameObject spawnedIcon;
    public int itemCount;

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
            item = null;
            numberText.gameObject.SetActive(false);
            Destroy(spawnedIcon);
            itemCount = 0;
        }
    }

    public void OnItemGained(Item itemToGain)
    {

        if (item == null)
        {
            item = itemToGain;
            spawnedIcon = Instantiate(item.icon, transform);
            spawnedIcon.GetComponent<RectTransform>().localPosition = Vector3.zero;
            itemCount++;
            if (item.isStackable)
            {
                numberText.text = itemCount.ToString();
                numberText.gameObject.SetActive(true);

            }
        }
        else if(itemToGain == item && item.isStackable)
        {
            itemCount++;
            numberText.text = itemCount.ToString();
            numberText.gameObject.SetActive(true);
            
        }
        else
        {
            Debug.Log("Could not add new item");
        }

    }
}
