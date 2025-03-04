using UnityEngine;

public class PcStationManager : MonoBehaviour
{
    [SerializeField] public GameObject[] stations; // Assign your images in the Inspector
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
        if (GameManager.instance.playerStartedGame == false) return;

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
        if (currentIndex < stations.Length - 1)
        {
            stations[currentIndex].SetActive(false); // Disable current image
            if (currentIndex == stations.Length) currentIndex = 0; // If we are at the last image, move to the first one
            else currentIndex++; // Move to the next image
            stations[currentIndex].SetActive(true);
            Debug.Log($"🔼 Moved to image {currentIndex}");
        }
    }

    void ShowPreviousImage()
    {
        if (currentIndex > 0)
        {
            stations[currentIndex].SetActive(false); // Disable current image
            if (currentIndex == 0) currentIndex = stations.Length;
            else currentIndex--; // Move to the previous image
            stations[currentIndex].SetActive(true); // Enable new image
            Debug.Log($"🔽 Moved to image {currentIndex}");
        }
    }
}
