using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class InventoryItem : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector] public CookingManager manager;
    [HideInInspector] public Ingredient ingredient;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        manager.SelectInventoryItem(this);
    }
}
