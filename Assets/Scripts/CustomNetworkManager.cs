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

    new void OnDestroy()
    {
        // Unsubscribe when the object is destroyed (prevents memory leaks)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("🖥️ Server started! Loading PC scene...");
        SceneManager.LoadScene(pcScene);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // Load client scene only for remote clients (not the host client)
        if (!NetworkServer.active) 
        {
            Debug.Log("📱 Client connected! Loading Phone scene...");
            SceneManager.LoadScene(phoneScene);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Searching for disabled NetworkIdentity objects...");

        // Find all objects in the scene
        NetworkIdentity[] allObjects = FindObjectsOfType<NetworkIdentity>(true); // true finds disabled objects

        foreach (NetworkIdentity netIdentity in allObjects)
        {
            if (!netIdentity.gameObject.activeSelf) // Check if it's disabled
            {
                Debug.Log($"✅ Enabling & Registering: {netIdentity.gameObject.name}");
                netIdentity.gameObject.SetActive(true); // Enable the object
            }

            // Ensure the object is correctly registered in the scene
            if (!netIdentity.isServer)
            {
                NetworkServer.Spawn(netIdentity.gameObject);
            }
        }

        Debug.Log("All disabled NetworkIdentity objects have been registered.");
    }
}
