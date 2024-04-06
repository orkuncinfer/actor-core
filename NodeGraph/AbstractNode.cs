using Sirenix.OdinInspector;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Core.Editor
{
    public abstract class AbstractNode : ScriptableObject
    {
        [HideInInspector] public Vector2 Position;
        public string Guid;
        public Group Group = null;
    }
}