using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CookingTester : MonoBehaviour
{
    List<Ingredient> ingredients;
    
    public string currentStation = "knife";

    public Ingredient carrot;
    public CookingStep chopCarrot;

    void Start()
    {
        ingredients = new List<Ingredient>();
        ingredients.Add(carrot);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            chopCarrot.ProcessCookingStep(ref ingredients, currentStation);
        }
    }
}
