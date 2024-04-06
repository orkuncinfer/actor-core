using System;
using UnityEngine;

namespace Core.Editor
{
    public abstract class IntermediateNode : CodeFunctionNode
    {
        public abstract void RemoveChild(CodeFunctionNode child, string portName);
        public abstract void AddChild(CodeFunctionNode child, string portName);
        public abstract System.Collections.ObjectModel.ReadOnlyCollection<CodeFunctionNode> children { get; }
    }
}