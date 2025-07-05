using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Data_Combatable : Data
{

    [SerializeField][Tooltip("Vector3.zero means no target")] private Vector3 _aimPosition;
    public Vector3 AimPosition
    {
        get => _aimPosition;
        set => _aimPosition = value;
    }
    
    private Transform _aimTransform;
    public Transform AimTransform
    {
        get => _aimTransform;
        set => _aimTransform = value;
    }
    
    [SerializeField][Tooltip("You'd want to enable this if it is player")] private bool _shootRayFromCamera;
    public bool ShootRayFromCamera
    {
        get => _shootRayFromCamera;
        set => _shootRayFromCamera = value;
    }
}
