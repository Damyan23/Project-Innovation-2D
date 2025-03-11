using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.BouncyCastle.Utilities;
using Unity.VisualScripting;

public class CookingManager : MonoBehaviour
{
    [Header("Inventory debug Stuff")]
    public List<Ingredient> startingInventory;
    public List<Ingredient> inventory;
    [SerializeField] public string currentStation;

    [Header("Recipe Debug Stuff")]
    [HideInInspector] public Dish finishedDish;
    public List<Dish> wantedDishes;

    [Header("Prefabs")]
    [SerializeField] private GameObject cookingStepPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("References")]
    [SerializeField] private Transform inventoryParent;

    List<GameObject> inventoryItems;

    //Allow for different selected ingredients for each station
    [HideInInspector] public Dictionary<string, List<Ingredient>> selectedIngredients;

    [HideInInspector] public bool isCookingRecipe;
    [HideInInspector] public bool canServe;

    CustomerManager customerManager;
    
    private Ingredient[] ingredients;

    void Start()
    {
        customerManager = GetComponent<CustomerManager>();
        if (customerManager == null) Debug.LogWarning("No CustomerManager Script Found");

        inventoryItems = new();

        selectedIngredients = new();

        selectedIngredients.Add("knife", new());
        selectedIngredients.Add("soup", new());
        selectedIngredients.Add("plating", new());

        DisplayInventory();

        finishedDish = null;
        isCookingRecipe = false;
        canServe = false;
        
        ingredients = Resources.LoadAll<Ingredient>("Ingredients");
    }


    void Update()
    {
        if (isCookingRecipe) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TrashIngredients();
        }

        Debug.Log (currentStation);

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    if (currentStation == "plating")
        //    {
        //        TryPlateDish();
        //    }
        //    else
        //    {
        //        TryProcessIngredient();
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(currentStation == "plating")
            {
                TryServePlate();
            }
        }
    }

    public void TryProcessIngredient()
    {
        List<CookingStep> available = GetAvailableSteps(selectedIngredients[currentStation], currentStation);
        Debug.Log (available);

        foreach (CookingStep step in available)
        {
            if (IsValidCookingStep(selectedIngredients[currentStation], step))
            {
                step.ProcessCookingStep(this, currentStation);
                selectedIngredients[currentStation].Clear();
                return;
            }
        }
    }

    public void TryPlateDish()
    {
        Recipe[] recipes = Resources.LoadAll<Recipe>("Recipes");

        foreach (Recipe recipe in recipes)
        {
            if (IsValidRecipe(selectedIngredients[currentStation], recipe))
            {
                StartCoroutine(recipe.StartMakingRecipe(this));
                selectedIngredients[currentStation].Clear();
                return;
            }
        }
    }
    private void TryServePlate()
    {
        if (canServe && finishedDish != null)
        {
            canServe = false;
            customerManager.FinishDish(finishedDish);
            finishedDish = null;
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
                if (CheckRequiredIngedients(inventory, step))
                {
                    availableSteps.Add(step);
                }
            }
        }

        return availableSteps;
    }

    public void TrashIngredients()
    {
        selectedIngredients[currentStation].Clear();
    }

    bool CheckRequiredIngedients(List<Ingredient> inventory, CookingStep step)
    {

        foreach (Ingredient ingredient in step.inputIngredients)
        {
            if (!inventory.Contains(ingredient)) return false;
        }

        return true;
    }

    void DisplayInventory()
    {

        foreach (Ingredient ingredient in startingInventory)
        {
            AddIngredient(ingredient);
        }
    }

    public void AddIngredient(Ingredient ingredient)
    {
        GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryParent);
        inventoryItem.GetComponent<Image>().sprite = ingredient.icon;
        inventoryItem.GetComponent<InventoryItem>().ingredient = ingredient;
        inventoryItem.GetComponent<InventoryItem>().manager = this;

        Transform child = inventoryItem.transform.GetChild(0);
        child.GetComponent<TMP_Text>().text = ingredient.name;

        inventoryItems.Add(inventoryItem);
        inventory.Add(ingredient);
    }

    public void AddIngredient(string ingredientName)
    {
        Ingredient ingredient = null;

        foreach (Ingredient ing in ingredients)
        {
            if (ing.name == ingredientName)
            {
                ingredient = ing;
                break;  // Add this to exit the loop when found
            }
        }
        
        if (ingredient == null)
        {
            Debug.LogError($"Could not find ingredient with name: {ingredientName}");
            return;
        }

        GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryParent);
        inventoryItem.GetComponent<Image>().sprite = ingredient.icon;
        inventoryItem.GetComponent<InventoryItem>().ingredient = ingredient;
        inventoryItem.GetComponent<InventoryItem>().manager = this;

        Transform child = inventoryItem.transform.GetChild(0);
        child.GetComponent<TMP_Text>().text = ingredient.name;

        inventoryItems.Add(inventoryItem);
        inventory.Add(ingredient);
    }

    public void ResetCookingOutput ()
    {
        FindLocalPlayer ().GetComponent<NetworkEventManager> ().ResetCookingOutput ();
    }

    public void RemoveIngredient(Ingredient ingredient)
    {
        inventory.Remove(ingredient);

        foreach (GameObject item in inventoryItems)
        {
            if (item.GetComponent<InventoryItem>().ingredient == ingredient)
            {
                inventoryItems.Remove(item);
                Destroy(item);
                return;
            }
        }
    }

    public void SelectInventoryItem(InventoryItem item)
    {
        if (isCookingRecipe) return;
        if (currentStation == "knife" && selectedIngredients["knife"].Count >= 1) return;

        if (!item.ingredient.isInfinite) RemoveIngredient(item.ingredient);
        selectedIngredients[currentStation].Add(item.ingredient);

       FindLocalPlayer().GetComponent<NetworkEventManager>().CmdSendSelectedIngredients (getSelectedIngredientsNames ());
    }

    List<string> getSelectedIngredientsNames ()
    {
        List<string> ingredientNames = new List<string> ();

        foreach (Ingredient ingredient in selectedIngredients[currentStation])
        {
            if (!ingredientNames.Contains (ingredient.name)) ingredientNames.Add (ingredient.name);
        }

        return ingredientNames;
    }

    private NetworkBehaviour FindLocalPlayer()
    {
        NetworkBehaviour[] allNetworkBehaviours = FindObjectsOfType<NetworkBehaviour>();
        foreach (var netBehaviour in allNetworkBehaviours)
        {
            if (netBehaviour.isLocalPlayer)
            {
                return netBehaviour;
            }
        }
        
        return null;
    }

    bool IsValidCookingStep(List<Ingredient> ingredients, CookingStep step)
    {
        if (ingredients.Count == step.inputIngredients.Count)
        {
            //Sort the lists and then compare them (ChatGPT)
            //This makes sure the ingredients are correct regardless of order of selecting them
            bool areEqual = ingredients.Select(obj => obj.name).OrderBy(x => x)
                .SequenceEqual(step.inputIngredients.Select(obj => obj.name).OrderBy(x => x));

            return areEqual;
        }
        else
        {
            return false;
        }
    }

    public void GetValidCookingStepName()
    {
        // Load all cooking steps from the "CookingSteps" resource folder
        CookingStep[] cookingSteps = Resources.LoadAll<CookingStep>("CookingSteps");

        List<Ingredient> currentSelectedIngredients = selectedIngredients[currentStation];

        foreach (CookingStep step in cookingSteps)
        {
            if (currentSelectedIngredients.Count == step.inputIngredients.Count)
            {
                // Check if the ingredients match, regardless of order
                bool areEqual = currentSelectedIngredients.Select(obj => obj.name).OrderBy(x => x)
                    .SequenceEqual(step.inputIngredients.Select(obj => obj.name).OrderBy(x => x));
                if (areEqual)
                {
                    CmdSetOutputStepName(step.output.name);
                    Debug.Log ("are equal");
                }
            }
        }
    }

    void CmdSetOutputStepName(string stepOutputName)
    {
        // Set the output step name on the server (or on the NetworkManager)
        NetworkEventManager.instance.CmdSetOutputStepName (stepOutputName);
    }


    public string GetValidRecipeOutput(List<Ingredient> ingredients, CookingStep step)
    {
        if (ingredients.Count == step.inputIngredients.Count)
        {
            //Sort the lists and then compare them (ChatGPT)
            //This makes sure the ingredients are correct regardless of order of selecting them
            bool areEqual = ingredients.Select(obj => obj.name).OrderBy(x => x)
                .SequenceEqual(step.inputIngredients.Select(obj => obj.name).OrderBy(x => x));
            
            if (!areEqual) { return null; }

            return step.output.name;
        }

        return null;
    }

    bool IsValidRecipe(List<Ingredient> ingredients, Recipe recipe)
    {
        if (ingredients.Count == recipe.ingredients.Count)
        {
            //Sort the lists and then compare them (ChatGPT)
            //This makes sure the ingredients are correct regardless of order of selecting them
            bool areEqual = ingredients.Select(obj => obj.name).OrderBy(x => x)
                .SequenceEqual(recipe.ingredients.Select(obj => obj.name).OrderBy(x => x));

            return areEqual;
        }
        else
        {
            return false;
        }
    }

}
