using UnityEngine;
using Mirror;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    private Vector3 lastAcceleration;
    private Vector3 lastGyroRotation;

    private float lastMotionTime = 0f; // Prevents multiple detections
    private float motionCooldown = 0.35f; // Time between detections

    private CookingManager cookingManager;

    [Command(requiresAuthority = false)]
    void CmdSendInput(Vector2 touchPosition, Vector3 accelerometer, Vector3 gyroRotation, Vector3 magnetometer, NetworkConnectionToClient sender = null)
    {
        if (accelerometer != lastServerAccel)
        {
            lastServerAccel = accelerometer;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
        {
            if (!isServer) 
            {
                cookingManager = GameManager.instance.cookingManager;
            }
            StartCoroutine(RequestAuthorityDelayed());
        }
    }
    
    private System.Collections.IEnumerator RequestAuthorityDelayed()
    {
        yield return null; 
        CmdRequestAuthority(connectionToClient);
    }
    
    [Command(requiresAuthority = false)]
    public void CmdRequestAuthority(NetworkConnectionToClient conn = null)
    {
        if (conn != connectionToClient)
        {
            Debug.Log($"Granting authority to {conn.connectionId} for {gameObject.name}");
            netIdentity.AssignClientAuthority(conn);
        }
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

        if (!isServer && isLocalPlayer)
        {
            DetectMotion();
        }
    }

    void DetectMotion()
    {
        if (cookingManager == null) return;
        
        switch (cookingManager.currentStation)
        {
            case "knife":
                DetectKnifeMotion();
                break;
            case "soup":
                DetectMixingMotion();
                break;
            case "plating":
                DetectPlatingMotion();
                break;
        }
    }

    [Command(requiresAuthority = false)]
    void SendKnifeDetection()
    {
        GameManager.instance.cookRecipeEvent?.Invoke();
        GameManager.instance.isCookingRecipe = true;
        Debug.Log("Sent knife detection to server");
    }

    void DetectKnifeMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gyroRotation = Input.gyro.rotationRate;
        Vector3 gravity = Input.gyro.gravity;

        bool isSideways = Mathf.Abs(gravity.x) > 0.7f;
        bool isFastDownward = acceleration.z < -0.5f;
        bool isFastUpward = acceleration.z > 0.5f;
        bool isRotatingFast = Mathf.Abs(gyroRotation.y) > 1.5f;

        if (isSideways && (isFastDownward || isFastUpward) && isRotatingFast)
        {
            SendKnifeDetection();
            Debug.Log("Detected knife motion");
        }

        lastAcceleration = acceleration;
        lastGyroRotation = gyroRotation;
    }

    [Command(requiresAuthority = false)]
    void SendMixingDetection()
    {
        GameManager.instance.cookRecipeEvent?.Invoke();
        GameManager.instance.isCookingRecipe = true;
        Debug.Log("Sent mixing detection to server");
    }

    void DetectMixingMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gravity = Input.gyro.gravity;

        // Check if the phone's long edge is pointing up (landscape mode)
        bool isOnSide = Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight;

        // Detect left-right movement
        float movementThreshold = 0.7f; // Adjust for sensitivity
        bool isMovingRight = acceleration.x > movementThreshold;
        bool isMovingLeft = acceleration.x < -movementThreshold;

        // Check cooldown
        if (Time.time - lastMotionTime < motionCooldown)
        {
            return; // Exit if still in cooldown
        }

        Debug.Log(Input.deviceOrientation);

        if (isOnSide && (isMovingRight || isMovingLeft))
        {
            SendMixingDetection();
            Debug.Log("Detected mixing motion");

            lastMotionTime = Time.time; // Update last detection time
        }
    }

    [Command(requiresAuthority = false)]
    void SendPlatingDetection()
    {
        GameManager.instance.cookRecipeEvent?.Invoke();
        GameManager.instance.isCookingRecipe = true;
        Debug.Log("Sent plating detection to server");
    }

    private bool wasLandscape = false;
    private float platingStartTime = 0f;

    void DetectPlatingMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gravity = Input.gyro.gravity;
        
        // Check if phone is in landscape mode
        bool isLandscape = Input.deviceOrientation == DeviceOrientation.LandscapeLeft || 
                        Input.deviceOrientation == DeviceOrientation.LandscapeRight;
        
        // Check if screen is pointing upward (forward tilt)
        // Using a more generous threshold to make it easier
        bool isScreenUp = gravity.z < -0.4f;
        
        // Check cooldown
        if (Time.time - lastMotionTime < motionCooldown)
        {
            return; // Exit if still in cooldown
        }
        
        // Start tracking when in landscape
        if (isLandscape && !wasLandscape)
        {
            wasLandscape = true;
            platingStartTime = Time.time;
        }
        
        // Detect plating motion - in landscape and screen tilts up
        // Using a more generous time window of 1.5 seconds
        if (wasLandscape && isLandscape && isScreenUp && 
            (Time.time - platingStartTime < 1.5f))
        {
            SendPlatingDetection();
            Debug.Log("Detected plating motion - landscape with forward tilt");
            
            // Reset detection state
            wasLandscape = false;
            lastMotionTime = Time.time;
        }
        
        // Reset if we leave landscape mode or too much time passes
        if ((!isLandscape && wasLandscape) || 
            (wasLandscape && (Time.time - platingStartTime > 1.5f)))
        {
            wasLandscape = false;
        }
    }

}
