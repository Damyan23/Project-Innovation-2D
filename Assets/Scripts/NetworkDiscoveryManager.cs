using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class NetworkDiscoveryManager : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;

    private void Start()
    {
        networkDiscovery = GetComponent<NetworkDiscovery>();
    }

    // Start Hosting + Advertise Server
    public void StartHost()
    {
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer(); // Broadcast server presence
        Debug.Log("Hosting started and advertising...");
    }

    // Start Searching for Servers
    public void StartClient()
    {
        Debug.Log("Searching for servers...");
        networkDiscovery.StartDiscovery();
    }

    // Callback for when a server is found
    public void OnServerFound(ServerResponse response)
    {
        Debug.Log($"Server found at {response.uri}");

        // Set the found server IP and connect to it
        NetworkManager.singleton.networkAddress = response.uri.Host;
        NetworkManager.singleton.StartClient();
    }
}
