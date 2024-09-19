using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum ActorStateFlags
{
    Nothing = 0,
    Started = 1 << 0,
    Stopped = 1 << 1
}

public enum ActorStartMethods
{
    Auto,
    Manual,
    OnStart,
}

public class ActorBase : MonoBehaviour, ITagContainer
{
    [ShowInInspector]
    [HorizontalGroup("Status")]
    [DisplayAsString(FontSize = 14)]
    [PropertyOrder(-1000)]
    [HideLabel]
    [GUIColor("GetColorForProperty")]
    public virtual ActorStateFlags Flags
    {
        get
        {
            ActorStateFlags flags = ActorStateFlags.Nothing;
            if (_started)
            {
                flags |= ActorStateFlags.Started;
            }

            if (_stopped)
            {
                flags |= ActorStateFlags.Stopped;
            }

            return flags;
        }
    }

    [ReadOnly] public string ActorID;
    public ActorStartMethods StartMethod;
    [SerializeField] private List<GenericKey> _initialTags = new List<GenericKey>();
    private HashSet<string> _tags = new HashSet<string>();
    
    private bool _started;
    private bool _stopped;
    private bool _startedOnce;
    public bool StartedOnce => _startedOnce;
    public bool IsRunning => _started && !_stopped;

    [ShowInInspector] [ReadOnly] [HideInEditorMode]
    protected Dictionary<string, Data> _datasets = new Dictionary<string, Data>();
    
    protected Dictionary<Type,object> _services = new Dictionary<Type, object>();

 

    [ReadOnly]
    [ShowInInspector]
    [HideInEditorMode]
    private HashSet<string> _outputTags => _tags;

    public event Action onActorStopped;
    public event Action onActorStarted;
    public event Action<ITagContainer, string> onTagAdded;
    public event Action<ITagContainer, string> onTagRemoved;
    public event Action<ITagContainer, string> onTagsChanged;

    protected ActorBase ParentActor;

    private SocketRegistry _socketRegistry;

    public Transform GetSocket(string socketName)
    {
        if (_socketRegistry == null) return null;
        Transform socketTransform = _socketRegistry.GetSocket(socketName);
        return socketTransform;
    }

    public GameObject GetEquippedInstance()
    {
        DS_EquipmentUser equipmentUser = GetData<DS_EquipmentUser>();
        return equipmentUser.EquipmentInstance;
    }

    protected virtual void OnActorStart()
    {
        onActorStarted?.Invoke();
        _started = true;
        _startedOnce = true;
        _stopped = false;
    }

    protected virtual void OnActorStop()
    {
        _started = false;
        _stopped = true;
    }

    public void StartIfNot(ActorBase parentActor = null)
    {
        if (parentActor != null)
        {
            parentActor.onActorStopped += OnParentActorStopped;
            ParentActor = parentActor;
        }

        if (!_started)
        {
            OnActorStart();
        }
    }

    public void StopIfNot()
    {
        if (_started)
        {
            OnActorStop();
            _stopped = true;
            onActorStopped?.Invoke();
        }
    }

    private void OnParentActorStopped()
    {
        StopIfNot();
        ParentActor.onActorStopped -= OnParentActorStopped;
    }


    protected virtual void Awake()
    {
        ActorID = ActorIDManager.GetUniqueID(gameObject.name);
        name = ActorID;

        if (Utils.TryGetComponentInChildren(gameObject, out SocketRegistry registry))
        {
            _socketRegistry = registry;
        }

        InstallDataSelf();
        CreateTagSet();
    }

    protected void Start()
    {
        if (StartMethod == ActorStartMethods.OnStart)
        {
            StartIfNot();
        }
    }

    private void InstallDataSelf()
    {
        DataList[] dataListComponents = GetComponentsInChildren<DataList>();
        foreach (var dataComponent in dataListComponents)
        {
            foreach (var data in dataComponent.Datas)
            {
                if (data.IsGlobal) continue;
                InstallData(data);
            }
        }
    }

    private void CreateTagSet()
    {
        for (int i = 0; i < _initialTags.Count; i++)
        {
            _tags.Add(_initialTags[i].ID);
        }
    }

    public T GetData<T>(string key = "", bool checkGlobal = false) where T : Data
    {
        if (key != "")
        {
            key = ":" + key;
        }

        if (_datasets.ContainsKey(typeof(T) + key))
        {
            _datasets[typeof(T) + key].OnFirstTimeGet();
            return (T) _datasets[typeof(T) + key];
        }
        else
        {
            if (GlobalData.TryGetData<T>(key, out T data))
            {
                return data;
            }

            Debug.LogWarning($"Dataset of type {typeof(T).Name} not found");
            return null;
        }
    }

    public void InstallData(object[] dataSet)
    {
        foreach (var data in dataSet)
        {
            InstallData(data as Data);
        }
    }

    private void InstallData(Data data)
    {
        if (data.UseKey)
        {
            string key = data.GetType() + ":" + data.DataKey;
            _datasets[key] = data;
        }
        else
        {
            _datasets[data.GetType().ToString()] = data;
        }

        data.OwnerActor = this;
        data.IsInstalled = true;
        data.OnInstalled();
    }

    public bool TryGetData<T>(string key, out T data) where T : Data
    {
        if (_datasets.ContainsKey(key + typeof(T).ToString()))
        {
            data = (T) _datasets[key + typeof(T).ToString()];
            return true;
        }
        else
        {
            data = null;
            return false;
        }
    }

    public T GetGlobalData<T>() where T : Data
    {
        //return GlobalData.GetData<T>();
        return null;
    }
    
    public object GetService<T> ()
    {
        return _services[typeof(T)];
    }
    
    public bool TryGetService<T>(out T service)
    {
        if (_services.ContainsKey(typeof(T)))
        {
            service = (T) _services[typeof(T)];
            return true;
        }
        else
        {
            service = default;
            return false;
        }
    }
    
    public void AddService<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public bool ContainsTag(string t)
    {
        return _tags.Contains(t);
    }

    public void AddTag(string t)
    {
        _tags.Add(t);
        onTagAdded?.Invoke(this, t);
        onTagsChanged?.Invoke(this, t);
    }

    public void RemoveTag(string t)
    {
        _tags.Remove(t);
        onTagRemoved?.Invoke(this, t);
        onTagsChanged?.Invoke(this, t);
    }

    public void ModifyFlagTag(string t, bool flagValue)
    {
        if (flagValue)
        {
            if (!ContainsTag(t))
            {
                AddTag(t);
            }
        }
        else
        {
            if (ContainsTag(t))
            {
                RemoveTag(t);
            }
        }
    }

    [Button]
    [HideIf("_started")]
    [HideInEditorMode]
    private void TestStartActor()
    {
        StartIfNot();
    }

    [Button]
    [HideIf("_stopped")]
    [HideInEditorMode]
    private void TestStopActor()
    {
        StopIfNot();
    }

    private Color GetColorForProperty()
    {
        return _started ? new Color(0.35f, .83f, .29f, 255) : new Color(1f, .29f, .29f, 255);
    }
}