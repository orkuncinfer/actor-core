using System;
using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using StatSystem;
using UnityEngine;

public class Service_GAS : ActorMonoService<Service_GAS>
{
    private AbilityController _abilityController;
    public AbilityController AbilityController => _abilityController;
    
    private GameplayEffectController _effectController;
    public GameplayEffectController EffectController => _effectController;
    
    private StatController _statController;
    public StatController StatController => _statController;
    
    private LevelController _levelController;
    public LevelController LevelController => _levelController;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _abilityController = GetComponent<AbilityController>();
        _effectController = GetComponent<GameplayEffectController>();
        _statController = GetComponent<StatController>();
        _levelController = GetComponent<LevelController>();
    }

 

    public override void OnServiceBegin()
    {
        base.OnServiceBegin();
        
    }
}
