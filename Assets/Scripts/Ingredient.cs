using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Cooking/Ingredient")]
public class Ingredient : ScriptableObject
{
    new public string name;
    public Sprite icon;
    public bool isInfinite;    
    
}
