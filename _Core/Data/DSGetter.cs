using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

using UnityEngine;
[System.Serializable]
[BoxGroup("DataGetter")]

public class DSGetter<T> where T : Data
{
    [StringInput][Tag]//[ValueDropdown("GetAllGenericKeys")]
    public string Key;

    public GenericKey GenericKey;

    public GetterType From;

    [HideInEditorMode]
    public T Data;

    [SerializeField][SerializeReference][HideInEditorMode]
    private Data _retrievedData;

    public void GetData(ActorBase owner = null)
    {
        string key = "";
        if (Key != null)
        {
            key = Key;
        }
        
        if (From == GetterType.Owner)
        {
            Data = owner.GetData<T>(key);
        }
        else
        {
            Data = GlobalData.GetData<T>(key);
        }
        _retrievedData = Data;
    }

#if UNITY_EDITOR
    private List<ValueDropdownItem<GenericKey>> GetAllGenericKeys() {
        var allKeys = Resources.FindObjectsOfTypeAll<GenericKey>();
        var dropdownItems = new List<ValueDropdownItem<GenericKey>>();
        foreach (var key in allKeys) {
            if (key.name.StartsWith("DK_"))
            {
                dropdownItems.Add(new ValueDropdownItem<GenericKey>(key.name, key));
            }
        }
        return dropdownItems;
    }
#endif
    
}

public enum GetterType
{
    Global,
    Owner
}

public class StringInputAttribute : Attribute {}
