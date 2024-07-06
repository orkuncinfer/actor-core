using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[TopTitle(ShowGenericName = false,
    NameSuffix = "<color=#ffff00><b>₪ITEM</b></color>",
    NamePrefix = "<color=#ffff00><b>₪</b></color>",
    PerGenericArgString = ",", SetParentObject = true,
    BoldName = true)]
[GUIColor(0.9f, 0.9f, 0f)]
public struct ItemField : ISerializationCallbackReceiver
{
    private UnityEngine.Object _parentObject;

    public UnityEngine.Object parentObject
    {
        get => _parentObject;
        set => _parentObject = value;
    }
    
    [FormerlySerializedAs("_itemDefinition")]
    [ValueDropdown("GetAllAppropriateKeys")]
    [OnValueChanged("OnEventKeyChanged")]
    [DisableInPlayMode]
    [SerializeField]
    [HideLabel]
    [HorizontalGroup(GroupID = "install")]
    public ItemDefinition ItemDefinition;
    
    public void OnBeforeSerialize()
    {
        
    }
    public void SetSelected(int index)
    {
        
    }
    public void OnAfterDeserialize()
    {
        
    }
    
    private void OnEventKeyChanged()
    {
        if (ItemDefinition == null) return;
    }
    
#if UNITY_EDITOR


    private List<ValueDropdownItem<ItemDefinition>> GetAllAppropriateKeys()
    {
        var allKeys = Resources.FindObjectsOfTypeAll<ItemDefinition>();
        var dropdownItems = new List<ValueDropdownItem<ItemDefinition>>();
        foreach (var key in allKeys)
        {
            dropdownItems.Add(new ValueDropdownItem<ItemDefinition>(key.name, key));
        }

        return dropdownItems;
    }
#endif
}
