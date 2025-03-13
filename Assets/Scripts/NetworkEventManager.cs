using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkEventManager : NetworkBehaviour
{
    public static NetworkEventManager instance;

    // Define a delegate for handling button events
    public delegate void ButtonEventHandler(string buttonFunc);
    public static event ButtonEventHandler OnButtonPressed;
    private string currentstation;

    [HideInInspector] public string outputStepName;

    void OnEnable()
    {
       GameManager.instance.OnCurrentStationChanged += (station) =>
        {
            if (isServer)
            {
                ClientUpdateCurrentStation(station);
            }
        };
    }

    void OnDisable()
    {
        if (!isServer) return;

        GameManager.instance.OnCurrentStationChanged -= (station) =>
        {
            if (isServer)
            {
                ClientUpdateCurrentStation(station);
            }
        };
    }

    private void Awake()
    {
        instance = this;
    }

    public void SendEventToServer(string buttonFunc)
    {
        Debug.Log($"[CLIENT] Sending event to server: {buttonFunc}");
        CmdTriggerEvent(buttonFunc);
    }

    // Command: Runs only on the Server
    [Command(requiresAuthority = false)]
    void CmdTriggerEvent(string buttonFunc, NetworkConnectionToClient sender = null)
    {
        Debug.Log($"[SERVER] Received event: {buttonFunc}");

        // Broadcast the event to all listeners
        OnButtonPressed?.Invoke(buttonFunc);
    }

    [Command(requiresAuthority = false)]
    public void StartGame ()
    {
        GameManager.instance.playerStartedGame = true;
    }

    [Command] 
    public void CmdSendSelectedIngredients(List<string >ingredients)
    {
        // GameObject ingridient = prefab;
        // ingridient.GetComponent<InventoryItem> ().ingredient = ingredient;
        // ingridient.GetComponent<UnityEngine.UI.Image> ().sprite = ingredient.icon;        

        GameManager.instance.InstantiateIngredientById (ingredients);
    }

    // Get output name from here so that current station can be set from the game manager in server. If it is said from TargetRequestCookingStepOutput () 
    // its gonna try to get it from the client side game manager
    public string RequestCookingStepOutput ()
    {
        currentstation = GameManager.instance.currentStation;
        TargetRequestCookingStepOutput ();
        return outputStepName;
    }
    
    [ClientRpc]
    void TargetRequestCookingStepOutput()
    {
        if (isServer) return;
        //GameManager.instance.cookingManager.currentStation = currentstation;
        GameManager.instance.cookingManager.GetValidCookingStepName ();
    }

    [Command]
    public void CmdSetOutputStepName(string stepOutputName)
    {
        // Set the output step name on the server (or on the NetworkManager)
        GameManager.instance.cookingOutputName = stepOutputName;
    }

    [ClientRpc]
    public void SendIngredientToInventory (string outPutName)
    {
        if (isServer) return;

        GameManager.instance.cookingManager.AddIngredient (outPutName);
    }

    [Command]
    public void ResetCookingOutput ()
    {
        GameManager.instance.cookingOutputName = "";
    }

    [ClientRpc]
    public void ClientUpdateCurrentStation (string station)
    {
        if (isServer) return;

        GameManager.instance.cookingManager.currentStation = station;
    }
    // [TargetRpc, Command]
    // public void UpdateVariableInClinet (bool variable)   
    // {
    //     GameManager.instance.isCookingRecipe = variable;
    // }



}
