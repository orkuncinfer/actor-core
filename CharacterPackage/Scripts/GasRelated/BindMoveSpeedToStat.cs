using System;
using System.Collections;
using System.Collections.Generic;
using ECM2;
using StatSystem;
using UnityEngine;

public class BindMoveSpeedToStat : MonoBehaviour
{
    private Service_GAS _gas;
    private StatController _statController;
    private Character _character;
    
    private void Start()
    {
        _gas = GetComponentInParent<Service_GAS>();
        _statController = _gas.StatController;
        _gas.onServiceBegin += OnGasBegin;
    }

    private void OnGasBegin(ActorBase owner)
    {
        _character = owner.GetData<Data_Character>().Character;
        
        if (_statController.IsInitialized)
            OnStatControllerInitialized();
        else
            _statController.onInitialized += OnStatControllerInitialized;
    }

    private void OnStatControllerInitialized()
    {
        _character.maxWalkSpeed = _statController.Stats["MoveSpeed"].Value;
        _statController.onStatIsModified += OnStatIsModified;
    }

    private void OnStatIsModified(Stat stat)
    {
        //Debug.Log("Stat is modified : " + stat.Definition.name + " : " + stat.Value);
        if (stat.Definition.name == "MoveSpeed")
        {
            _character.maxWalkSpeed = stat.Value;
        }
    }
}