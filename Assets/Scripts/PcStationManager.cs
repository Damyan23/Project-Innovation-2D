using UnityEngine;

public class StationManagerPc : MonoBehaviour
{
    [SerializeField] public GameObject[] stations; // Assign your images in the Inspector
    [SerializeField] private GameObject startScreen;
    private int currentIndex = 0;
    private bool initialized = false; // Ensures initialization happens only once

    void OnEnable()
    {
        NetworkEventManager.OnButtonPressed += HandleButtonEvent;
    }

    void OnDisable()
    {
        NetworkEventManager.OnButtonPressed -= HandleButtonEvent;
    }

    void Update()
    {
        if (GameManager.instance.playerStartedGame)
        {
            if (!initialized)
            {
                InitializeStations();
                initialized = true;
            }

            if (startScreen.activeSelf) startScreen.SetActive(false);
        }
    }

    void InitializeStations()
    {
        // Disable all stations first
        foreach (var station in stations)
        {
            station.SetActive(false);
        }

        // Enable the first station
        if (stations.Length > 0)
        {
            stations[0].SetActive(true);
            currentIndex = 0;
            Debug.Log("Game started, showing kutting station");
        }
    }

    void HandleButtonEvent(string buttonFunc)
    {
        if (!GameManager.instance.playerStartedGame) return;

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
        stations[currentIndex].SetActive(false); // Disable current station

        // Move to the next index, loop back if needed
        currentIndex = (currentIndex + 1) % stations.Length;

        stations[currentIndex].SetActive(true); // Enable new station
    }

    void ShowPreviousImage()
    {
        stations[currentIndex].SetActive(false); // Disable current station

        // Move to the previous index, loop to the last if needed
        currentIndex = (currentIndex - 1 + stations.Length) % stations.Length;

        stations[currentIndex].SetActive(true); // Enable new station
    }
}
