using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CookingTester : MonoBehaviour
{
    [Header("Debug Stuff")]
    public List<Ingredient> startingInventory;
    public List<Ingredient> inventory;
    public List<Ingredient> selectedIngredients;
    public string currentStation = "knife";

    [Header("Prefabs")]
    public GameObject cookingStepPrefab;
    public GameObject inventoryItemPrefab;

    [Header("References")]
    public Transform canvas;
    public Transform inventoryParent;

    List<GameObject> inventoryItems;


    void Start()
    {
        inventoryItems = new();
        selectedIngredients = new();
        DisplayInventory();
    }


    void Update()
    {
        //Execute a cooking step (if possible)
        if (Input.GetKeyDown(KeyCode.F))
        {
            List<CookingStep> available = GetAvailableSteps(selectedIngredients, currentStation);
            
            foreach(CookingStep step in available)
            {
                if(IsValidRecipe(selectedIngredients, step))
                {
                    step.ProcessCookingStep(this, currentStation);
                    selectedIngredients.Clear();
                    return;
                }
            }

            Debug.LogWarning("Nothing Processed");

            foreach(Ingredient ingredient in selectedIngredients)
            {
                AddIngredient(ingredient);
            }

            selectedIngredients.Clear();
           
        }
    }

    List<CookingStep> GetAvailableSteps(List<Ingredient> inventory, string station)
    {
        CookingStep[] allSteps = Resources.LoadAll<CookingStep>("CookingSteps");

        List<CookingStep> availableSteps = new List<CookingStep>();

        foreach (CookingStep step in allSteps)
        {
            if (step.requiredStation == currentStation)
            {
                if(CheckRequiredIngedients(inventory, step))
                {
                    availableSteps.Add(step);
                }
            }
        }

        return availableSteps;
    }

    bool CheckRequiredIngedients(List<Ingredient> inventory, CookingStep step)
    {
        foreach(Ingredient ingredient in step.inputIngredients)
        {
            if (!inventory.Contains(ingredient)) return false;
        }

        return true;
    }

    void DisplayInventory()
    { 

        foreach(Ingredient ingredient in startingInventory)
        {
            AddIngredient(ingredient);
        }        
    }

    public void AddIngredient(Ingredient ingredient)
    {
        GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryParent);
        inventoryItem.GetComponent<Image>().sprite = ingredient.icon;
        inventoryItem.GetComponent<InventoryItem>().ingredient = ingredient;
        inventoryItem.GetComponent<InventoryItem>().testerScript = this;
        inventoryItems.Add(inventoryItem);
        inventory.Add(ingredient);
    }

    public void RemoveIngredient(Ingredient ingredient)
    {
        inventory.Remove(ingredient);

        foreach(GameObject item in inventoryItems)
        {
            if(item.GetComponent<InventoryItem>().ingredient == ingredient)
            {
                inventoryItems.Remove(item);
                Destroy(item);
                return;
            }
        }
    }

    public void SelectInventoryItem(InventoryItem item)
    {
        RemoveIngredient(item.ingredient);
        selectedIngredients.Add(item.ingredient);
    }


    //Dont look at this function tbh
    bool IsValidRecipe(List<Ingredient> ingredients, CookingStep step)
    {
        if (ingredients.Count == step.inputIngredients.Count)
        {
            for(int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i].name != step.inputIngredients[i].name)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

}
