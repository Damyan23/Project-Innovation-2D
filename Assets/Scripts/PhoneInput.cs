using UnityEngine;
using Mirror;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    [Command]
    void CmdSendInput(Vector2 touchPosition, Vector3 accelerometer, Vector3 gyroRotation, Vector3 magnetometer, NetworkConnectionToClient sender = null)
    {
        if (accelerometer != lastServerAccel)
        {
            Debug.Log($"📡 [SERVER] Received Input from Client {sender.connectionId}:\n" +
                    $"📍 Touch Position: {touchPosition}\n" +
                    $"📈 Accel: {accelerometer}\n" +
                    $"🌀 Gyro: {gyroRotation}\n" +
                    $"🧭 Magneto: {magnetometer}");
            lastServerAccel = accelerometer;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("📱 Client started!");

        // Enable sensors
        Input.gyro.enabled = true;
        Input.compass.enabled = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("🖥️ Server started!");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("🎮 Local player started - we have authority!");
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || 
            Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("🔗 Starting Host on Windows...");
            NetworkManager.singleton.StartHost();
        }
        else // Assume mobile platform
        {
            Debug.Log($"📱 Starting Client on mobile device. Connecting to: {NetworkManager.singleton.networkAddress}");
            NetworkManager.singleton.StartClient();
        }
    }

    void Update()
    {
        // Prevent the server from running this part
        if (isServer || !NetworkClient.active || !NetworkClient.isConnected)
        {
            return;
        }

        // Get Sensor Data
        Vector3 accelerometer = Input.acceleration;
        Vector3 gyroRotation = Input.gyro.rotationRate;
        Vector3 magnetometer = Input.compass.rawVector;

        // Debug log only on client
        Debug.Log($"📱 [CLIENT] Sensor Data:\n" +
                  $"Accelerometer: {accelerometer}\n" +
                  $"Gyroscope: {gyroRotation}\n" +
                  $"Magnetometer: {magnetometer}");

        Vector2 touchPos = Vector2.zero;
        // Send data to server on touch or click
        if (Input.touchCount > 0)
        {
            touchPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            Debug.Log($"📤 Sending input to server: {touchPos}");
        }
        CmdSendInput(touchPos, accelerometer, gyroRotation, magnetometer);
    }
}
