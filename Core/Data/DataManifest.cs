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
                if(!data.IsPersistent) continue;
                string key = data.GetType().ToString();
                if (data.UseKey)
                {
                    key += ":" + data.DataKey;
                }
                Debug.Log("Saving Data : " + key);
                ES3Wrapper.Save(key,data,transform);
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
               key = data.GetType().ToString();
               if (data.UseKey)
               {
                   key += ":" + data.DataKey;
               }

               if (ES3.KeyExists(key))
               {
                   ES3Wrapper.LoadInto(key,data,transform);
               }
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
