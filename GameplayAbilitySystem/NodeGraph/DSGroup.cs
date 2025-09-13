using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

namespace Core.Editor
{
    public  class DSGroup : ScriptableObject
    {
        public Vector2 Position;
        public List<CodeFunctionNode> GroupedNodes = new List<CodeFunctionNode>();
        public string Title;
        
#if UNITY_EDITOR
        public Group GroupInstance;
#endif
        [HideInInspector] public string Guid;
       

        DSGroup()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}