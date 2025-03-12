using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI; // Required for UI Image component

public class PhoneStationManager : MonoBehaviour
{
    [SerializeField] private Image leftIcon;   // Previous station icon
    [SerializeField] private Image centerIcon; // Current station icon
    [SerializeField] private Image rightIcon;  // Next station icon

    [SerializeField] private Sprite[] stationIcons;  // Array of station icons
    //[SerializeField] private Sprite[] stationFullImages; // Array of full station images

    private int currentIndex = 0; // Start at Knife Station (index 0)

    public void OnButtonClick(string buttonFunc)
    {
        // Find the local player and send the event
        NetworkEventManager localPlayer = FindLocalPlayer().GetComponent<NetworkEventManager>();
        localPlayer.SendEventToServer(buttonFunc);

        // Handle UI update
        if (buttonFunc == "Up")
        {
            ShowPreviousStation();
        }
        else if (buttonFunc == "Down")
        {
            ShowNextStation();
        }
    }

    public void StartGame ()
    {
        FindLocalPlayer().GetComponent<NetworkEventManager>().StartGame();

        // Ensure all UI elements are fully visible
        SetFullAlpha(leftIcon);
        SetFullAlpha(centerIcon);
        SetFullAlpha(rightIcon);

        // Display the Knife Station (index 0)
        UpdateStationDisplay();

        GameManager.instance.playerStartedGame = true;
    }

    public void StartCooking ()
    {
        Debug.Log (GameManager.instance.playerStartedGame);
        if (GameManager.instance.playerStartedGame && !GameManager.instance.isCookingRecipe)
        {
            GameManager.instance.isCookingRecipe = true;
            //NetworkEventManager.instance.UpdateVariableInClinet (true);
        }
    }

    private void ShowNextStation()
    {
        // Move to the next index, looping if needed
        currentIndex = (currentIndex + 1) % stationIcons.Length;
        UpdateStationDisplay();
    }

    private void ShowPreviousStation()
    {
        // Move to the previous index, looping if needed
        currentIndex = (currentIndex - 1 + stationIcons.Length) % stationIcons.Length;
        UpdateStationDisplay();
    }

    private void UpdateStationDisplay()
    {
        // Update icons
        centerIcon.sprite = stationIcons[currentIndex];
        leftIcon.sprite = stationIcons[(currentIndex - 1 + stationIcons.Length) % stationIcons.Length];
        rightIcon.sprite = stationIcons[(currentIndex + 1) % stationIcons.Length];
    }

    private void SetFullAlpha(Image img)
    {
        if (img != null)
        {
            Color color = img.color;
            color.a = 1f; // Set alpha to max
            img.color = color;
        }
    }

    private NetworkBehaviour FindLocalPlayer()
    {
        NetworkBehaviour[] allNetworkBehaviours = FindObjectsOfType<NetworkBehaviour>();
        foreach (var netBehaviour in allNetworkBehaviours)
        {
            if (netBehaviour.isLocalPlayer)
            {
                return netBehaviour;
            }
        }
        
        return null;
    }
}
