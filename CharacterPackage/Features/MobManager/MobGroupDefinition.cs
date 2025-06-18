using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[SOCreatable("Mobs")]
public class MobGroupDefinition : ScriptableObject
{
    public string GroupName;
    [DrawWithUnity]public List<MobDefinition> Mobs;
}
