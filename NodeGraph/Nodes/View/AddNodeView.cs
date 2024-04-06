using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Editor
{
    [NodeType(typeof(AddNode))]
    [Title("Math", "Add")]
    public class AddNodeView : NodeView
    {
        public AddNodeView()
        {
            title = "Add";
            Node = ScriptableObject.CreateInstance<AddNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
            Inputs.Add(CreateInputPort("B"));
        }
    }
}

