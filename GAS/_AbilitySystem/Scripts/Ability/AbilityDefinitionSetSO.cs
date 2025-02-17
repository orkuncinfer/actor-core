using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AbilityDefinitionSet", menuName = "ActorCore/AbilitySystem/AbilityDefinitionSet")]
public class AbilityDefinitionSetSO : ScriptableObject
{
    public List<AbilityDefinition> AbilityDefinitions;
}
