using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ActorMonoService<T> : MonoBehaviour where T : class
{
    [SerializeField][ReadOnly] private ActorBase _actor;

    public ActorBase Actor
    {
        get => _actor;
        private set => _actor = value;
    }
    protected virtual bool BeginOnActorInitialize => false;
    
    [ShowInInspector, ReadOnly, HideInEditorMode]
    public bool IsRegistered { get; set; }

    private void Awake()
    {
        Actor = ActorUtilities.FindFirstActorInParents(transform);
        
        Initialize(Actor);
    }
    private void Initialize(ActorBase actor)
    {
        if (actor != null)
        {
            RegisterService();
            Actor = actor;
            Actor.onActorStarted += OnActorBegin;
            Actor.onActorStopped += OnActorStop;
            
            if (Actor.IsRunning)
            {
                OnActorBegin();
            }
        }
        OnAfterInitialize();
    }
    public virtual void RegisterService()
    {
        if (IsRegistered)
        {
            return;
        }
        Actor.AddService(this as T);
        IsRegistered = true;
    }
    
    protected virtual void OnAfterInitialize()
    {
    }
    
    protected virtual void OnActorBegin()
    {
        OnServiceBegin();
    }
    
    protected virtual void OnActorStop()
    {
        OnServiceStop();
    }
    
    public virtual void OnServiceBegin(){}

    public virtual void OnServiceStop(){}
}
                        