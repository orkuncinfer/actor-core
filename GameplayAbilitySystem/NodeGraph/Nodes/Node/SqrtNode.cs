using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Core.Editor
{
    public class SqrtNode : IntermediateNode
    {
        [HideInInspector] public CodeFunctionNode inputA;

        public override float Value { get; set; }

        public override float CalculateValue(GameObject source)
        {
            float value = inputA.CalculateValue(source);
            return Mathf.Sqrt(Mathf.Max(0, value));
        }

        private void OnEnable()
        {
            RefreshValue();
            if (inputA != null)
            {
                inputA.onValueValidate += OnChildValidateSelf;
            }
        }

        private void OnDisable()
        {
            if (inputA != null)
            {
                inputA.onValueValidate -= OnChildValidateSelf;
            }
        }

        public override void RemoveChild(CodeFunctionNode child, string portName)
        {
            if (portName.Equals("A"))
            {
                inputA = null;
            }
            child.onValueValidate -= OnChildValidateSelf;
            OnChildValidateSelf(-1);
        }

        public override void AddChild(CodeFunctionNode child, string portName)
        {
            if (portName.Equals("A"))
            {
                inputA = child;
            }
            child.onValueValidate += OnChildValidateSelf;
            OnChildValidateSelf(-1);
        }

        private void OnChildValidateSelf(float obj)
        {
            RefreshValue();
        }

        public override ReadOnlyCollection<CodeFunctionNode> children
        {
            get
            {
                List<CodeFunctionNode> nodes = new List<CodeFunctionNode>();
                if (inputA != null)
                {
                    nodes.Add(inputA);
                }
                return nodes.AsReadOnly();
            }
        }

        void RefreshValue()
        {
            float tempA = 0;
            if (inputA != null) tempA = inputA.Value;

            Value = Mathf.Sqrt(Mathf.Max(0, tempA));
            OnValidateSelf();
        }
    }
}
