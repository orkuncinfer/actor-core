using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedOnScreenControls;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class BindStickToInputAction : MonoBehaviour
{
    public InputActionReference BindAction;
    public EnhancedOnScreenStick Stick;

    private void Awake()
    {
    //    Stick.onPointerDown += OnPointerDown;
       // Stick.onPointerUp += OnPointerUp;
        
        BindAction.action.Enable();
        
        BindAction.action.performed += OnMovementPerformed;
    }

    private void OnMovementPerformed(InputAction.CallbackContext obj)
    {
        float value = obj.ReadValue<float>();
        Debug.Log("value is " + value);
    }
    [Button]
    private void OnPointerUp()
    {
        if (BindAction?.action.controls[0] is ButtonControl buttonControl)
        {
            //buttonControl.WriteValueIntoState();
        }
    }


    private void OnPointerDown()
    {
        //BindAction.action.
    }
}
