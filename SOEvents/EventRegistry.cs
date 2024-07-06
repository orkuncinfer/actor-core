using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventRegistry
{
    private static readonly Dictionary<string, Action<EventArgs>> _globalEventDictionary =
        new Dictionary<string, Action<EventArgs>>();

    public static void Install(string key)
    {
        if (!_globalEventDictionary.ContainsKey(key))
        {
            _globalEventDictionary.Add(key,null);
        }
    }

    public static void Remove(string key)
    {
        if (_globalEventDictionary.ContainsKey(key))
        {
            _globalEventDictionary.Remove(key);
        }
    }

    public static void Register(string key,Action<EventArgs> action)
    {
        if (!ContainsEvent(key)) Install(key);
        _globalEventDictionary[key] += action;
    }

    public static void  Unregister(string key,Action<EventArgs> action)
    {
        if (!ContainsEvent(key)) return;
        _globalEventDictionary[key] -= action;
    }

    public static void Raise(string key)
    {
        if (!ContainsEvent(key)) return;
        _globalEventDictionary[key]?.Invoke(new EventArgs(){Sender = null,EventName = key});
    }

  
    
    public static bool ContainsEvent(string key)
    {
        if (!_globalEventDictionary.ContainsKey(key)) return false;
        return true;
    }
}

public static class EventRegistry<TArg1>
{
    private static readonly Dictionary<string, Action<EventArgs, TArg1>> _globalEventDictionary =
        new Dictionary<string, Action<EventArgs,TArg1>>();

    public static void Install(string key)
    {
        if (!_globalEventDictionary.ContainsKey(key))
        {
            _globalEventDictionary.Add(key,null);
        }
    }

    public static void Remove(string key)
    {
        if (_globalEventDictionary.ContainsKey(key))
        {
            _globalEventDictionary.Remove(key);
        }
    }

    public static void Register(string key,Action<EventArgs,TArg1> action)
    {
        if (!ContainsEvent(key)) Install(key);
        _globalEventDictionary[key] += action;
    }

    public static void  Unregister(string key,Action<EventArgs,TArg1> action)
    {
        if (!ContainsEvent(key)) return;
        _globalEventDictionary[key] -= action;
    }

    public static void Raise(string key,TArg1 arg1)
    {
        if (!ContainsEvent(key)) return;
        _globalEventDictionary[key]?.Invoke(new EventArgs(){Sender = null,EventName = key},arg1);
    }

  
    
    public static bool ContainsEvent(string key)
    {
        if (!_globalEventDictionary.ContainsKey(key)) return false;
        return true;
    }
}

