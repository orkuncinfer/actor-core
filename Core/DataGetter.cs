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
[IncludeMyAttributes]
[InlineProperty(LabelWidth = 30)]
public class DataGetter<T> where T : Data
{
    [StringInput][ValueDropdown("GetAllGenericKeys")]
    public GenericKey Key;

    [HideInEditorMode]
    public T Data;

    public void GetData()
    {
        Data = GlobalData.GetData<T>(Key.ID);
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

public class StringInputAttribute : Attribute {}
