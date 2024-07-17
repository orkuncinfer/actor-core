using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class Data :  IData
{
    [ShowInInspector]
    [HorizontalGroup("Status")]
    [DisplayAsString(FontSize = 14)]
    [PropertyOrder(-1000)]
    [HideLabel]
    [ShowIf("IsInstalled")]
    [GUIColor("GetColorForProperty")]
    private string Info
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
       // FIX ME : HANDLE REMOVE DATA ON DISABLE
    }
    [BoxGroup("Grp1",false)][GUIColor(0.35f,.83f,.29f)]
    [HorizontalGroup("Grp1/1")][ES3NonSerializable]public bool IsGlobal;
    [BoxGroup("Grp1",false)][GUIColor(0.35f,.83f,.29f)]
    [HorizontalGroup("Grp1/1")][ES3NonSerializable]public bool UseKey;
    [BoxGroup("Grp1",false)][GUIColor(0.35f,.83f,.29f)]
    [HorizontalGroup("Grp1/1")][ES3NonSerializable]public bool IsPersistent;
  
    [BoxGroup("Grp1",false)][GUIColor(0.35f,.83f,.29f)]
    [HorizontalGroup("Grp1/1",marginRight:20)][ES3NonSerializable][HideLabel]
    [ShowIf("UseKey")][Tag]
    public string DataKey;
    
    [HideInInspector][ES3NonSerializable]public ActorBase OwnerActor;

    [HideInInspector]public bool IsInstalled;

    private bool _retrievedOnce;
    
    private Dictionary<string, object> values = new Dictionary<string, object>();

    public virtual T GetValue<T>(string name)
    {
        if(!_retrievedOnce) OnFirstTimeGet();
        if (values.TryGetValue(name, out object value))
        {
            return (T)value;
        }
        _retrievedOnce = true;
        return default(T);
    }

    public virtual void OnFirstTimeGet()
    {
        
    }

    public virtual void OnInstalled()
    {
        if(OwnerActor)OwnerActor.onActorStopped += OnActorStopped;
        if(OwnerActor)OwnerActor.onActorStarted += OnActorStarted;
    }

    public virtual void OnActorStarted()
    {
    }

    public virtual void OnActorStopped()
    {
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

    public ActorBase Actor => OwnerActor;
    public string DefaultCategory { get; set; }
    public Transform Transform { get; set; }
    
    public string GetLoadKey()
    {
        string containerID = "Global"; 
        string typeName = GetType().Name;
        string saveFileName = "Default";
        return containerID + "-" + typeName + DataKey;
    }
    public void LoadData(string category = "Default")
    {
        string containerID = "Global"; 
        string typeName = GetType().Name;
        string saveFileName = category;
        Debug.Log("Loading Data, ID: "+containerID+"-"+typeName + DataKey + " | File name: " + saveFileName+".save");
        if (ES3.KeyExists(GetLoadKey()))
        {
            ES3.LoadInto(GetLoadKey(),this);
        }
    }

    public void SaveData(string category = "Default")
    {
        string containerID = "Global"; 
        string typeName = GetType().Name;
        string saveFileName = category;
        ES3.Save(GetLoadKey(),this);
        Debug.Log("Saving Data, ID: "+containerID+"-"+typeName + DataKey+ " | File name: " + saveFileName+".save");
    }
}

public interface IData
{
    public ActorBase Actor { get; }
    public string DefaultCategory { get; set; }
    public Transform Transform { get; set; }
    void LoadData(string category = null);
    void SaveData(string category = null);
}
