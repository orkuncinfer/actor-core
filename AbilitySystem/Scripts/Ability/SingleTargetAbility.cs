using UnityEditor;
using UnityEngine;

public class SingleTargetAbility : ActiveAbility
{
    public SingleTargetAbility(SingleTargetAbilityDefinition definition, AbilityController controller) : base(definition, controller)
    {
        
    }

    public void Cast(GameObject target)
    {
        ApplyEffects(target);
    }
}
