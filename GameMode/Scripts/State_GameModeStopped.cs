using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class State_GameModeStopped : MonoState
{
    protected override void OnEnter()
    {
        base.OnEnter();
        GlobalActorEvents.SetGameModeStopped();
    }
}