using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class DataGroup : MonoBehaviour
{
    private void Awake()
    {
        InstallFor(null);
    }

    private void OnDestroy()
    {
        foreach (IDataInstaller installer in GetInstallers())
        {
            installer.OnDestroy();
        }
    }
    [Button]
    public void InstallFor(ActorBase owner)
    {
        foreach (IDataInstaller installer in GetInstallers())
        {
            if (installer is DataInstaller<Data> data)
            {
                Debug.Log("3404 : Trying to install data :" + data.GetType());
            }
            Debug.Log("3404 : Trying to install data");
            installer.InstallFor(null);
        }
    }
  
    protected abstract IEnumerable<IDataInstaller> GetInstallers();
}
