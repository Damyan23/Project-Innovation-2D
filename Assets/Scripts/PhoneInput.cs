using UnityEngine;
using Mirror;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    private Vector3 lastAcceleration;
    private Vector3 lastGyroRotation;

    private float lastKnifeMotionTime = 0f; // Prevents multiple detections
    private float knifeMotionCooldown = 0.3f; // Time between detections

    [Command(requiresAuthority = false)]
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
        
        // Only call CmdRequestAuthority if this is the local player
        if (isLocalPlayer)
        {
            // Wait a frame to ensure network state is initialized
            StartCoroutine(RequestAuthorityDelayed());
        }
    }
    
    private System.Collections.IEnumerator RequestAuthorityDelayed()
    {
        yield return null; // Wait a frame
        
        CmdRequestAuthority(connectionToClient);
    }
    
    [Command(requiresAuthority = false)]
    public void CmdRequestAuthority(NetworkConnectionToClient conn = null)
    {
        // Make sure we don't already have authority
        if (conn != connectionToClient)
        {
            Debug.Log($"Granting authority to {conn.connectionId} for {gameObject.name}");
            netIdentity.AssignClientAuthority(conn);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device!");
        }
    }

    void Update()
    {
        if (isServer)
        {
            return;
        }

        // Get Sensor Data
        Vector3 accelerometer = Input.acceleration;
        Vector3 gyroRotation = Input.gyro.rotationRate;
        Vector3 magnetometer = Input.compass.rawVector;

        Vector2 touchPos = Vector2.zero;
        //CmdSendInput(touchPos, accelerometer, gyroRotation, magnetometer);

            //DebugData ();

        if (Input.GetKeyDown(KeyCode.F) && !isServer && isLocalPlayer)
        {
            SendKnifeDetection();
            Debug.Log("Knife thingy");
        }

        //if (GameManager.instance.isCookingRecipe) { DetectKnifeMotion();  }
        //if (!isServer) DetectKnifeMotion();
    }

    void DebugData ()
    {
        // Debug log only on client
        // Debug.Log($"[CLIENT] Sensor Data:\n" +
        //           $"Accelerometer: {accelerometer}\n" +
        //           $"Gyroscope: {gyroRotation}\n" +
        //           $"Magnetometer: {magnetometer}");
    }

    [Command (requiresAuthority = false)]
    void SendKnifeDetection ()
    {
        GameManager.instance.cookRecipeEvent?.Invoke();
        GameManager.instance.isCookingRecipe = true;
        Debug.Log ("Sent knife detection to server");
    }

    void DetectKnifeMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gyroRotation = Input.gyro.rotationRate;
        Vector3 gravity = Input.gyro.gravity; // Used to detect orientation

        //Detect if the phone is sideways (thin side up)
        bool isSideways = Mathf.Abs(gravity.x) > 0.7f; // If x is large, phone is on its sidex > 0.7

        //Detect proper up/down motion (instead of side-to-side)
        bool isFastDownward = acceleration.z < -0.5f; // Moving down when sideways
        bool isFastUpward = acceleration.z > 0.5f;    // Moving up when sideways
        bool isRotatingFast = Mathf.Abs(gyroRotation.y) > 1.5f; // Rotating along correct axis

        //Detect knife motion ONLY when phone is sideways & power button is up
        if (isSideways && (isFastDownward || isFastUpward) && isRotatingFast)
        {
            SendKnifeDetection ();
            Debug.Log ("Detected knife motion");
        }

        

        lastAcceleration = acceleration;
        lastGyroRotation = gyroRotation;
    }
}
