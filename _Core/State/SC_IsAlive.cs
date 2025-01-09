using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SC_IsAlive : StateCondition
{
    public float _healthThreshold;
    
    private TestContainer _testContainer;

    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
        _testContainer = owner.GetComponent<TestContainer>();
    }

    public override bool CheckCondition()
    {
        if (_testContainer.Health <= _healthThreshold)
        {
            return true;
        }
        return false;
    }
}