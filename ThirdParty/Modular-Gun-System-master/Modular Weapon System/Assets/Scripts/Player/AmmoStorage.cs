using System.Collections.Generic;
using UnityEngine;

public class AmmoStorage : MonoBehaviour
{
    [SerializeField] int[] startingAmmoAmounts;
    [SerializeField] private GenericKey _ammoInventoryKey;

    private InventoryDefinition _ammoInventory => DefaultPlayerInventory.Instance.GetInventoryDefinition(_ammoInventoryKey.ID);

    void Start()
    {
        
    }

    public int GetAmmoAmount(ItemDefinition itemDefinition)
    {
        Debug.Log("Inventory count : "+DefaultPlayerInventory.Instance.InventoryDefinitions.Count);
        return _ammoInventory.GetItemCount(itemDefinition);
    }

    public void ReduceAmmoAmount(ItemDefinition itemDefinition, int amount){
        _ammoInventory.RemoveItem(itemDefinition,amount);
    }

    void OnValidate(){
        int[] temp = startingAmmoAmounts;
        InitialiseArraysInInspector();

        if (temp == null) return;
        // needed to stop old values set in inspector from being overwritten on validation
        startingAmmoAmounts = temp;
    }

    /**
    * Used to initialise two equal length arrays based on ammo categories available and set
    * each element in startingAmmoCategories equal to the enums available
    */
    void InitialiseArraysInInspector(){
        string[] ammoCategories = System.Enum.GetNames(typeof(AmmoCategory));
        int categoryCount = ammoCategories.Length;
    }
}
