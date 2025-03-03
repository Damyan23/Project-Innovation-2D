using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SpawnNetworkObjects : NetworkBehaviour
{
    [SerializeField] private List<GameObject> clientObjects;
    [SerializeField] private List<GameObject> serverObjects;
    [SerializeField] private List<GameObject> networkObjects;

    public Scene clientScene;
    public Scene serverScene;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ensure there are enough scenes loaded
        if (SceneManager.sceneCount > 1)
        {
            clientScene = SceneManager.GetSceneAt(1);
        }
        if (SceneManager.sceneCount > 2)
        {
            serverScene = SceneManager.GetSceneAt(2);
        }
        Debug.Log (clientScene);

        CmdRequestSpawn();
    }

    [Command (requiresAuthority = false)]
    void CmdRequestSpawn()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SpawnObjects(currentScene);
        Debug.Log ("Going into command");
    }

    void SpawnObjects(Scene scene)
    {
        // Ensure only the server spawns objects
        if (!isServer) return;


        foreach (GameObject obj in clientObjects)
        {
            GameObject instance = Instantiate(obj);
            NetworkServer.Spawn(instance, this.gameObject);
            Debug.Log ("spawning obj");
        }
        if (scene == clientScene)
        {
        }
        else if (scene == serverScene)
        {
            foreach (GameObject obj in serverObjects)
            {
                GameObject instance = Instantiate(obj);
                NetworkServer.Spawn(instance);
            }
        }
    }
}
