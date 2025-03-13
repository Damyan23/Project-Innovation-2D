using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class PhoneInput : NetworkBehaviour
{
    private Vector3 lastServerAccel;

    private Vector3 lastAcceleration;
    private Vector3 lastGyroRotation;

    private float lastMotionTime = 0f; // Prevents multiple detections
    private float motionCooldown = 0.35f; // Time between detections

    private CookingManager cookingManager;
    private float platingStartTime = 0f;

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
            case "cutting":
                DetectKnifeMotion();
                break;
            case "mixing":
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

        bool isSideways = Mathf.Abs(gravity.y) > 0.7f;
        bool isFastDownward = Mathf.Abs (acceleration.y) > 1.5f;
        bool isRotatingFast = Mathf.Abs (gyroRotation.y) > 1.5f;


        Debug.Log ("is sideways :" + isSideways);
        Debug.Log ("is fast downward" + isFastDownward);
        // Debug.Log ("is fast downward" + isFastDownward);
        // Debug.Log (isRotatingFast);

        // Check cooldown
        if (Time.time - lastMotionTime < motionCooldown)
        {
            return; // Exit if still in cooldown
        }


        if (isSideways && isFastDownward)
        {
            SendKnifeDetection();
            lastMotionTime = Time.time;
            Debug.Log("Detected knife motion");
        }
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

        //Debug.Log(Input.deviceOrientation);

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



    void DetectPlatingMotion()
    {
        Vector3 acceleration = Input.acceleration;
        Vector3 gravity = Input.gyro.gravity;
        
        bool isGoingDown = Mathf.Abs (acceleration.z) > 1.5f;
        bool correctOrientation = Mathf.Abs (gravity.z) > 0.75;

        Debug.Log ("is going down :" + isGoingDown);
        Debug.Log ("correct orientation :" + correctOrientation);

        // Detect plating motion - in landscape and screen tilts up
        // Using a more generous time window of 1.5 seconds
        if (isGoingDown && correctOrientation && (Time.time - platingStartTime > 1.5f))
        {
            SendPlatingDetection();
            Debug.Log("Detected plating motion - landscape with forward tilt");
            
            platingStartTime = Time.time;
        }
    }

}
