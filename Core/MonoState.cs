using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
[Flags]
public enum StateFlags
{
    Nothing =0,
    Running =1<<0,
    Finished=1<<1
}
public abstract class MonoState : MonoBehaviour
{
    [HideInInspector]public bool IsRunning = false;
    [HideInInspector]public bool IsFinished = false;
    
    public event Action<MonoState> onStateFinished;
    
    [ShowInInspector][HorizontalGroup("Status")][DisplayAsString(FontSize = 14)][PropertyOrder(-1000)][HideLabel][GUIColor("GetColorForProperty")]
    public virtual StateFlags Flags
    {
        get
        {
            StateFlags flags = StateFlags.Nothing;
            if (IsRunning)
            {
                flags |= StateFlags.Running;
            }
            if (IsFinished)
            {
                flags |= StateFlags.Finished;
            }

            return flags;
        }
    }
    private readonly bool isUpdateOverridden;
    protected MonoState()
    {
        var methodInfo = GetType().GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.NonPublic); // invoke update only if it is being used. Can be done for fixedupdate and lateupdate too.
        isUpdateOverridden = methodInfo.DeclaringType != typeof(MonoState);
    }
    
    protected Actor Owner;
    protected virtual void OnEnter() { IsRunning = true; }

    protected virtual void OnExit()
    {
        IsRunning = false; 
        IsFinished = true; 
        onStateFinished?.Invoke(this);
        Debug.Log("State Finished");
    }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnLateUpdate() { }
    public virtual void CheckoutEnter(ActorBase ownerActor)
    {
        if (IsRunning) return;
        Owner = ownerActor as Actor;
        if(Owner != null)Owner.onActorStopped += OnOwnerActorStopped;
        OnInitialize();
        OnEnter();
    }

    private void OnOwnerActorStopped()
    {
        IsFinished = false;
        Owner.onActorStopped -= OnOwnerActorStopped;
    }

    public virtual void Reset()
    {
        IsFinished = false;
        IsRunning = false;
    }
    public virtual void CheckoutExit()
    {
        if (!IsRunning) return;
        OnExit();
    }
    public virtual void OnInitialize(){ }

    private void Update()
    {
        if (IsRunning && isUpdateOverridden) OnUpdate();
    }
    private void FixedUpdate()
    {
        if (IsRunning) OnFixedUpdate();
    }
    private void LateUpdate()
    {
        if (IsRunning) OnLateUpdate();
    }
    
    private Color GetColorForProperty()
    {
        return IsRunning ? new Color(0.35f,.83f,.29f,255) : new Color(1f,.29f,.29f,255);
    }
}