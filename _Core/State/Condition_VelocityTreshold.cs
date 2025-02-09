using System;
using ECM2;
using UnityEngine;

[Serializable]
public class Condition_VelocityTreshold : StateCondition
{
    [SerializeField] private float _treshold;
    private Character _character;
    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
        _character = owner.GetComponent<Character>();
    }

    public override bool CheckCondition()
    {
        if (_character.GetVelocity().magnitude > _treshold)
        {
            return true;
        }

        return false;
    }
}