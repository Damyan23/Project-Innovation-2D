using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeDataBase : MonoBehaviour
{
    [HideInInspector] public List<Recipe> recipes = new List<Recipe>(); // Auto-populated list
    private void Start()
    {
        LoadAllRecipes();
    }

    private void LoadAllRecipes()
    {
        Recipe[] recipes = Resources.LoadAll<Recipe>("Recipes");

        foreach (var recipe in recipes)
        {
            this.recipes.Add(recipe);
        }
    }
}
