using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_CinemachineFollowVarAsset : MonoState
{
    [SerializeField] private GameObjectVariable _playerFollower;
    protected override void OnEnter()
    {
        base.OnEnter();
        if(_playerFollower.Value == null)return;
        Owner.GetData<Data_Camera>().CurrentCamera.Follow = _playerFollower.Value.transform;
    }
}
