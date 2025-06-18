using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DS_MovingActor : Data
{
    [SerializeField][Tooltip("Mostly for 3d")] private Vector3 _lookDirection;
    public Vector3 LookDirection
    {
        get => _lookDirection;
        set => _lookDirection = value;
    }
    
    
    [SerializeField] private Vector3 _moveDirection;
    public Vector3 MoveDirection
    {
        get => _moveDirection;
        set => _moveDirection = value;
    }
    
     
    [SerializeField] private Vector2 _moveInput;
    public Vector2 MoveInput
    {
        get => _moveInput;
        set => _moveInput = value;
    }
    
    public bool BlockMoveInput { get; set; }
}
