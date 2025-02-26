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

