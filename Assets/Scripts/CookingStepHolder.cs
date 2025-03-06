using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CookingStepHolder : MonoBehaviour
{
    public CookingStep cookingStep; // Assign in Inspector or dynamically
    private Image stepImage;
    private CookingManager cookingManager;
    private string currentStation;
    public float delay = 1.0f;

    private void Start()
    {
        
        if (stepImage == null)
        {
            Debug.LogWarning("No Image component found on " + gameObject.name);
        }

        //cookingManager = FindObjectOfType<CookingManager>();
        // if (cookingManager == null)
        // {
        //     Debug.LogError("CookingManager not found in scene!");
        // }
    }

    public void AssignSprite()
    {
        stepImage = gameObject.GetComponent<Image>();

        if (stepImage != null && cookingStep.inputIngredients.Count > 0)
        {
            stepImage.sprite = cookingStep.inputIngredients[0].icon;
        }
    }

    public void StartCooking(string station)
    {
        if (cookingStep == null)
        {
            Debug.LogWarning("No CookingStep assigned!");
            return;
        }

        currentStation = station;

        if (stepImage != null)
        {
            stepImage.sprite = cookingStep.output.icon; // Show output icon
        }

        StartCoroutine(ProcessCookingStepCoroutine());
    }

    private IEnumerator ProcessCookingStepCoroutine()
    {
        yield return new WaitForSeconds(delay);

        ProcessCookingStep();
    }

    private void ProcessCookingStep()
    {
        // All ingredients are in inventory
        NetworkEventManager.instance.SendCookedIngredient (cookingStep.output, currentStation);
        Destroy (gameObject);

        Debug.Log("Created " + cookingStep.output.ingredientName);
    }
}
