using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEditor;

public class CustomerManager : MonoBehaviour
{
    private RecipeDataBase dataBase;
    private List<Recipe> recipes = new List<Recipe>();

    // Prefabs to instantiate
    [SerializeField] private GameObject recipePrefab;  // The prefab for the whole recipe
    [SerializeField] private Transform recipeContainer; // The container where the recipe prefab will be instantiated

    [SerializeField] private GameObject ingredientPrefab;  // The prefab for the ingredients
    private Transform ingredientsContainer; // The container where the ingredients will be added

    private List<GameObject> recipeObjects;

    [HideInInspector] public List<CustomerRequest> requests;


    private bool waitingForCustomer;
    private const float TimePerRequest = 40f;
    private float timeLeft;
    [HideInInspector] public TMP_Text levelTimer;

    // Start is called before the first frame update
    void Start()
    {
        requests = new();
        recipeObjects = new();

        dataBase = GameManager.instance.dataBase;
        recipes = dataBase.recipes;
        waitingForCustomer = false;

        timeLeft = 120f;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            Debug.LogError("You Fucking Died");
            EditorApplication.isPaused = true;
        }
        levelTimer.text = Mathf.RoundToInt(timeLeft).ToString();

        AddRandomRecipe ();

        foreach(CustomerRequest req in requests)
        {
            GameObject obj = req.requestObject;
            float timeLeft = TimePerRequest - (Time.time - req.startTime);
            if(timeLeft <= 0)
            {
                Debug.LogError("You Lose!");
                EditorApplication.isPaused = true;
            }

            obj.transform.Find("Top").GetComponentInChildren<Image>().fillAmount =  timeLeft / TimePerRequest;
        }
    } 


    public void AddRandomRecipe()
    {
        if (recipes.Count > 0 && requests.Count < 2 && !waitingForCustomer)
        {
            StartCoroutine(AddNewRequest(5));
        }
    }

    // Function to display the recipe
    private GameObject DisplayRecipe(Recipe recipe)
    {
        // Instantiate the recipe prefab
        GameObject recipeObject = Instantiate(recipePrefab, recipeContainer);
        recipeObjects.Add(recipeObject);

        // Set the recipe name (TMP Text) and the timer
        Transform topPart = recipeObject.transform.Find("Top");

        Image timerImage = topPart.GetComponentInChildren<Image>();
        timerImage.fillAmount = 1f;

        return recipeObject;
        
    }


    public void FinishDish(Dish dish)
    {
        CustomerRequest requestToFill = null;

        foreach(CustomerRequest req in requests)
        {
            if(req.wantedDish.dishName == dish.dishName)
            {
                //Serve the oldest request first
                if(requestToFill == null || req.startTime < requestToFill.startTime)
                {
                    requestToFill = req;
                }
            }
        }

        //No correct request can be filled
        if (requestToFill == null)
        {
            Debug.LogWarning("Wrong Dish!");
            return;
        }

        Destroy(requestToFill.requestObject);
        requests.Remove(requestToFill);

        timeLeft += 20f;
            
        Debug.Log("Served A Correct Dish!");
        
    }

    private IEnumerator AddNewRequest(int waitingTimeSeconds)
    {
        waitingForCustomer = true;
        yield return new WaitForSeconds(waitingTimeSeconds);

        // Get a random index
        int randomIndex = Random.Range(0, recipes.Count);
        Recipe randomRecipe = recipes[randomIndex];

        // Create the request
        CustomerRequest newRequest = new(randomRecipe.output, randomRecipe, Time.time, DisplayRecipe(randomRecipe));
        requests.Add(newRequest);
        recipeObjects.Add(newRequest.requestObject);

        waitingForCustomer = false;
    }
}
