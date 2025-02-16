using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ActorMonoService<T> : MonoBehaviour where T : class
{
    [SerializeField][ReadOnly][LabelText("Service Owner")] private ActorBase _owner;

    public event Action<ActorBase> onServiceBegin;

    protected ActorBase Owner
    {
        get => _owner;
        private set => _owner = value;
    }
    protected virtual bool BeginOnActorInitialize => false;
    
    [ShowInInspector, ReadOnly, HideInEditorMode]
    public bool IsRegistered { get; set; }

    private void Awake()
    {
        Owner = ActorUtilities.FindFirstActorInParents(transform);
        
        Initialize(Owner);
    }
    private void Initialize(ActorBase actor)
    {
        if (actor != null)
        {
            RegisterService();
            Owner = actor;
            Owner.onActorStarted += OnOwnerBegin;
            Owner.onActorStopped += OnOwnerStop;
            
            if (Owner.IsRunning)
            {
                OnOwnerBegin();
            }
        }
        OnInitialize();
    }
    public virtual void RegisterService()
    {
        if (IsRegistered)
        {
            return;
        }
        Owner.AddService(this as T);
        IsRegistered = true;
    }
    
    protected virtual void OnInitialize()
    {
    }
    
    protected virtual void OnOwnerBegin()
    {
        OnServiceBegin();
    }
    
    protected virtual void OnOwnerStop()
    {
        OnServiceStop();
    }
    
    public virtual void OnServiceBegin(){ onServiceBegin?.Invoke(Owner); }

    public virtual void OnServiceStop(){}
}
                        