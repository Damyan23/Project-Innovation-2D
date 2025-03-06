using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStep", menuName = "Cooking/CookingStep")]
public class CookingStep : ScriptableObject
{
    new public string name;
    public List<Ingredient> inputIngredients;
    public Ingredient output;
    public string requiredStation;
}
