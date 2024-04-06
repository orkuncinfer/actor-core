using Core;
using UnityEngine;

namespace StatSystem
{
    public class HealthModifier : StatModifier, IDamage
    {
        public bool IsCriticalHit { get; set; }
        public GameObject Instigator { get; set; }
    }

}