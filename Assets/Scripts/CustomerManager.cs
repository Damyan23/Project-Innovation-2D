using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerManager : MonoBehaviour
{
    private RecipeDataBase dataBase;
    private List<Recipe> recipes = new List<Recipe>();

    // Prefabs to instantiate
    public GameObject recipePrefab;  // The prefab for the whole recipe
    public Transform recipeContainer; // The container where the recipe prefab will be instantiated

    public GameObject ingredientPrefab;  // The prefab for the ingredients
    private Transform ingredientsContainer; // The container where the ingredients will be added

    private List<GameObject> recipeObjects;

    [HideInInspector] public List<Dish> wantedDishes;

    private bool waitingForCustomer;

    // Start is called before the first frame update
    void Start()
    {
        wantedDishes = new();
        recipeObjects = new();

        dataBase = GameManager.instance.dataBase;
        recipes = dataBase.recipes;
        waitingForCustomer = false;
    }

    void Update()
    {
        AddRandomRecipe ();
    } 


    public void AddRandomRecipe()
    {
        if (recipes.Count > 0 && wantedDishes.Count < 2 && !waitingForCustomer)
        {
            StartCoroutine(AddNewRecipe(5));
        }
    }

    // Function to display the recipe
    private void DisplayRecipe(Recipe recipe)
    {
        // Instantiate the recipe prefab
        GameObject recipeObject = Instantiate(recipePrefab, recipeContainer);
        recipeObjects.Add(recipeObject);


        // Set the recipe name (TMP Text) and the timer
        Transform topPart = recipeObject.transform.Find("Top");
        TMP_Text recipeNameText = topPart.GetComponentInChildren<TMP_Text>();
        recipeNameText.text = recipe.name;

        Image timerImage = topPart.GetComponentInChildren<Image>();
        timerImage.fillAmount = 1f;

        ingredientsContainer = recipeObject.transform.Find("Content");

        // Display the ingredients
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            DisplayIngredient(ingredient);
        }
    }

    // Function to display an ingredient
    private void DisplayIngredient(Ingredient ingredient)
    {
        // Instantiate an ingredient prefab
        GameObject ingredientObject = Instantiate(ingredientPrefab, ingredientsContainer);

        Image ingredientIcon = ingredientObject.GetComponentInChildren<Image>();
        ingredientIcon.sprite = ingredient.icon;
    }

    public void FinishDish(Dish dish)
    {
        if (wantedDishes.Contains(dish))
        {
            //I can't think of another way to do this
            foreach(GameObject obj in recipeObjects)
            {
                Destroy(obj);
            }
            wantedDishes.Remove(dish);

            foreach(Dish wantedDish in wantedDishes)
            {
                DisplayRecipe(wantedDish.recipe);
            }

            
            Debug.Log("Served A Correct Dish!");
        }
        else
        {
            Debug.Log("Served Wrong Dish!");
        }
    }

    private IEnumerator AddNewRecipe(int waitingTimeSeconds)
    {
        waitingForCustomer = true;
        yield return new WaitForSeconds(waitingTimeSeconds);

        // Get a random index
        int randomIndex = Random.Range(0, recipes.Count);
        Recipe randomRecipe = recipes[randomIndex];

        // Display the recipe
        DisplayRecipe(randomRecipe);
        wantedDishes.Add(randomRecipe.output);

        waitingForCustomer = false;
    }
}
