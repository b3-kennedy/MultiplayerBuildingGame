using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemAndCount
{
    public Item item;
    public int count;
}

[CreateAssetMenu(fileName = "Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string itemName;
    public List<ItemAndCount> itemsRequired = new List<ItemAndCount>();
    public Item craftedItem;
}
