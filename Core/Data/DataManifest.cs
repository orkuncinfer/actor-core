using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DataManifest : MonoBehaviour
{
    public Actor Actor;
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
                    key += ":" + data.DataKey.ID;
                }
                Debug.Log("Saving Data : " + key);
                ES3Wrapper.Save(key,data,transform);
            }
        }
    }

    private void Awake()
    {

       foreach (var data in InstallData())
       {
           string key = "";
           if (IsPersistent)
           {
               key = data.GetType().ToString();
               if (data.UseKey)
               {
                   key += ":" + data.DataKey.ID;
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
                   key = data.DataKey.ID;
               }
                
               GlobalData.LoadData(key, data);
           }
           else
           {
               Actor.InstallData(InstallData());
           }
       }
        
        
        
    }
}
