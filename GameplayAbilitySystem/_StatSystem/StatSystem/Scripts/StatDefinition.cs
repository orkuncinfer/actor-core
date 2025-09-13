using Core.Editor;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "StatDefinition", menuName = "StatSystem/StatDefinition", order = 0)]
    public class StatDefinition : ScriptableObject
    {
        public string Title => _title;
        [SerializeField] private string _title;
        
        public int BaseValue => _baseValue;
        [SerializeField] private int _baseValue;
        
        public int Cap => _cap;
        [SerializeField] private int _cap;
        
        public NodeGraph Formula => _formula;
        [SerializeField] private NodeGraph _formula;
    }
}
