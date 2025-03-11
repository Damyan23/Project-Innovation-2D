using UnityEngine;
using Mirror;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    private Vector3 lastAcceleration;
    private Vector3 lastGyroRotation;

    private float lastMotionTime = 0f; // Prevents multiple detections
    private float motionCooldown = 0.3f; // Time between detections

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
        Debug.Log("Sent mixing detection to server");
    }

    private float lastZAcceleration = 0f;
    private bool wasMovingRight = false;
    private int rockingCount = 0;
    private float rockingTimer = 0f;

    void DetectMixingMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gravity = Input.gyro.gravity;
        
        // Check if thin edge (width side) is pointing up
        bool isWidthSideUp = Mathf.Abs(gravity.x) > 0.7f;
        
        if (isWidthSideUp)
        {
            // Track time for detecting rocking frequency
            rockingTimer += Time.deltaTime;
            
            // Check for direction change in z-axis (left-right rocking)
            bool isMovingRight = acceleration.z > 0.3f;
            bool isMovingLeft = acceleration.z < -0.3f;
            
            // Detect change in direction (indicates rocking motion)
            if ((isMovingRight && !wasMovingRight && lastZAcceleration < -0.2f) || 
                (!isMovingRight && wasMovingRight && lastZAcceleration > 0.2f))
            {
                rockingCount++;
                wasMovingRight = isMovingRight;
            }
            
            // Reset detection if too much time passes
            if (rockingTimer > 2.0f)
            {
                rockingTimer = 0f;
                rockingCount = 0;
            }
            
            // If we detect multiple direction changes within time window, it's a rocking motion
            if (rockingCount >= 3)
            {
                SendMixingDetection();
                Debug.Log("Detected stirring/mixing motion");
                
                // Reset after detection
                rockingCount = 0;
                rockingTimer = 0f;
            }
            
            // Store current acceleration for next frame comparison
            lastZAcceleration = acceleration.z;
        }
        else
        {
            // Reset when phone is not in correct orientation
            rockingCount = 0;
            rockingTimer = 0f;
        }
    }

    [Command(requiresAuthority = false)]
    void SendPlatingDetection()
    {
        GameManager.instance.cookRecipeEvent?.Invoke();
        Debug.Log("Sent plating detection to server");
    }

    void DetectPlatingMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gravity = Input.gyro.gravity;

        bool isFlat = gravity.z > 0.7f;
        bool isTapping = Mathf.Abs(acceleration.y) > 0.5f;

        if (isFlat && isTapping)
        {
            SendPlatingDetection();
            Debug.Log("Detected plating motion");
        }
    }
}
