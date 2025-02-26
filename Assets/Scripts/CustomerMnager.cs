using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerMnager : MonoBehaviour
{
    private RecipeDataBase dataBase;
    private List<Recipe> recipes = new List<Recipe>();
    private List<Recipe> currentRecipes = new List<Recipe>();

    // Prefabs to instantiate
    public GameObject recipePrefab;  // The prefab for the whole recipe
    public Transform recipeContainer; // The container where the recipe prefab will be instantiated

    public GameObject ingredientPrefab;  // The prefab for the ingredients
    private Transform ingredientsContainer; // The container where the ingredients will be added

    // Start is called before the first frame update
    void Start()
    {
        dataBase = GameManager.instance.dataBase;
        recipes = dataBase.recipes;
    }

    void Update()
    {
        AddRandomRecipe ();
    } 


    public void AddRandomRecipe()
    {
        if (recipes.Count > 0 && currentRecipes.Count < 1)
        {
            // Get a random index
            int randomIndex = Random.Range(0, recipes.Count);
            Recipe randomRecipe = recipes[randomIndex];

            // Display the recipe
            DisplayRecipe(randomRecipe);
            currentRecipes.Add (randomRecipe);

            Debug.Log($"Added {randomRecipe.name} to current recipes.");
        }
        else
        {
            Debug.LogWarning("No recipes available in the database.");
        }
    }

    // Function to display the recipe
    private void DisplayRecipe(Recipe recipe)
    {
        // Instantiate the recipe prefab
        GameObject recipeObject = Instantiate(recipePrefab, recipeContainer);

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

        TMP_Text ingredientNameText = ingredientObject.transform.Find("IngridientName").GetComponentInChildren<TMP_Text>();
        TMP_Text ingredientQuantityText = ingredientObject.transform.Find("QuantityText").GetComponent<TMP_Text>();

        ingredientNameText.text = ingredient.name;
        ingredientQuantityText.text = ingredient.quantity.ToString();
    }
}
