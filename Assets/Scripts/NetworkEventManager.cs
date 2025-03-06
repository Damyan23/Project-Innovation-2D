using UnityEngine;
using Mirror;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine.UI;

public class NetworkEventManager : NetworkBehaviour
{
    public static NetworkEventManager instance;

    // Define a delegate for handling button events
    public delegate void ButtonEventHandler(string buttonFunc);
    public static event ButtonEventHandler OnButtonPressed;
    public GameObject idk;

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
    public void CmdSpawnIngridient(string ingredient)
    {
        // GameObject ingridient = prefab;
        // ingridient.GetComponent<InventoryItem> ().ingredient = ingredient;
        // ingridient.GetComponent<UnityEngine.UI.Image> ().sprite = ingredient.icon;        

        GameManager.instance.InstantiateIngredientById (ingredient);
    }

    
    [TargetRpc]
    public void SendCookedIngredient (Ingredient output, string currentStation)
    {
        GameManager.instance.cookingManager.selectedIngredients[currentStation].Clear();
        GameManager.instance.cookingManager.AddIngredient(output);
        Debug.Log ("added ingredient");
    }

    [TargetRpc, Command]
    public void UpdateVariableInClinet (bool variable)
    {
        GameManager.instance.isCookingRecipe = variable;
    }



}
