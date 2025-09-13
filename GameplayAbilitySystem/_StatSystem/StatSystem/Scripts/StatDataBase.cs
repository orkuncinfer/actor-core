using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "StatDataBase", menuName = "StatSystem/StatDataBase", order = 0)]
    public class StatDataBase : ScriptableObject
    {
        [ReadOnly]public List<StatDefinition> Stats;
        [ReadOnly]public List<StatDefinition> Attributes;
        [ReadOnly]public List<StatDefinition> PrimaryStats;
    }
}
