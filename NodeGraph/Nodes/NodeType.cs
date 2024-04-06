using StatSystem;
using System;

namespace Core.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeType : System.Attribute
    {
        public readonly Type Type;

        public NodeType(Type type)
        {
            Type = type;
        }
    }
}