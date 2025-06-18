using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Data_Combatable : Data
{

    [SerializeField] private Vector3 _aimDirection;
    public Vector3 AimDirection
    {
        get => _aimDirection;
        set => _aimDirection = value;
    }
    
    [SerializeField] private bool _shootRayFromCamera;
    public bool ShootRayFromCamera
    {
        get => _shootRayFromCamera;
        set => _shootRayFromCamera = value;
    }
}
