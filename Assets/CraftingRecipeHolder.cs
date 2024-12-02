using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecipeHolder : MonoBehaviour
{

    public static CraftingRecipeHolder Instance;

    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

}
