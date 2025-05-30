using System;
using System.Collections;
using UnityEngine;

public static class GlobalActorEvents
{
    public static event Action onActorsInitialized;
    public static event Action onGameModeStopped;
    public static event Action onGameModeStarted;
    public static bool ActorsInitialized { get; set; }
    
    public static bool GameModeStarted { get; set; }

    public static void SetActorsInitialized()
    {
        if (ActorsInitialized) return;
        ActorsInitialized = true;
        onActorsInitialized?.Invoke();
    }
    
    public static void SetGameModeStopped()
    {
        onGameModeStopped?.Invoke();
        GameModeStarted = false;
    }

    public static void SetGameModeStarted()
    {
        onGameModeStarted?.Invoke();
        GameModeStarted = true;
    }
}