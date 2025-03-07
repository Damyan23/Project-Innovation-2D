using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class CookingManager : MonoBehaviour
{
    [Header("Inventory debug Stuff")]
    public List<Ingredient> startingInventory;
    public List<Ingredient> inventory;
    public string currentStation = "cutting";

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
    }


    void Update()
    {
        if (isCookingRecipe) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TrashIngredients();
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentStation == "plating")
            {
                TryPlateDish();
            }
            else
            {
                TryProcessIngredient();
            }
        }
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
        child.GetComponent<TMP_Text>().text = ingredient.ingredientName;

        inventoryItems.Add(inventoryItem);
        inventory.Add(ingredient);
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

       //FindLocalPlayer().GetComponent<NetworkEventManager>().CmdSpawnIngridient (item.ingredient.name);
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

    //Dont look at this function tbh
    bool IsValidCookingStep(List<Ingredient> ingredients, CookingStep step)
    {
        if (ingredients.Count == step.inputIngredients.Count)
        {
            //Sort the lists and then compare them (ChatGPT)
            //This makes sure the ingredients are correct regardless of order of selecting them
            bool areEqual = ingredients.Select(obj => obj.ingredientName).OrderBy(x => x)
                .SequenceEqual(step.inputIngredients.Select(obj => obj.ingredientName).OrderBy(x => x));

            return areEqual;
        }
        else
        {
            return false;
        }
    }

    bool IsValidRecipe(List<Ingredient> ingredients, Recipe recipe)
    {
        if (ingredients.Count == recipe.ingredients.Count)
        {
            //Sort the lists and then compare them (ChatGPT)
            //This makes sure the ingredients are correct regardless of order of selecting them
            bool areEqual = ingredients.Select(obj => obj.ingredientName).OrderBy(x => x)
                .SequenceEqual(recipe.ingredients.Select(obj => obj.ingredientName).OrderBy(x => x));

            return areEqual;
        }
        else
        {
            return false;
        }
    }

}
