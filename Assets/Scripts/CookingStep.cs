using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStep", menuName = "Cooking/CookingStep")]
public class CookingStep : ScriptableObject
{
    public List<Ingredient> inputIngredients;
    public Ingredient output;

    public string requiredStation;

    public void ProcessCookingStep(ref List<Ingredient> inventory, string currentStation)
    {
        if (currentStation != requiredStation) 
        { 
            Debug.LogWarning("Wrong station!");
            return;
        }

        foreach (var ingredient in inputIngredients)
        {
            if (!inventory.Contains(ingredient))
            {
                Debug.LogWarning("Don't have required ingredients!");
                return;
            }
        }

        //All ingredients are in inventory
        foreach(var ingredient in inputIngredients)
        {
            inventory.Remove(ingredient);
        }
        inventory.Add(output);

        Debug.Log("Created " + output.ingredientName);
        
    }
}
