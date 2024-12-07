using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIPrefab : MonoBehaviour
{
    public GameObject materialReqPrefab;
    public Transform materialReqParent;

    public Image craftableItemIcon;
    public TextMeshProUGUI craftableItemName;

    [HideInInspector] public CraftingManager craftingManager;

    public Item craftableItem;

    [HideInInspector] public CraftingRecipe craftingRecipe;

    GameObject player;

    Button button;


    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CraftItem);
        player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
    }

    void CraftItem()
    {
        if (craftingManager.CanCraftItem(craftableItem))
        {

            
            foreach (var materialReq in craftingRecipe.itemsRequired)
            {
                for (int i = 0; i < materialReq.count; i++)
                {
                    player.GetComponent<InventoryManager>().RemoveItem(materialReq.item);
                }
            }
            craftingManager.OnOpenCraftingMenu();

            for (int i = 0; i < craftingRecipe.numberOfCraftedItem; i++)
            {
                player.GetComponent<InventoryManager>().AddItem(craftableItem);
            }

        }
        else
        {
            Debug.Log("Cant Craft");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
