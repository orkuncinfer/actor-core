using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

using UnityEngine;

namespace Core.Editor
{
    public abstract class AbstractNode : ScriptableObject
    {
        [HideInInspector] public Vector2 Position;
        public string Guid;
        
#if UNITY_EDITOR
        public Group Group = null;
#endif
        
    }
}
