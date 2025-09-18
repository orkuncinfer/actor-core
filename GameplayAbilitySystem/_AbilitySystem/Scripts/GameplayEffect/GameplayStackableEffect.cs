using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStackableEffect : GameplayPersistentEffect
{
    public new GameplayStackableEffectDefinition Definition => _definition as GameplayStackableEffectDefinition;
    public int StackCount;
    public GameplayStackableEffect(GameplayStackableEffectDefinition definition, object source, GameObject instigator,GameObject victim) : base(definition, source, instigator,victim)
    {
        StackCount = 1;
    }
}
