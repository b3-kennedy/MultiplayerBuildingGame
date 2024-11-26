using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemIcon : MonoBehaviour
{

    public Item item;
    public int slotIndex;
    [HideInInspector] public int itemCount;
    [HideInInspector] public int maxItemCount;
    [HideInInspector] public TextMeshProUGUI itemCountText;
    public bool isInToolbelt;

    // Start is called before the first frame update
    void Start()
    {
        itemCountText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void ShowText()
    {
        itemCountText.gameObject.SetActive(true);
    }

    public void HideText()
    {
        itemCountText.gameObject.SetActive(false);
    }

}
