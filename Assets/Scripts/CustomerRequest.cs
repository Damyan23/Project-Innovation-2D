using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerRequest
{
    public CustomerRequest(Dish wantedDish, Recipe dishRecipe, float startTime, GameObject requestObject)
    {
        this.wantedDish = wantedDish;
        this.dishRecipe = dishRecipe;
        this.startTime = startTime;
        this.requestObject = requestObject;
    }

    public Dish wantedDish;
    public Recipe dishRecipe;
    public float startTime;
    public GameObject requestObject;

}
