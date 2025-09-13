using System;
using UnityEngine;

namespace Core.Editor
{
    public abstract class CodeFunctionNode : AbstractNode
    {
        public abstract float Value { get; set; }

        public abstract float CalculateValue(GameObject source); 

        public event Action<float> onValueValidate;

        public void OnValidateSelf()
        {
            onValueValidate?.Invoke(Value);
            NodeGraphHelper.SetDirty(this);
        }
    }
}