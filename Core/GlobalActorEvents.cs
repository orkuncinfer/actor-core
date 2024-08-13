using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalActorEvents
{
    public static event Action onActorsInitialized;
    public static event Action onGameModeStopped;
    public static bool ActorsInitialized { get; set; }

    public static void SetActorsInitialized()
    {
        if (ActorsInitialized) return;
        ActorsInitialized = true;
        onActorsInitialized?.Invoke();
    }
    
    public static void SetGameModeStopped()
    {
        onGameModeStopped?.Invoke();
    }
}