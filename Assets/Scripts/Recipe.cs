using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    new public string name;
    public List<Ingredient> ingredients;
    public Sprite icon;
    public float timeToCook;
    public string station;

    public Dish output;

    public IEnumerator StartMakingRecipe(CookingManager manager)
    {
        manager.isCookingRecipe = true;
        Debug.Log("Started Cooking");   

        yield return new WaitForSeconds(timeToCook);

        //manager.AddIngredient(output);
        Debug.Log("Order Done!");

        manager.isCookingRecipe = false;
        manager.canServe = true;
        manager.finishedDish = output;
    }
}

