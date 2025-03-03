using UnityEngine;
using System;
using System.Collections.Generic;
public class EventManager : MonoBehaviour
{
    public static Action OnButtonUpEvent;
    public static Action OnButtonDownEvent;

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
