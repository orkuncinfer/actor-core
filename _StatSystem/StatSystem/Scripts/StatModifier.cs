using UnityEngine;

namespace StatSystem
{
    public enum ModifierOperationType
    {
        Additive,
        Multiplicative,
        Override
    }
    [System.Serializable]
    public class StatModifier
    {
        public object Source { get; set; }
        public float Magnitude { get; set; }
        public ModifierOperationType Type { get; set; }
        
        public override string ToString()
        {
            return Magnitude.ToString();
        }
    }
}
