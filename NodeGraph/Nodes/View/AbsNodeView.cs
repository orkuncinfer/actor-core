using UnityEngine;

namespace Core.Editor
{
    [NodeType(typeof(AbsNode))]
    [Title("Math","Abs")]
    public class AbsNodeView : NodeView
    {
        public AbsNodeView()
        {
            title = "Abs";
            Node = ScriptableObject.CreateInstance<AbsNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
        }
    }
}