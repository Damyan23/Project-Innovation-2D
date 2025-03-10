using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.BouncyCastle.Security;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public bool playerStartedGame = false;
    [HideInInspector] public RecipeDataBase dataBase;
    [HideInInspector] public EventManager eventManager;

    // List of all ingredient data (Assign in Inspector)
    public Ingredient[] allIngredients;

    // Prefab to instantiate (Assign in Inspector)
    public GameObject ingredientPrefab;
    private GameObject spawnedIngredient;
    [HideInInspector] public bool isCookingRecipe;
    public Action cookRecipeEvent;

    [HideInInspector] public CookingManager cookingManager;

    [HideInInspector] public List<CookingStep> boilingStationCookingSteps;

    public string currentStation;

    private Dictionary<string, GameObject> instantiatedIngredients = new Dictionary<string, GameObject>();

    [HideInInspector] public string cookingOutputName;

    [SerializeField] private Slider slider;

    private const int CutsNeeded = 5;
    private int currentCuts = 0;

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

        allIngredients = Resources.LoadAll <Ingredient> ("Ingredients");

        if (isServer)
        {
            slider.maxValue = CutsNeeded;
            slider.value = 0;
        }
        cookRecipeEvent += CookRecipe;
    }

    private void OnEnable()
    {
        dataBase = GetComponent<RecipeDataBase>();
        eventManager = GetComponent<EventManager>();
        cookingManager = GetComponent<CookingManager>();

        if (eventManager != null)
        {
            eventManager.startGameEvent += () => playerStartedGame = true;
        }
    }

    public void InstantiateIngredientById(List<string> ingredientNames)
    {
        // Find the active "Placement UI" object
        GameObject placementUI = GameObject.Find("Placement UI");

        // If not found or inactive, return early
        if (placementUI == null || !placementUI.activeInHierarchy)
        {
            Debug.LogWarning("Placement UI not found or inactive!");
            return;
        }

        foreach (string ingredientName in ingredientNames)
        {
            // Check if we already instantiated this ingredient
            if (instantiatedIngredients.ContainsKey(ingredientName))
            {
                Debug.Log($"Ingredient {ingredientName} is already instantiated. Skipping.");
                continue;
            }

            // Find the ingredient in allIngredients
            Ingredient foundIngredient = null;
            foreach (Ingredient ingredient in allIngredients)
            {
                if (ingredient.name == ingredientName)
                {
                    foundIngredient = ingredient;
                    break;
                }
            }

            // If ingredient was found, instantiate it
            if (foundIngredient != null)
            {
                GameObject instantiatedObj = Instantiate(ingredientPrefab, placementUI.transform);
                Image prefabSprite = instantiatedObj.GetComponent<Image>();
                prefabSprite.sprite = foundIngredient.icon;

                // Ensure the object has a RectTransform component
                RectTransform rectTransform = instantiatedObj.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    // Center the object inside its parent
                    rectTransform.anchoredPosition = Vector2.zero; // Set position to (0,0)
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f); // Set pivot to center

                    // Optionally match the parent's size
                    rectTransform.sizeDelta = placementUI.GetComponent<RectTransform>().sizeDelta;
                }
                else
                {
                    Debug.LogWarning("RectTransform missing on instantiated object!");
                }

                // Store the instantiated object
                instantiatedIngredients[ingredientName] = instantiatedObj;
            }
            else
            {
                Debug.LogWarning($"Ingredient {ingredientName} not found in allIngredients!");
            }
        }
    }

    void Update()
    {
        //Debug.Log(isCookingRecipe);
        if (spawnedIngredient != null && isCookingRecipe)
        {
            
            isCookingRecipe = false;
            UpdateVariableInClient (false);
            Debug.Log ("cooking ingridient called:");
        }
    } 

    void CookRecipe()
    {
        if (isCookingRecipe)
        {
            currentCuts++;

            slider.value = currentCuts;
        }

        if(currentCuts >= CutsNeeded)
        {
            FindLocalPlayer().GetComponent<NetworkEventManager> ().RequestCookingStepOutput ();
            Debug.Log (cookingOutputName);
            StartCoroutine(WaitForOutputStepName());
            slider.value = 0;
        }

    }

    private IEnumerator WaitForOutputStepName()
    {
        // Wait for a maximum of 5 seconds (you can adjust this time)
        float waitTime = 5f;
        float timeElapsed = 0f;

        // Keep checking if outputStepName is empty
        while (string.IsNullOrEmpty(cookingOutputName))
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > waitTime)
            {
                Debug.LogWarning("Output step name not received in time. Aborting.");
                yield break; // Exit the coroutine if time exceeds the limit
            }

            yield return null; // Wait for the next frame
        }

        // Now loop through all ingredients to find the one corresponding to the outputStepName
        Ingredient foundIngredient = null;
        foreach (Ingredient ingredient in allIngredients)
        {
            if (ingredient.name == cookingOutputName)
            {
                foundIngredient = ingredient;
                break;
            }
        }

        if (foundIngredient != null)
        {
            // Update the ingredient UI with the new ingredient
            UpdateIngredientUI(foundIngredient);
            cookingOutputName = "";
            isCookingRecipe = false;
        }
        else
        {
            Debug.LogWarning($"Ingredient matching outputStepName '{cookingOutputName}' not found!");
        }
    }

    private void UpdateIngredientUI(Ingredient newIngredient)
    {
        // Destroy all currently spawned ingredients
        foreach (var instantiatedIngredient in instantiatedIngredients.Values)
        {
            Destroy(instantiatedIngredient);
        }

        // Clear the dictionary of instantiated ingredients
        instantiatedIngredients.Clear();

        // Instantiate the new ingredient
        GameObject instantiatedObj = Instantiate(ingredientPrefab, GameObject.Find("Placement UI").transform);
        Image prefabSprite = instantiatedObj.GetComponent<Image>();
        prefabSprite.sprite = newIngredient.icon;

        // Ensure the object has a RectTransform component
        RectTransform rectTransform = instantiatedObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Center the object inside its parent
            rectTransform.anchoredPosition = Vector2.zero; // Set position to (0,0)
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // Set pivot to center

            // Optionally match the parent's size
            rectTransform.sizeDelta = GameObject.Find("Placement UI").GetComponent<RectTransform>().sizeDelta;
        }

        // Store the instantiated object
        instantiatedIngredients[newIngredient.name] = instantiatedObj;
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

    void UpdateVariableInClient (bool variable)
    {
        //NetworkEventManager.instance.UpdateVariableInClinet (variable);
    }

}
