using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Timeline.Actions;


public class CookingTester : MonoBehaviour
{
    public List<Ingredient> inventory;
    
    public string currentStation = "knife";

    public int selectedIngredientIndex = 0;
    //public CookingStep chopCarrot;


    public Transform canvas;
    public GameObject cookingStepPrefab;

    Dictionary<CookingStep, GameObject> cookingStepObjects;

    void Start()
    {
        cookingStepObjects = new();    
        DisplayAvailableCookingSteps();
    }


    void Update()
    {
        //Execute a cooking step (if possible)
        if (Input.GetKeyDown(KeyCode.F))
        {
            List<CookingStep> available = GetAvailableSteps(inventory, currentStation);
            if(available.Count > 0 && selectedIngredientIndex >= 0 && selectedIngredientIndex < available.Count)
            {
                available[selectedIngredientIndex].ProcessCookingStep(ref inventory, currentStation);
                if (selectedIngredientIndex == available.Count - 1) selectedIngredientIndex--;
            }
            DisplayAvailableCookingSteps();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIngredientIndex--;
            if (selectedIngredientIndex < 0) selectedIngredientIndex = 0;
            DisplayAvailableCookingSteps();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int stepsAmount = GetAvailableSteps(inventory, currentStation).Count;
            selectedIngredientIndex++;
            if (selectedIngredientIndex >= stepsAmount) selectedIngredientIndex = stepsAmount - 1;
            DisplayAvailableCookingSteps();
        }

        //Debug available steps to take
        if (Input.GetKeyDown(KeyCode.Q))
        {
            List<CookingStep> available = GetAvailableSteps(inventory, currentStation);

            Debug.Log("Available steps at this station: ");

            foreach(CookingStep step in available)
            {
                Debug.Log(step.output);
            }
        }
    }

    List<CookingStep> GetAvailableSteps(List<Ingredient> inventory, string station)
    {
        CookingStep[] allSteps = Resources.LoadAll<CookingStep>("CookingSteps");

        List<CookingStep> availableSteps = new List<CookingStep>();

        foreach (CookingStep step in allSteps)
        {
            if (step.requiredStation == currentStation)
            {
                if(CheckRequiredIngedients(inventory, step))
                {
                    availableSteps.Add(step);
                }
            }
        }

        return availableSteps;
    }

    bool CheckRequiredIngedients(List<Ingredient> inventory, CookingStep step)
    {
        foreach(Ingredient ingredient in step.inputIngredients)
        {
            if (!inventory.Contains(ingredient)) return false;
        }

        return true;
    }

    void DisplayAvailableCookingSteps()
    {
        foreach(GameObject obj in cookingStepObjects.Values)
        {
            Destroy(obj);
        }

        cookingStepObjects.Clear();

        List<CookingStep> available = GetAvailableSteps(inventory, currentStation);

        float yOffset = 0;

        foreach (CookingStep step in available) 
        {
            GameObject stepObject = Instantiate(cookingStepPrefab, canvas);
            stepObject.transform.position = new Vector3(stepObject.transform.position.x, stepObject.transform.position.y + yOffset, 0);

            TMP_Text ingredientNameText = stepObject.transform.Find("Ingredient Name").GetComponent<TMP_Text>();
            ingredientNameText.text = step.inputIngredients[0].ingredientName;
            if(available.Count == 1 || step == available[selectedIngredientIndex])
            {
                ingredientNameText.color = Color.white;
            }
            else
            {
                ingredientNameText.color = Color.gray;
            }

            Image ingredientImage = stepObject.GetComponentInChildren<Image>();
            ingredientImage.sprite = step.inputIngredients[0].icon;

            cookingStepObjects.Add(step, stepObject);
            yOffset -= 100;
        }
    }

}
