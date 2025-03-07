using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public bool playerStartedGame = false;
    [HideInInspector] public RecipeDataBase dataBase;
    [HideInInspector] public EventManager eventManager;

    // List of all ingredient data (Assign in Inspector)
    public List<CookingStep> allCookingSteps;

    // Prefab to instantiate (Assign in Inspector)
    public GameObject ingredientPrefab;
    private GameObject spawnedIngredient;
    [HideInInspector] public bool isCookingRecipe;
    public Action cookRecipeEvent;

    [HideInInspector] public CookingManager cookingManager;

    public string currentStation;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        dataBase = GetComponent<RecipeDataBase>();
        eventManager = GetComponent<EventManager>();
        cookingManager = gameObject?.GetComponent<CookingManager>();

        if (eventManager != null)
        {
            eventManager.startGameEvent += () => playerStartedGame = true;
        }
    }

    public void InstantiateIngredientById(string ingredientId)
    {
        // Find the active "Placement UI" object
        GameObject placementUI = GameObject.Find("Placement UI");

        // If not found or inactive, return early
        if (placementUI == null || !placementUI.activeInHierarchy)
        {
            Debug.LogWarning("Placement UI not found or inactive!");
            return;
        }

        foreach (CookingStep cookingStep in allCookingSteps)
        {
            if (cookingStep.name == ingredientId)
            {
                // Instantiate the prefab as a child of Placement UI
                spawnedIngredient = Instantiate(ingredientPrefab, placementUI.transform);

                // Assign the CookingStep data
                CookingStepHolder stepHolder = spawnedIngredient.GetComponent<CookingStepHolder>();
                if (stepHolder != null)
                {
                    stepHolder.cookingStep = cookingStep;
                    stepHolder.AssignSprite();
                }
                else
                {
                    Debug.LogWarning("CookingStepHolder component not found on prefab!");
                }

                // Get RectTransform components
                RectTransform spawnedRect = spawnedIngredient.GetComponent<RectTransform>();
                RectTransform placementRect = placementUI.GetComponent<RectTransform>();

                if (spawnedRect != null && placementRect != null)
                {
                    // Match the size of Placement UI
                    spawnedRect.sizeDelta = placementRect.sizeDelta;

                    // Set anchors to the center of the parent
                    spawnedRect.anchorMin = new Vector2(0.5f, 0.5f);
                    spawnedRect.anchorMax = new Vector2(0.5f, 0.5f);
                    spawnedRect.pivot = new Vector2(0.5f, 0.5f);

                    // Set position to the center of the parent
                    spawnedRect.anchoredPosition = Vector2.zero;
                }
                else
                {
                    Debug.LogWarning("RectTransform missing on either spawned object or Placement UI!");
                }

                return; // Stop the loop after spawning
            }
        }

        Debug.LogWarning($"Ingredient with ID '{ingredientId}' not found!");
    }

    void Update()
    {
        Debug.Log (isCookingRecipe);
        if (spawnedIngredient != null && isCookingRecipe)
        {
            cookRecipeEvent += () => spawnedIngredient.GetComponent<CookingStepHolder>().StartCooking("knife");
            // if (currentStation == "Cutting") 
            // if (currentStation == "Boiling") cookRecipeEvent += () => spawnedIngredient.GetComponent<CookingStepHolder>().StartCooking("Boiling");
            isCookingRecipe = false;
            UpdateVariableInClient (false);
            Debug.Log ("cooking ingridient called:");
        }
    } 

    void UpdateVariableInClient (bool variable)
    {
        NetworkEventManager.instance.UpdateVariableInClinet (variable);
    }

}
