using System;
using System.Collections.Generic;
using GenericUnityObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
[CreateGenericAssetMenu] 
public class GameEvent<T> : GenericScriptableObject // Restrict T to be a reference type
{
    // Define a UnityEvent that takes a parameter of type T
    private readonly UnityEvent<T> onEventRaisedWithParam = new UnityEvent<T>();
    private readonly UnityEvent onEventRaisedWithoutParam = new UnityEvent();

#if UNITY_EDITOR
    [ShowInInspector] private List<object> _listeners = new List<object>();
#endif

    public void Register(UnityAction<T> listener)
    {
        onEventRaisedWithParam.AddListener(listener);

#if UNITY_EDITOR
        _listeners.Add(listener.Target);
#endif
    }

    public void Register(UnityAction listener)
    {
        onEventRaisedWithoutParam.AddListener(listener);

#if UNITY_EDITOR
        _listeners.Add(listener.Target);
#endif
    }

    public void Unregister(UnityAction<T> listener)
    {
        onEventRaisedWithParam.RemoveListener(listener);
        
#if UNITY_EDITOR
        _listeners.Remove(listener.Target);
#endif
    }

    public void Unregister(UnityAction listener)
    {
        onEventRaisedWithoutParam.RemoveListener(listener);

#if UNITY_EDITOR
        _listeners.Remove(listener.Target);
#endif
    }

    [Button]
    public void Raise(T parameter)
    {
        onEventRaisedWithParam.Invoke(parameter);
    }

    [Button]
    public void Raise()
    {
        onEventRaisedWithoutParam.Invoke();
    }
}