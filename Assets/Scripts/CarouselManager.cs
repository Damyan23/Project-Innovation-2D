using UnityEngine;

public class CarouselManager : MonoBehaviour
{
    public GameObject[] images; // Assign your images in the Inspector
    private int currentIndex = 0;

    void OnEnable()
    {
        NetworkEventManager.OnButtonPressed += HandleButtonEvent;
    }

    void OnDisable()
    {
        NetworkEventManager.OnButtonPressed -= HandleButtonEvent;
    }

    void HandleButtonEvent(string buttonFunc)
    {
        if (buttonFunc == "Up")
        {
            ShowNextImage();
        }
        else if (buttonFunc == "Down")
        {
            ShowPreviousImage();
        }
    }

    void ShowNextImage()
    {
        if (currentIndex < images.Length - 1)
        {
            images[currentIndex].SetActive(false); // Disable current image
            currentIndex++; // Move to the next image
            images[currentIndex].SetActive(true); // Enable new image
            Debug.Log($"🔼 Moved to image {currentIndex}");
        }
    }

    void ShowPreviousImage()
    {
        if (currentIndex > 0)
        {
            images[currentIndex].SetActive(false); // Disable current image
            currentIndex--; // Move to the previous image
            images[currentIndex].SetActive(true); // Enable new image
            Debug.Log($"🔽 Moved to image {currentIndex}");
        }
    }
}
