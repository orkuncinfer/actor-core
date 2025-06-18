using System.Collections;
using System.Collections.Generic;
using ECM2;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class State_ApplyJoystickInput : MonoState
{
    
    //[SerializeField] private DataGetter<DS_Joystick> _joystickData;
    private CharacterInput _character;
    protected override void OnEnter()
    {
        base.OnEnter();
        _character = Owner.GetComponent<CharacterInput>();
    }
    
}
