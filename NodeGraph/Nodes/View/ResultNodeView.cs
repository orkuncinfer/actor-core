#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor
{
    [NodeType(typeof(ResultNode))]
    public class ResultNodeView : NodeView
    {
        public ResultNodeView()
        {
            title = "Result";
            Inputs.Add(CreateInputPort());
        }
    }
}
#endif