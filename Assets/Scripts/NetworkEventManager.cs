using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NetworkEventManager : NetworkBehaviour
{

    // Client calls this when button is clicked
    public void SendEventToServer()
    {
        if (!isServer)
        {
            Debug.Log("[CLIENT] Sending event to server...");
            CmdTriggerEvent("Button Clicked!"); 
        }
    }

    // Command: Runs only on the Server
    [Command]
    void CmdTriggerEvent(string eventMessage, NetworkConnectionToClient sender = null)
    {
        Debug.Log($"📡 [SERVER] Event received from Client {sender.connectionId}: {eventMessage}");
        
        // Call a function on the server (e.g., spawn an object)
        ServerHandleEvent();
    }

    // This function only runs on the server
    void ServerHandleEvent()
    {
        Debug.Log("🖥️ [SERVER] Handling event (e.g., spawning an object)...");
    }
}
