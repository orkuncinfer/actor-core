using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventRegistry
{
    private static readonly Dictionary<ActorBase, Dictionary<string,Action<EventArgs>>> _eventDictionary 
        = new Dictionary<ActorBase, Dictionary<string,Action<EventArgs>>>();
    
    private static readonly Dictionary<string, Action<EventArgs>> _globalEventDictionary =
        new Dictionary<string, Action<EventArgs>>();
    
    private static readonly Dictionary<string, List<Action<EventArgs>>> _autoRemoveDictionary =
        new Dictionary<string,  List<Action<EventArgs>>>();

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

    public static void Register(string key,Action<EventArgs> action, bool autoUnregister = false)
    {
        if (!ContainsEvent(key)) Install(key);
        _globalEventDictionary[key] += action;

        if (autoUnregister)
        {
            _autoRemoveDictionary[key].Add(action);
        }
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
        
        if (_autoRemoveDictionary.ContainsKey(key))
        {
            foreach (var action in _autoRemoveDictionary[key])
            {
                _globalEventDictionary[key] -= action;
                Debug.Log("Unregistering action: " + action.Method.Name);
            }
            _autoRemoveDictionary.Remove(key);
        }
    }
    
    public static bool ContainsEvent(string key)
    {
        if (!_globalEventDictionary.ContainsKey(key)) return false;
        return true;
    }

    #region ContextEvent
    public static Action<EventArgs> Register(ActorBase main, string key,Action<EventArgs> action)
    {
        if(!ContainsEvent(main,key)) Install(main,key);
        _eventDictionary[main][key] += action;
        return _eventDictionary[main][key];
    }

    public static Action<EventArgs> Unregister(ActorBase main, string key,Action<EventArgs> action)
    {
        if(!ContainsEvent(main,key)) return null;
        _eventDictionary[main][key] -= action;
        return _eventDictionary[main][key];
    }

    public static void Raise(ActorBase main, string key)
    {
        if (!ContainsEvent(main,key)) return;
        _eventDictionary[main][key]?.Invoke(new EventArgs(){Sender = main,EventName = key});
    }
    
    public static bool ContainsEvent(ActorBase main, string key)
    {
        if (!_eventDictionary.ContainsKey(main)) return false;
        if (!_eventDictionary[main].ContainsKey(key)) return false;
        return true;
    }
    
    public static void Install(ActorBase main, string key)
    {
        if (_eventDictionary.ContainsKey(main))
        {
            if (_eventDictionary[main].ContainsKey(key)) return;
        
            _eventDictionary[main].Add(key,null);
        }
        else
        {
            _eventDictionary.Add(main,new Dictionary<string, Action<EventArgs>>(){{key,null}});
            main.requestRemoveEvents += OnRequestRemoveData;
        }
    }
    private static void OnRequestRemoveData(ActorBase obj)
    {
        _eventDictionary[obj] = null;
        _eventDictionary.Remove(obj);
        obj.requestRemoveEvents -= OnRequestRemoveData;
    }
    
    #endregion
}

public static class EventRegistry<TArg1>
{
    private static readonly Dictionary<ActorBase, Dictionary<string,Action<EventArgs,TArg1>>> _eventDictionary 
        = new Dictionary<ActorBase, Dictionary<string,Action<EventArgs, TArg1>>>();
    
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
    
    #region ContextEvent
    public static Action<EventArgs,TArg1> Register(ActorBase main, string key,Action<EventArgs,TArg1> action)
    {
        if(!ContainsEvent(main,key)) Install(main,key);
        _eventDictionary[main][key] += action;
        return _eventDictionary[main][key];
    }

    public static Action<EventArgs,TArg1> Unregister(ActorBase main, string key,Action<EventArgs,TArg1> action)
    {
        if(!ContainsEvent(main,key)) return null;
        _eventDictionary[main][key] -= action;
        return _eventDictionary[main][key];
    }

    public static void Raise(ActorBase main, string key, TArg1 arg1)
    {
        if (!ContainsEvent(main,key)) return;
        _eventDictionary[main][key]?.Invoke(new EventArgs(){Sender = main,EventName = key},arg1);
    }
    
    public static bool ContainsEvent(ActorBase main, string key)
    {
        if (!_eventDictionary.ContainsKey(main)) return false;
        if (!_eventDictionary[main].ContainsKey(key)) return false;
        return true;
    }
    
    public static void Install(ActorBase main, string key)
    {
        if (_eventDictionary.ContainsKey(main))
        {
            if (_eventDictionary[main].ContainsKey(key)) return;
        
            _eventDictionary[main].Add(key,null);
        }
        else
        {
            _eventDictionary.Add(main,new Dictionary<string, Action<EventArgs,TArg1>>(){{key,null}});
            main.requestRemoveEvents += OnRequestRemoveData;
        }
    }
    private static void OnRequestRemoveData(ActorBase obj)
    {
        _eventDictionary[obj] = null;
        _eventDictionary.Remove(obj);
        obj.requestRemoveEvents -= OnRequestRemoveData;
    }
    
    #endregion
}

