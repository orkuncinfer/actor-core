using System;
using System.Collections.Generic;

public static class EventBus
{
    // Dictionary to hold events with their subscribers
    private static readonly Dictionary<Type, Delegate> events = new Dictionary<Type, Delegate>();

    // Subscribe to an event
    public static void Subscribe<T>(Action<T> callback)
    {
        if (events.TryGetValue(typeof(T), out var existingEvent))
        {
            events[typeof(T)] = Delegate.Combine(existingEvent, callback);
        }
        else
        {
            events[typeof(T)] = callback;
        }
    }

    // Unsubscribe from an event
    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (events.TryGetValue(typeof(T), out var existingEvent))
        {
            var newEvent = Delegate.Remove(existingEvent, callback);
            if (newEvent == null)
            {
                events.Remove(typeof(T));
            }
            else
            {
                events[typeof(T)] = newEvent;
            }
        }
    }

    // Publish an event
    public static void Publish<T>(T eventArg)
    {
        if (events.TryGetValue(typeof(T), out var existingEvent))
        {
            var callback = existingEvent as Action<T>;
            callback?.Invoke(eventArg);
        }
    }
}