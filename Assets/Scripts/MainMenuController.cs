using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject networkMenu;
    public void GoToNetworkMenu ()
    {
        mainMenu.SetActive (false);
        networkMenu.SetActive (true);
    }

    public void StartHost()
    {
        Debug.Log("🖥️ Starting Host...");
        NetworkManager.singleton.StartHost();
    }

    // For when we start using IP adresses
        // void JoinServer()
        // {
        //     string ipAddress = ipInputField.text.Trim();
        //     if (string.IsNullOrEmpty(ipAddress))
        //     {
        //         Debug.LogError("❌ Please enter a valid IP address!");
        //         return;
        //     }

        //     Debug.Log($"📱 Connecting to server at: {ipAddress}");
        //     NetworkManager.singleton.networkAddress = ipAddress;
        //     NetworkManager.singleton.StartClient();
        // }

    public void JoinServer()
    {
        // Debug.Log($"📱 Connecting to server at: {}");
        NetworkManager.singleton.networkAddress = GetIp();
        NetworkManager.singleton.StartClient();
    }

    private string GetIp()
    {
        return "localHost";
    }
}
