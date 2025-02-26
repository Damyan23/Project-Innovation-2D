using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector] public RecipeDataBase dataBase;
    [HideInInspector] public EventManager eventManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
            Debug.Log($"Accelerometer Supported: {SystemInfo.supportsAccelerometer}");
    Debug.Log($"Gyroscope Supported: {SystemInfo.supportsGyroscope}");
    } 

    void Update()
    {
        // Check if accelerometer is available
    }

    private void OnEnable()
    {
        dataBase = this?.GetComponent<RecipeDataBase>();
        eventManager = this?.GetComponent<EventManager>();
    } 
}
