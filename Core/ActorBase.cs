using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
public enum ActorStateFlags
{
    Nothing =0,
    Started =1<<0,
    Stopped=1<<1
}
public class ActorBase : MonoBehaviour, ITagContainer
{
    [ShowInInspector][HorizontalGroup("Status")][DisplayAsString(FontSize = 14)][PropertyOrder(-1000)][HideLabel][GUIColor("GetColorForProperty")]
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
    
    [ReadOnly]public string ActorID;
    public bool BeginOnStart;
    private bool _started;
    private bool _stopped;
    
    protected Dictionary<Type, Data> _datasets = new Dictionary<Type, Data>();
    
    [SerializeField] private List<GenericKey> _initialTags = new List<GenericKey>();
    private HashSet<string> _tags = new HashSet<string>();
    [ReadOnly][ShowInInspector][HideInEditorMode]private HashSet<string> _outputTags => _tags;
  
    public event Action onActorStopped;
    public event Action<ITagContainer, string> onTagAdded;
    public event Action<ITagContainer, string> onTagRemoved;
    public event Action<ITagContainer, string> onTagsChanged;

    protected Actor ParentActor;
    
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

    protected virtual void OnActorStart() { _started = true; _stopped = false; }
    protected virtual void OnActorStop() { _started = false; _stopped = true; }

    public void StartIfNot(Actor parentActor = null)
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
        
        LoadData();
        CreateTagSet();
    }
    protected void Start()
    {
        if (BeginOnStart)
        {
            OnActorStart();
        }
    }
    private void LoadData()
    {
        Data[] dataComponents = GetComponentsInChildren<Data>();
        foreach (var dataComponent in dataComponents)
        {
            _datasets[dataComponent.GetType()] = dataComponent;
            dataComponent.OwnerActor = this;
        }
    }

    private void CreateTagSet()
    {
        for (int i = 0; i < _initialTags.Count; i++)
        {
            _tags.Add(_initialTags[i].ID);
        }
    }
    public T GetData<T>() where T : Data
    {
        if (_datasets.ContainsKey(typeof(T)))
        {
            return (T)_datasets[typeof(T)];
        }
        else
        {
            Debug.LogWarning($"Dataset of type {typeof(T).Name} not found");
            return null;
        }
    }
    public T GetGlobalData<T>() where T : Data
    {
            //return GlobalData.GetData<T>();
            return null;
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


    [Button][HideIf("_started")]
    private void TestStartActor()
    {
        StartIfNot();
    }
    [Button][HideIf("_stopped")]
    private void TestStopActor()
    {
        StopIfNot();
    }
    
    private Color GetColorForProperty()
    {
        return _started ? new Color(0.35f,.83f,.29f,255) : new Color(1f,.29f,.29f,255);
    }
}
