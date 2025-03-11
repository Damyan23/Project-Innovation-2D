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
        foreach (var station in stations) station.SetActive(false);

        if (stations.Length > 0)
        {
            stations[0].SetActive(true);
            currentIndex = 0;
            UpdateCurrentStation();
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
        stations[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % stations.Length;
        stations[currentIndex].SetActive(true);
        UpdateCurrentStation(); // Invoke event
    }

    void ShowPreviousImage()
    {
        stations[currentIndex].SetActive(false);
        currentIndex = (currentIndex - 1 + stations.Length) % stations.Length;
        stations[currentIndex].SetActive(true);
        UpdateCurrentStation(); // Invoke event
    }

    void UpdateCurrentStation()
    {
        string stationName = currentIndex switch
        {
            0 => "knife",
            1 => "plating",
            2 => "soup",
            _ => "unknown"
        };


        GameManager.instance.currentStation = stationName;
        GameManager.instance.OnCurrentStationChanged?.Invoke(stationName); // Invoke the event
    }
}
