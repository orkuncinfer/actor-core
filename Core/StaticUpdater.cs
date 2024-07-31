using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticUpdater
{
    public static event Action onUpdate;
    public static event Action onLateUpdate;
    public static event Action onFixedUpdate;
    
    public static void Update()
    {
        onUpdate?.Invoke();
    }
    
    public static void LateUpdate()
    {
        onLateUpdate?.Invoke();
    }
    
    public static void FixedUpdate()
    {
        onFixedUpdate?.Invoke();
    }
}
