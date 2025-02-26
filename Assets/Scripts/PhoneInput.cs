using UnityEngine;
using Mirror;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    private Vector3 lastAcceleration;
    private Vector3 lastGyroRotation;

    private float lastKnifeMotionTime = 0f; // Prevents multiple detections
    private float knifeMotionCooldown = 0.3f; // Time between detections

    [Command]
    void CmdSendInput(Vector2 touchPosition, Vector3 accelerometer, Vector3 gyroRotation, Vector3 magnetometer, NetworkConnectionToClient sender = null)
    {
        if (accelerometer != lastServerAccel)
        {
            // Debug.Log($"📡 [SERVER] Received Input from Client {sender.connectionId}:\n" +
            //         $"📍 Touch Position: {touchPosition}\n" +
            //         $"📈 Accel: {accelerometer}\n" +
            //         $"🌀 Gyro: {gyroRotation}\n" +
            //         $"🧭 Magneto: {magnetometer}");
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

        // // Debug log only on client
        // Debug.Log($"📱 [CLIENT] Sensor Data:\n" +
        //           $"Accelerometer: {accelerometer}\n" +
        //           $"Gyroscope: {gyroRotation}\n" +
        //           $"Magnetometer: {magnetometer}");

        Vector2 touchPos = Vector2.zero;
        // Send data to server on touch or click
        if (Input.touchCount > 0)
        {
            touchPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            Debug.Log($"📤 Sending input to server: {touchPos}");
        }
        CmdSendInput(touchPos, accelerometer, gyroRotation, magnetometer);

        DetectKnifeMotion ();
    }

    void DetectKnifeMotion()
    {
        if (Time.time - lastKnifeMotionTime < knifeMotionCooldown)
            return; // Prevent rapid multiple detections

        Vector3 acceleration = Input.acceleration;
        Vector3 gyroRotation = Input.gyro.rotationRate;
        Vector3 gravity = Input.gyro.gravity; // Used to detect orientation

        // ✅ Step 1: Detect if the phone is sideways (thin side up)
        bool isSideways = Mathf.Abs(gravity.x) > 0.7f; // If x is large, phone is on its sidex > 0.7

        // ✅ Step 3: Detect proper up/down motion (instead of side-to-side)
        bool isFastDownward = acceleration.z < -0.6f; // Moving down when sideways
        bool isFastUpward = acceleration.z > 0.6f;    // Moving up when sideways
        bool isRotatingFast = Mathf.Abs(gyroRotation.y) > 1.8f; // Rotating along correct axis

        // ✅ Step 4: Detect knife motion ONLY when phone is sideways & power button is up
        if (isSideways && (isFastDownward || isFastUpward) && isRotatingFast)
        {
            Debug.Log("🔪 Knife Motion Detected!");
            lastKnifeMotionTime = Time.time; // Update cooldown timer
        }

        lastAcceleration = acceleration;
        lastGyroRotation = gyroRotation;
    }
}
