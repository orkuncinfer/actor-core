using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Editor
{
    public class ResultNode : CodeFunctionNode
    {
        [HideInInspector] public CodeFunctionNode Child;
        public override float Value
        {
            get => Child.Value;
            set {}
        }

        public override float CalculateValue(GameObject source)
        {
            return Child.CalculateValue(source);
        }

        [Button]
        public void TestValue()
        {
            Debug.Log(Value);
        }
    }
    
}