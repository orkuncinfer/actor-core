using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class DataManifest : MonoBehaviour
{
    [ReadOnly]public Actor Actor;
    public bool IsPersistent;
    protected virtual Data[] InstallData()
    {
        return Array.Empty<Data>();
    }
    private void OnDestroy()
    {
        if (IsPersistent)
        {
            foreach (var data in InstallData())
            {
                data.SaveData();
            }
        }
    }

    private void Awake()
    {
        Actor = FindFirstActorInParents(transform);
       foreach (var data in InstallData())
       {
           string key = "";
           if (IsPersistent)
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
    
    private Actor FindFirstActorInParents(Transform currentParent)
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
