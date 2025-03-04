using UnityEngine;
using System;
using System.Collections.Generic;
public class EventManager : MonoBehaviour
{
    public Action OnButtonUpEvent;
    public Action OnButtonDownEvent;
    public Action startGameEvent;

    void OnEnable()
    {
        startGameEvent += startGame;
    }

    void OnDisable()
    {
        startGameEvent -= startGame;
    }

    public void startGame ()
    {
        startGameEvent.Invoke ();
        GameManager.instance.playerStartedGame = true;
    }
    public void OnButtonUp ()
    {
        OnButtonUpEvent.Invoke ();
        Debug.Log ("Button Up");
    }

    public void OnButtonDown ()
    {
        OnButtonDownEvent.Invoke ();
        Debug.Log ("Button Down");
    }
}
