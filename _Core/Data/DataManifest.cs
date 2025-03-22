using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class DataManifest : MonoBehaviour
{
    [ReadOnly] public ActorBase Actor;
    private Data[] _installData;

    protected abstract Data[] InstallData();

    private void OnDestroy()
    {
        foreach (var data in InstallData())
        {
            if(!data.IsPersistent) continue;
            data.SaveData();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SaveData();
    }

    private void Awake()
    {
        Actor = FindFirstActorInParents(transform);

        _installData = InstallData();

        for (int i = 0; i < _installData.Length; i++)
        {
            
            #region LOAD
            Data data = _installData[i];
            string key = "";
            if (data.IsPersistent)
            {
                data.LoadData();
            }
            #endregion
            
            key = "";
            if (data.IsGlobal)
            {
                if (data.UseKey)
                {
                    key = data.DataKey;
                }

                GlobalData.InstallData(key, data);
            }
            else
            {
                if (Actor == null)
                {
                    Debug.Log("Actor is null on " + transform + "not global " + data.GetType());
                    return;
                }
                Actor.InstallData(InstallData());
            }
        }
    }
    
    public void SaveData()
    {
        foreach (var data in InstallData())
        {
            if(!data.IsPersistent) continue;
            data.SaveData();
        }
    }

    public void LoadData()
    {
        foreach (var data in InstallData())
        {
            string key = "";
            if (data.IsPersistent)
            {
                data.LoadData();
            }

            key = "";
            if (data.IsGlobal)
            {
                if (data.UseKey)
                {
                    key = data.DataKey;
                }

                GlobalData.InstallData(key, data);
            }
            else
            {
                Actor.InstallData(InstallData());
            }
        }
    }

    public static ActorBase FindFirstActorInParents(Transform currentParent)
    {
        if (currentParent == null)
        {
            return null;
        }

        ActorBase actor = currentParent.GetComponent<ActorBase>();

        if (actor != null)
        {
            return actor;
        }

        return FindFirstActorInParents(currentParent.parent);
    }
}