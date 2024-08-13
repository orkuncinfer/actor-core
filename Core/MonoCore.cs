using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoCore : MonoBehaviour
{
    private bool _initialized;
    private void Awake()
    {
        GlobalActorEvents.onActorsInitialized += OnActorsInitialized;
        GlobalActorEvents.onGameModeStopped += OnGameStopped;
    }

    private void OnDestroy()
    {
        GlobalActorEvents.onActorsInitialized -= OnActorsInitialized;
        GlobalActorEvents.onGameModeStopped -= OnGameStopped;
    }

    private void OnActorsInitialized()
    {
        OnGameReady();
        _initialized = true;
    }

    protected virtual void OnGameReady()
    {
        
    }

    protected virtual void OnGameStopped()
    {
        
    }

    private void OnEnable()
    {
        if (_initialized)
        {
            OnGameReady();
        }
    }
}
