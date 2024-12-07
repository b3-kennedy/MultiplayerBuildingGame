using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{

    InventoryManager inventoryManager;

    public GameObject craftingPrefab;
    public Transform craftingParent;

    Dictionary<Item, List<CraftingRecipe>> itemToRecipes = new Dictionary<Item, List<CraftingRecipe>>();
    Dictionary<Item, List<CraftingRecipe>> craftableItemToRecipe = new Dictionary<Item, List<CraftingRecipe>>();


    public List<Item> oldList = new List<Item>();
    public List<Item> craftableItems = new List<Item>();

    public List<ItemAndCount> inventoryItems;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
        foreach (var recipe in CraftingRecipeHolder.Instance.recipes)
        {
            foreach (var requiredItem in recipe.itemsRequired)
            {
                if (!itemToRecipes.ContainsKey(requiredItem.item))
                {
                    itemToRecipes[requiredItem.item] = new List<CraftingRecipe>();
                }
                itemToRecipes[requiredItem.item].Add(recipe);
            }
        }

        foreach (var recipe in CraftingRecipeHolder.Instance.recipes)
        {
            if (!craftableItemToRecipe.ContainsKey(recipe.craftedItem))
            {
                craftableItemToRecipe[recipe.craftedItem] = new List<CraftingRecipe>();
            }
            craftableItemToRecipe[recipe.craftedItem].Add(recipe);
        }

        DebugDictionary();
    }

    void DestroyCraftingPrefabs()
    {
        for (int i = 0; i < craftingParent.childCount; i++)
        {
            Destroy(craftingParent.GetChild(i).gameObject);
        }
    }


    public void GetRecipes()
    {
        inventoryItems.Clear();
        DestroyCraftingPrefabs();
        craftableItems.Clear();
        foreach (var slot in inventoryManager.visibleBackpackSlots)
        {
            var item = slot.GetComponent<ItemSlot>().item;
            var count = slot.GetComponent<ItemSlot>().itemCount;
            if (item != null)
            {
                var existingItem = inventoryItems.Find(pair => pair.item.gameObject == item.gameObject);

                if (existingItem == null)
                {
                    // If item doesn't exist, add it
                    inventoryItems.Add(new ItemAndCount { item = item, count = count });
                }
                else
                {
                    // If item exists, update the count by removing the old entry and adding the updated one
                    existingItem.count += count;
                }
            }
        }

        foreach (var slot in inventoryManager.visibleBackpackSlots)
        {
            var item = slot.GetComponent<ItemSlot>().item;
            if(item != null)
            {
                //iterate through each non empty slot and debug recipes based on the items found
                if (itemToRecipes.TryGetValue(item, out var recipes))
                {
                    foreach (var craftableItem in recipes)
                    {
                        
                        if (!craftableItems.Contains(craftableItem.craftedItem))
                        {
                            craftableItems.Add(craftableItem.craftedItem);
                            GameObject craft = Instantiate(craftingPrefab, craftingParent);
                            CraftingUIPrefab craftUI = craft.GetComponent<CraftingUIPrefab>();

                            craftUI.craftableItemIcon.sprite = craftableItem.craftedItem.icon.GetComponent<Image>().sprite;
                            craftUI.craftableItemName.text = craftableItem.itemName + " x" + craftableItem.numberOfCraftedItem;
                            craftUI.craftableItem = craftableItem.craftedItem;
                            craftUI.craftingManager = this;
                            craftUI.craftingRecipe = craftableItem;


                            foreach (var matReqItem in craftableItem.itemsRequired)
                            {

                                GameObject materialReq = Instantiate(craftUI.materialReqPrefab, craftUI.materialReqParent);
                                materialReq.transform.GetChild(0).GetComponent<Image>().sprite = matReqItem.item.icon.GetComponent<Image>().sprite;

                                materialReq.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    GetItemCount(matReqItem.item).ToString() + "/" + matReqItem.count.ToString();

                                if(GetItemCount(matReqItem.item) >= matReqItem.count)
                                {
                                    materialReq.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.green;
                                }
                                else if(GetItemCount(matReqItem.item) < matReqItem.count)
                                {
                                    materialReq.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.red;
                                }
                            }
                        }
                    }


                    Debug.Log($"Available recipes for {item.name}: {string.Join(", ", recipes.Select(r => r.name))}");
                }
                else
                {
                    Debug.Log($"No recipes available for {item.name}.");
                }
            }

        }

    }

    int GetItemCount(Item item)
    {
        int count = 0;
        foreach (var i in inventoryItems)
        {
            if(i.item == item)
            {
                count = i.count;
            }
        }

        //for (int i = 0; i < inventoryItems.Count; i++)
        //{
        //    Debug.Log(inventoryItems[i].item);
        //    if (inventoryItems[i].item == item)
        //    {
        //        return inventoryItems[i].count;
        //    }
        //}
        return count;
    }

    void CountResourcesForRecipes()
    {
        // Check if the craftable items can actually be crafted
        foreach (var craftableItem in craftableItems)
        {
            bool canCraft = CanCraftItem(craftableItem);
            if (canCraft)
            {
                Debug.Log($"You can craft: {craftableItem.name}");
            }
            else
            {
                Debug.Log($"Not enough items to craft: {craftableItem.name}");
            }
        }
    }

    void DisplayNewRecipes()
    {
        var sortedCraftableItems = craftableItems.OrderBy(item => item.name).ToList();
        var sortedOldList = oldList.OrderBy(item => item.name).ToList();

        if (!sortedCraftableItems.SequenceEqual(sortedOldList))
        {
            Debug.Log("New Craftable Items Available");

            var newItems = sortedCraftableItems.Except(sortedOldList).ToList();

            if (newItems.Count > 0)
            {
                Debug.Log($"New craftable items: {string.Join(", ", newItems.Select(item => item.name))}");
            }

            oldList = new List<Item>(craftableItems);
        }
    }

    public void OnOpenCraftingMenu()
    {
        
        GetRecipes();
        DisplayNewRecipes();
        CountResourcesForRecipes();
    }


    public bool CanCraftItem(Item itemToCraft)
    {
        // Get the recipes for the item
        if (craftableItemToRecipe.TryGetValue(itemToCraft, out var recipes))
        {

            foreach (var recipe in recipes)
            {
                Debug.Log(recipe);

                bool canCraft = true;

                // Check if we have enough of each required item
                foreach (var requiredItem in recipe.itemsRequired)
                {
                    // Find the item in the inventory
                    var inventoryItem = inventoryItems.Find(pair => pair.item == requiredItem.item);

                    // If the item is not in inventory or not enough count, return false
                    if (inventoryItem == null || inventoryItem.count < requiredItem.count)
                    {
                        canCraft = false;
                        break;
                    }
                }

                // If we have enough of all required items for at least one recipe, return true
                if (canCraft)
                {
                    return true;
                }
            }
        }

        // If no recipes work, return false
        return false;
    }

    public void DebugDictionary()
    {
        foreach (var entry in itemToRecipes)
        {
            // Log the item name
            string itemName = entry.Key.name; // Assuming Item has a 'name' property
            string recipes = string.Join(", ", entry.Value.Select(recipe => recipe.craftedItem)); // Get recipe names

            Debug.Log($"Item: {itemName} is required for recipes: {recipes}");
        }

        Debug.Log("Required Items for Recipes:");
        foreach (var entry in craftableItemToRecipe)
        {
            // For each recipe that can craft this item, list the required items
            string requiredItems = string.Join(", ", entry.Value
                .SelectMany(recipe => recipe.itemsRequired) // Flatten the required items for each recipe
                .Select(requiredItem => requiredItem.item.name) // Get the name of the item
            );
            Debug.Log($"Required Item: {entry.Key.name} is crafted using: {requiredItems}");
        }
    }


}
