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

    [HideInInspector][ES3NonSerializable]public bool IsInstalled;

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

    protected virtual void OnActorStarted()
    {
    }

    protected virtual void OnActorStopped()
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
        string containerID = OwnerActor == null ? "Global" : OwnerActor.GetInstanceID().ToString();
        string typeName = GetType().Name;
        string dataKey = string.IsNullOrEmpty(DataKey) ? "" : "-" + DataKey;
        return containerID + "-" + typeName + dataKey;
    }
    public object LoadData(string category = "DefaultData")
    {
        string saveFileName = category + ".save";
        
        if (ES3.KeyExists(GetLoadKey(),saveFileName))
        {
            DDebug.Log("Loading Data, Key: "+ GetLoadKey() + " | File name: " + saveFileName+".save-" + ES3.GetKeys(saveFileName)[0]);
            var data = ES3.Load(GetLoadKey(), saveFileName, this as object);
            ES3.LoadInto(GetLoadKey(),saveFileName,this);
            return data;
        }
        DDebug.Log("Loading Data Failed can not find data with key : "+ GetLoadKey());
        
        return null;
    }

    public void SaveData(string category = "DefaultData")
    {
        string saveFileName =  category + ".save";
        ES3.Save(GetLoadKey(),this ,saveFileName);
        DDebug.Log("Saving Data, ID: " + GetLoadKey() + " | File name: " + saveFileName);
    }
    public virtual void MergePersistentData(Data loadedData)
    {
        // Implement type-specific merging in child classes
        if (loadedData.GetType() != GetType())
        {
            Debug.LogError("Data type mismatch during merge");
            return;
        }
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(loadedData), this);
    }

}

public interface IData
{
    public ActorBase Actor { get; }
    public string DefaultCategory { get; set; }
    public Transform Transform { get; set; }
    object LoadData(string category = null);
    void SaveData(string category = null);
}
