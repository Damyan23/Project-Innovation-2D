using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool playerStartedGame = false;

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

    private void OnEnable()
    {
        dataBase = this?.GetComponent<RecipeDataBase>();
        eventManager = this?.GetComponent<EventManager>();

        eventManager.startGameEvent += () => playerStartedGame = true;
    } 
}
