using System;
using UnityEngine;

[Serializable]
public class Condition_CanUseAbility : StateCondition
{
    [SerializeField] private AbilityDefinition _abilityDefinition;

    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
    }

    public override bool CheckCondition()
    {
        Service_GAS gas = Owner.GetService<Service_GAS>();
        AbilityController abilityController = gas.AbilityController;

        bool canUse = abilityController.CanActivateAbility(_abilityDefinition);
        
        abilityController.AddAndTryActivateAbility(_abilityDefinition);
        
        return canUse;
    }
}