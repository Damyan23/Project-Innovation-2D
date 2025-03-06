using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewFinishedRecipe", menuName = "Cooking/Dish")]
public class Dish : ScriptableObject
{
    public Sprite icon;
    public Recipe recipe;
    public string dishName;
}
