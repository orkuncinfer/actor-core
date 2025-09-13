using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeSingletons()
    {
        SingletonSetup[] singletons = Resources.LoadAll<SingletonSetup>("SingletonSetups");
        
        foreach (var singleton in singletons)
        {
            singleton.Initialize();
        }
        
        Debug.Log("Singletons initialized");
    }
}
