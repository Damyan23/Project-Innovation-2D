using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InventoryItem : MonoBehaviour
{
    // Start is called before the first frame update

    public CookingTester testerScript;
    public Ingredient ingredient;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        testerScript.SelectInventoryItem(this);
    }
}
