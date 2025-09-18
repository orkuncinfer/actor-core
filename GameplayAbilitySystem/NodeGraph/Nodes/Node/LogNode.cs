using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Core.Editor
{
    public class LogNode : IntermediateNode
    {
        [HideInInspector] public CodeFunctionNode inputA;

        public override float Value { get; set; }
        public override float CalculateValue(GameObject source)
        {
            float value = inputA.CalculateValue(source);
            return value > 0 ? Mathf.Log(value) : 0f;
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

            Value = tempA > 0 ? Mathf.Log(tempA) : 0f;
            OnValidateSelf();
        }
    }
}