using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Core.Editor
{
    public class MultiplyNode : IntermediateNode
    {
        [HideInInspector] public CodeFunctionNode inputA;
        [HideInInspector] public CodeFunctionNode inputB;

        public override float Value { get; set; }
        public override float CalculateValue(GameObject source)
        {
            return  Value = inputA.CalculateValue(source) * inputB.CalculateValue(source);
        }

        private void OnEnable()
        {
            RefreshValue();
            if (inputA != null)
            {
                inputA.onValueValidate += OnChildValidateSelf;
            }
            if (inputB != null)
            {
                inputB.onValueValidate += OnChildValidateSelf;
            }
        }

        private void OnDisable()
        {
            if (inputA != null)
            {
                inputA.onValueValidate -= OnChildValidateSelf;
            }
            if (inputB != null)
            {
                inputB.onValueValidate -= OnChildValidateSelf;
            }
        }
        public override void RemoveChild(CodeFunctionNode child, string portName)
        {
            if (portName.Equals("A"))
            {
                inputA = null;
            }
            else
            {
                inputB = null;
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
            else
            {
                inputB = child;
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

                if (inputB != null)
                {
                    nodes.Add(inputB);
                }

                return nodes.AsReadOnly();
            }
        }

        void RefreshValue()
        {
            float tempA = 0;
            float tempB = 0;
            if (inputA != null) tempA = inputA.Value;
            if (inputB != null) tempB = inputB.Value;

            Value = tempA * tempB;
            OnValidateSelf();
        }
    }
}