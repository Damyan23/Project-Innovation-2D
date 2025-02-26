using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    new public string name;
    public List<Ingredient> ingredients;
    public Sprite icon;
    public float timeToCook;
}


[System.Serializable]
public class Ingredient
{
    public string name;
    public int quantity;
    public Sprite icon;
}
