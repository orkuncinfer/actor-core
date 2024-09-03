using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionDefinition : ScriptableObject
{
    public abstract bool IsConditionMet(ActorBase actor);
    
}
