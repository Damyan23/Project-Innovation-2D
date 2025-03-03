using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InteractiveObject : MonoBehaviour
{
    public void OnButtonClick(string buttonFunc)
    {
        // Find a NetworkBehaviour that is the local player
        NetworkEventManager localPlayer = FindLocalPlayer().GetComponent<NetworkEventManager>();
        localPlayer.SendEventToServer (buttonFunc);
    }
    
    private NetworkBehaviour FindLocalPlayer()
    {
        // Find all player objects with NetworkBehaviour
        NetworkBehaviour[] allNetworkBehaviours = FindObjectsOfType<NetworkBehaviour>();
        foreach (var netBehaviour in allNetworkBehaviours)
        {
            Debug.Log (netBehaviour.isLocalPlayer);
            // Check if this is the local player
            if (netBehaviour.isLocalPlayer)
            {
                return netBehaviour;
            }
        }
        
        return null;
    }
}