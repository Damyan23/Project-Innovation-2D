using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
   // public List<Ingredient> ingredients;
    public Sprite icon;
    public Ingredient output;
}

