using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    public string pcScene = "PC Scene";      // Scene for the server/PC
    public string phoneScene = "Phone Scene"; // Scene for the client/phone

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SceneManager.LoadScene(pcScene);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        if (!NetworkServer.active) 
        {
            SceneManager.LoadScene(phoneScene);
        }

        // If this is a client, set the client ready
        if (NetworkClient.connection is NetworkConnectionToClient clientConnection)
        {
            NetworkServer.SetClientReady(clientConnection);
        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        
        // Automatically set the client ready when they connect to the server
        NetworkServer.SetClientReady(conn);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterDisabledNetworkObjects();
    }

    void RegisterDisabledNetworkObjects()
    {
        // Find all disabled NetworkIdentity objects
        NetworkIdentity[] allObjects = FindObjectsOfType<NetworkIdentity>(true);

        foreach (NetworkIdentity netIdentity in allObjects)
        {
            if (!netIdentity.gameObject.activeSelf) // If disabled
            {
                netIdentity.gameObject.SetActive(true); // Enable it
            }
        }
    }
}