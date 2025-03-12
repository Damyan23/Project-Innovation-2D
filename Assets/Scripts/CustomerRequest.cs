using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerRequest
{
    public CustomerRequest(Ingredient wantedDish, float startTime, GameObject requestObject)
    {
        this.wantedDish = wantedDish;
        this.startTime = startTime;
        this.requestObject = requestObject;
    }

    public Ingredient wantedDish;
    public float startTime;
    public GameObject requestObject;

}
