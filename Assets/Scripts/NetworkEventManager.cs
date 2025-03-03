using UnityEngine;
using Mirror;

public class NetworkEventManager : NetworkBehaviour
{
    // Define a delegate for handling button events
    public delegate void ButtonEventHandler(string buttonFunc);
    public static event ButtonEventHandler OnButtonPressed;

    public void SendEventToServer(string buttonFunc)
    {
        Debug.Log($"[CLIENT] Sending event to server: {buttonFunc}");
        CmdTriggerEvent(buttonFunc);
    }

    // Command: Runs only on the Server
    [Command(requiresAuthority = false)]
    void CmdTriggerEvent(string buttonFunc, NetworkConnectionToClient sender = null)
    {
        Debug.Log($"[SERVER] Received event: {buttonFunc}");

        // Broadcast the event to all listeners
        OnButtonPressed?.Invoke(buttonFunc);
    }
}
