using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Data : MonoBehaviour, IData
{
    [ShowInInspector]
    [HorizontalGroup("Status")]
    [DisplayAsString(FontSize = 14)]
    [PropertyOrder(-1000)]
    [HideLabel]
    [ShowIf("_isInstalled")]
    [GUIColor("GetColorForProperty")]
    public string Info
    {
        get
        {
            
            if (OwnerActor)
            {
                return "Installed to : "  + OwnerActor.name;
            }
            if (OwnerActor == null)
            {
                return "";
            }

            return Info;
        }
    }
    private void OnEnable()
    {
       
        if ( IsGlobal)
        {
            string key = DataKey.ID + DataType.GetType();
            GlobalData.LoadData(key,DataType);
        }
    }
    
    [BoxGroup("InstallMethod")] public bool IsGlobal;
    
    [ShowIf("IsGlobal")][BoxGroup("InstallMethod")][ValueDropdown("GetAllGenericKeys")]
    public GenericKey DataKey;

    public Data DataType;

    [HideInInspector]public ActorBase OwnerActor;

    private bool _isInstalled => OwnerActor;
    
    
    public Actor Actor => OwnerActor as Actor;
    private Dictionary<string, object> values = new Dictionary<string, object>();

    public virtual T GetValue<T>(string name)
    {
        if (values.TryGetValue(name, out object value))
        {
            return (T)value;
        }
        return default(T);
    }

    public virtual void SetValue<T>(string name, T value)
    {
        values[name] = value;
    }
    private Color GetColorForProperty()
    {
        return new Color(0.35f,.83f,.29f,255);
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

public interface IData
{
    T GetValue<T>(string name);
    void SetValue<T>(string name, T value);
}





