using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Core.Editor
{
    public  class DSGroup : ScriptableObject
    {
        public Vector2 Position;
        public List<CodeFunctionNode> GroupedNodes = new List<CodeFunctionNode>();
        public string Title;
        [HideInInspector] public string Guid;
        public Group GroupInstance;

        DSGroup()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}