using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Mirror.Discovery;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject networkMenu;
    [SerializeField] private NetworkDiscovery networkDiscovery;
    private Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    
    private void Start()
    {
        networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
    }

    public void GoToNetworkMenu()
    {
        mainMenu.SetActive(false);
        networkMenu.SetActive(true);
    }

    public void StartHost()
    {
        Debug.Log("Starting host...");
        discoveredServers.Clear ();
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    public void JoinServer()
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
        StartCoroutine(TryJoinServer());
    }

    private IEnumerator TryJoinServer()
    {
        float timeout = 10f; // Wait for servers to be discovered
        float timer = 0f;

        while (discoveredServers.Count == 0 && timer < timeout)
        {
            yield return new WaitForSeconds(1f);
            timer += 1f;
        }

        if (discoveredServers.Count > 0)
        {
            foreach (ServerResponse info in discoveredServers.Values)
            {
                Debug.Log("Trying to connect to: " + info.EndPoint.Address);
                NetworkManager.singleton.networkAddress = info.EndPoint.Address.ToString();
                NetworkManager.singleton.StartClient();
                yield break; // Stop after first successful attempt
            }
        }
        else
        {
            Debug.LogError("No servers found.");
        }
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        if (!discoveredServers.ContainsKey(info.serverId))
        {
            discoveredServers[info.serverId] = info;
        }
    }
}
