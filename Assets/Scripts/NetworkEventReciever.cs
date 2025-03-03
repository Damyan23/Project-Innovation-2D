using UnityEngine;

public class NetworkEventReceiver : MonoBehaviour
{
    void OnEnable()
    {
        // Subscribe to the event when enabled
        NetworkEventManager.OnButtonPressed += HandleButtonEvent;
    }

    void OnDisable()
    {
        // Unsubscribe when disabled (to prevent memory leaks)
        NetworkEventManager.OnButtonPressed -= HandleButtonEvent;
    }

    void HandleButtonEvent(string buttonFunc)
    {
        Debug.Log($"[EVENT RECEIVER] Received event: {buttonFunc}");

        switch (buttonFunc)
        {
            case "Up":
                GameManager.instance.eventManager.OnButtonUp();
                break;
            case "Down":
                GameManager.instance.eventManager.OnButtonDown();
                break;
            default:
                Debug.Log($"[EVENT RECEIVER] Unknown event: {buttonFunc}");
                break;
        }
    }
}
