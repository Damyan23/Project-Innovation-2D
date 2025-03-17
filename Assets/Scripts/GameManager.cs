using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public bool playerStartedGame = true;
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

    private Dictionary<string, GameObject> instantiatedIngredients;

    [HideInInspector] public string cookingOutputName;

    [SerializeField] private float delay = 2;

    [SerializeField] private Slider slider;

    private const int CutsNeeded = 6;
    private int currentCuts = 0;

    [HideInInspector] public Action<string> OnCurrentStationChanged;
    private bool isSendingToInventory = false; // Flag to prevent multiple calls

    [HideInInspector] public CustomerManager customerManager;
    [HideInInspector] public PopupTextManager popupTextManager;
    [HideInInspector] public string doneDishName;

    SoundManager soundManager;
    bool isBurnerOn = false;

    [SerializeField] private GameObject tutorialSprite;
    bool seenTutorial;

    [SerializeField] private GameObject waterPot;

    [SerializeField] private GameObject ui;

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

        instantiatedIngredients = new ();

        allIngredients = Resources.LoadAll <Ingredient> ("Ingredients");

       
        cookRecipeEvent += CookRecipe;
    }

    

    private void OnEnable()
    {
        dataBase = GetComponent<RecipeDataBase>();
        eventManager = GetComponent<EventManager>();
        cookingManager = GetComponent<CookingManager>();
        customerManager = GetComponent <CustomerManager> ();
        popupTextManager = GetComponent <PopupTextManager> ();
        soundManager = GetComponent<SoundManager>();

        seenTutorial = false;

        if (eventManager != null)
        {
            eventManager.startGameEvent += () =>
            {
                playerStartedGame = true;
            };
        }

        OnCurrentStationChanged += ToggleProgressBar;
        OnCurrentStationChanged += StationSounds;
        OnCurrentStationChanged += ToggleWaterPot;

        if(slider != null)
        {
            currentCuts = 0;
            slider.maxValue = CutsNeeded;
            slider.value = 0;
        }
    }

    private void OnDisable()
    {
        OnCurrentStationChanged -= ToggleProgressBar;
        OnCurrentStationChanged -= StationSounds;
        OnCurrentStationChanged -= ToggleWaterPot;
    }

    public void InstantiateIngredientById(List<string> ingredientNames)
    {
        if (!playerStartedGame) return;

        // Find the active "Placement UI" object
        GameObject placementUI = GameObject.Find("Placement UI");

        // If not found or inactive, return early
        if (placementUI == null || !placementUI.activeInHierarchy)
        {
            Debug.LogWarning("Placement UI not found or inactive!");
            return;
        }

        if (currentStation == "plating" && ingredientNames.Count == 1) doneDishName = ingredientNames[0];

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
                instantiatedIngredients.Add (ingredientName, instantiatedObj);

                if (currentStation == "plating")
                {
                    soundManager.PlayPlatingNoise();
                }
                else if(currentStation == "mixing")
                {
                    soundManager.PlayWaterSplash();
                }
                else
                {
                    soundManager.PlayItemPickup();
                }

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

    void ToggleProgressBar(string station)
    {
        slider.gameObject.SetActive(station != "plating");
    }

    void Update()
    {
        if(!seenTutorial && playerStartedGame)
        {
            Destroy(tutorialSprite);
            if (ui != null) ui.SetActive (true);
            seenTutorial = true;
        }
    } 

    void CookRecipe()
    {
        if (currentStation != "plating")
        {
            if (isCookingRecipe && string.IsNullOrEmpty(cookingOutputName) && instantiatedIngredients.Count > 0)
            {
                currentCuts++;
                slider.value = currentCuts;

                if (currentStation == "cutting")
                {
                    soundManager.PlayCut();
                }

                if(currentStation == "mixing")
                {
                     soundManager.PlayMixing();
                }

            }

            if(currentCuts >= CutsNeeded && instantiatedIngredients.Count > 0)
            {
                FindLocalPlayer().GetComponent<NetworkEventManager> ().RequestCookingStepOutput ();
                StartCoroutine(WaitForOutputStepName());
                slider.value = 0;
                currentCuts = 0;
                isCookingRecipe = false;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(doneDishName)) return;

            customerManager.FinishDish(doneDishName);
            Destroy(instantiatedIngredients[doneDishName]);
            doneDishName = "";

            soundManager.PlayOrderSent();
        }
    }

    public void TrashIngredients()
    {
        if (isServer) return;

        foreach(var obj in instantiatedIngredients.Values)
        {
            Destroy(obj);
        }

        soundManager.PlayTrashing();

        cookingManager.selectedIngredients[currentStation].Clear();
    }

    void ToggleWaterPot(string station)
    {
        if (isServer) return;

        if (station == "mixing")
        {
            waterPot.SetActive(true);
        }
        else
        {
            waterPot.SetActive(false);
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
            isCookingRecipe = false;
        }
        else
        {
            Debug.LogWarning($"Ingredient matching outputStepName '{cookingOutputName}' not found!");
        }
    }

    private void UpdateIngredientUI(Ingredient newIngredient)
    {
        waterPot.SetActive(false);

        // Instantiate the new ingredient
        GameObject instantiatedObj = Instantiate(ingredientPrefab, GameObject.Find("Placement UI").transform);
        Image prefabSprite = instantiatedObj.GetComponent<Image>();
        prefabSprite.sprite = newIngredient.icon;

        foreach (Ingredient ingredient in allIngredients)
        {
            foreach (string ingredientName in instantiatedIngredients.Keys)
            {
                if (ingredient.name == ingredientName)
                {
                    Destroy(instantiatedIngredients[ingredientName]);
                }
            }
        }
        instantiatedIngredients.Clear ();

        instantiatedIngredients.Add (cookingOutputName, instantiatedObj);

        // Ensure the object has a RectTransform component
        RectTransform rectTransform = instantiatedObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Center the object inside its parent
            rectTransform.anchoredPosition = Vector2.zero; // Set position to (0,0)
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // Set pivot to center

            if(currentStation == "mixing")
            {
                RectTransform potRect = waterPot.GetComponent<RectTransform>();
                rectTransform.position = potRect.position;
                rectTransform.sizeDelta = potRect.sizeDelta;
            }
            else
            {
                // Optionally match the parent's size
                rectTransform.sizeDelta = GameObject.Find("Placement UI").GetComponent<RectTransform>().sizeDelta;
            }
        }


        StartCoroutine (SendIngredientToInventory ());
    }


    private IEnumerator SendIngredientToInventory()
    {
        if (isSendingToInventory) yield break; // Prevent duplicate execution
        isSendingToInventory = true; // Set flag to true

        soundManager.PlayActionDone();

        yield return new WaitForSeconds(delay);

        popupTextManager.ShowIngredientSentToInventory();
        //Debug.Log(cookingOutputName);

        if (!string.IsNullOrEmpty(cookingOutputName) && instantiatedIngredients.ContainsKey(cookingOutputName))
        {
            FindLocalPlayer().GetComponent<NetworkEventManager>().SendIngredientToInventory(cookingOutputName);
            Destroy(instantiatedIngredients[cookingOutputName]); // Fixed ingredientName reference
            instantiatedIngredients.Clear();
            soundManager.PlayInventorySwoosh();

            if(currentStation == "mixing")
            {
                waterPot.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Tried to send an ingredient to inventory but no valid ingredientName was found!");
        }

        isSendingToInventory = false; // Reset flag after execution

        cookingOutputName = "";
    }

    void StationSounds(string station)
    {
        if(station == "mixing")
        {
            soundManager.PlayBoilingWater(true);
            if (!isBurnerOn)
            {
                soundManager.PlayBurnerClicks();
                isBurnerOn = true;
            }
        }
        else
        {
            soundManager.PlayBoilingWater(false);
        }
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

    public void LoadScene (string scene)
    {
        SceneManager.LoadScene (scene);
    }

}
