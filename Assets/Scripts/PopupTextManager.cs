using UnityEngine;
using TMPro;
using System.Collections;

public class PopupTextManager : MonoBehaviour
{
    public GameObject[] popupTextContainers; // Assign the 3 popup containers in the Inspector
    private GameManager gameManager; // Reference to the GameManager instance
    public GameObject textPrefab; // Prefab for TMP Pro text

    private void Start()
    {
        gameManager = GameManager.instance;
    } 

    private GameObject GetActivePopupContainer()
    {
        foreach (GameObject container in popupTextContainers)
        {
            if (container.transform.parent.gameObject.activeSelf)   
            {
                return container;
            }
        }
        return null;
    }

    private void CreatePopupText(string message)
    {
        GameObject activeContainer = GetActivePopupContainer();
        if (activeContainer == null)
        {
            Debug.LogWarning("No active popup container found.");
            return;
        }
        
        GameObject newText = Instantiate(textPrefab, activeContainer.transform);
        TMP_Text textComponent = newText.GetComponent<TMP_Text>();
        textComponent.text = message;

        StartCoroutine(AnimatePopupText(newText));
    }

    private IEnumerator AnimatePopupText(GameObject textObject)
    {
        TMP_Text textComponent = textObject.GetComponent<TMP_Text>();
        CanvasGroup canvasGroup = textObject.AddComponent<CanvasGroup>();
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        
        float duration = 1.5f;
        float elapsedTime = 0f;
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 50, 0);
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(textObject);
    }

    public void ShowIngredientSentToInventory()
    {
        CreatePopupText("Ingredient sent to inventory.");
        Debug.Log ("asd");
    }

    public void ShowCorrectDishSentToCustomer()
    {
        if (gameManager.currentStation == "Plating")
        {
            CreatePopupText("Correct dish sent to customer.");
        }
    }

    public void ShowIncorrectDishSentToCustomer()
    {
        if (gameManager.currentStation == "Plating")
        {
            CreatePopupText("Incorrect dish sent to customer.");
        }
    }
}
