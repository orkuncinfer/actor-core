using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class DataManifest : MonoBehaviour
{
    [ReadOnly] public Actor Actor;
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
            if (_installData[i].IsPersistent)
            {
                // Critical change: Merge data instead of replacing reference
                var installedData = _installData[i].LoadData() as Data;
                if(installedData == null) continue;
                _installData[i].MergePersistentData(installedData);
            }
        }
        foreach (var data in InstallData())
        {
            string key = "";
            if (data.IsPersistent)
            {
                //data.LoadData();
            }

            key = "";
            if (data.IsGlobal)
            {
                if (data.UseKey)
                {
                    key = data.DataKey;
                }

                GlobalData.LoadData(key, data);
            }
            else
            {
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

                GlobalData.LoadData(key, data);
            }
            else
            {
                Actor.InstallData(InstallData());
            }
        }
    }

    public static Actor FindFirstActorInParents(Transform currentParent)
    {
        if (currentParent == null)
        {
            return null;
        }

        Actor actor = currentParent.GetComponent<Actor>();

        if (actor != null)
        {
            return actor;
        }

        return FindFirstActorInParents(currentParent.parent);
    }
}