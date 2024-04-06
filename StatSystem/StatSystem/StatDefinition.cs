using Core.Editor;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "StatDefinition", menuName = "StatSystem/StatDefinition", order = 0)]
    public class StatDefinition : ScriptableObject
    {
        [SerializeField] private int _baseValue;
        public int BaseValue => _baseValue;
        
        [SerializeField] private int _cap;
        public int Cap => _cap;
        
        [SerializeField] private NodeGraph _formula;
        public NodeGraph Formula => _formula;
    }
}
