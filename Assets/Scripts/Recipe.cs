using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] private string recipeName;
    [SerializeField] private List<Ingredient> ingredients;
    [SerializeField] private Sprite recipeImage;
}


[System.Serializable]
public class Ingredient
{
    public string ingredientName;
    public int quantity;
    public Sprite ingredientImage;
}
