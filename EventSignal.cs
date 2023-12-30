using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewEventSignal", menuName = "Events/Event Signal")]
public class EventSignal : ScriptableObject
{
    private readonly UnityEvent onEventRaised = new UnityEvent();
    
#if UNITY_EDITOR
    [ShowInInspector]private List<object> _listeners = new List<object>();
#endif
    
    public void Register(UnityAction listener)
    {
        onEventRaised.AddListener(listener);

#if UNITY_EDITOR
        _listeners.Add(listener.Target);
#endif
    }
    public void Unregister(UnityAction listener)
    {
        onEventRaised.RemoveListener(listener);
        
#if UNITY_EDITOR
        _listeners.Remove(listener.Target);
#endif
    }
    [Button]
    public void Raise()
    {
        onEventRaised.Invoke();
    }
}