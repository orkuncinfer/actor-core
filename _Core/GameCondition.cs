using UnityEngine;
[System.Serializable]
public abstract class GameCondition
{
    public abstract bool IsConditionMet(ActorBase actor);
}