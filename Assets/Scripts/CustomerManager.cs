using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEditor;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class CustomerManager : MonoBehaviour
{
    private RecipeDataBase dataBase;
    private List<Recipe> recipes = new List<Recipe>();
    // Prefabs to instantiate
    [SerializeField] private GameObject recipePrefab;  // The prefab for the whole recipe
    [SerializeField] private Transform recipeContainer; // The container where the recipe prefab will be instantiated
    private List<GameObject> recipeObjects = new List<GameObject>(); // List to store the instantiated recipe prefabs
    [HideInInspector] public List<CustomerRequest> requests = new List<CustomerRequest>();


    private bool waitingForCustomer;
    private const float TimePerRequest = 45f;
    private float timeLeft;
    
    [SerializeField] private TMP_Text levelTimer;
    [SerializeField] private GameObject gameOverScreen;

    // Start is called before the first frame update
    void Start()
    {
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
            StartCoroutine(SendBackToMainMenu(7.5f));
        }

        levelTimer.text = Mathf.RoundToInt(timeLeft).ToString();

        AddRandomRecipe ();

        foreach(CustomerRequest req in requests)
        {
            GameObject obj = req.requestObject;
            float timeLeft = TimePerRequest - (Time.time - req.startTime);
            Image img = obj.transform.Find("Top").GetComponentInChildren<Image>();

            if (timeLeft <= 10.0f)
            {
                img.color = Color.red;
            }

            if(timeLeft <= 0)
            {
                StartCoroutine(SendBackToMainMenu(7.5f));
            }

            img.fillAmount =  timeLeft / TimePerRequest;
        }
    } 

    private IEnumerator SendBackToMainMenu(float seconds)
    {
        gameOverScreen.SetActive(true);
        yield return new WaitForSeconds(seconds);

        SceneManager.LoadScene("Main Menu");
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
        //Create new game object add a sprite to it and make it the recipe sprite then instantiate it

        // Instantiate the recipe prefab
        GameObject recipeObject = Instantiate(recipePrefab, recipeContainer);
        recipeObjects.Add(recipeObject);

        // Set the recipe name (TMP Text) and the timer
        Transform content = recipeObject.transform.Find("Content");
        content.GetComponent<Image>().sprite = recipe.icon;


        Transform topPart = recipeObject.transform.Find("Top");
        TMP_Text recipeName = topPart.GetComponentInChildren<TMP_Text>();
        recipeName.text = recipe.name;
        
        Image timerImage = topPart.GetComponentInChildren<Image>();
        timerImage.fillAmount = 1f;


        return recipeObject;
    }


    public void FinishDish(Dish dish)
    {
        CustomerRequest requestToFill = null;

        foreach(CustomerRequest req in requests)
        {
            if(req.wantedDish.name == dish.ingredient.name)
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

    public void FinishDish(string dishName)
    {
        CustomerRequest requestToFill = null;
        foreach(CustomerRequest req in requests)
        {
            Debug.Log (req.wantedDish.name);
            if(req.wantedDish.name == dishName)
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
            GameManager.instance.popupTextManager.ShowIncorrectDishSentToCustomer ();
            return;
        }

        GameManager.instance.popupTextManager.ShowCorrectDishSentToCustomer ();

        Destroy(requestToFill.requestObject);
        requests.Remove(requestToFill);

        timeLeft += 20f;
            
        Debug.Log("Served A Correct Dish!");
        
    }

    private IEnumerator AddNewRequest(int waitingTimeSeconds)
    {
        Debug.Log("AddNewRequest started");
        waitingForCustomer = true;
        yield return new WaitForSeconds(waitingTimeSeconds);
        Debug.Log("After waiting");

        // Get a random index
        int randomIndex = Random.Range(0, recipes.Count);
        Recipe randomRecipe = recipes[randomIndex];

        // Create the request
        CustomerRequest newRequest = new(randomRecipe.output, Time.time, DisplayRecipe(randomRecipe));
        requests.Add(newRequest);
        recipeObjects.Add(newRequest.requestObject);
        Debug.Log("Request added, requests.Count = " + requests.Count);

        waitingForCustomer = false;
    }
}
