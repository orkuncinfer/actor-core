using UnityEngine;

namespace Core.Editor
{
    [NodeType(typeof(PowerNode))]
    [Title("Math", "Power")]
    public class PowerNodeView : NodeView
    {
        public PowerNodeView()
        {
            title = "Power";
            Node = ScriptableObject.CreateInstance<PowerNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
            Inputs.Add(CreateInputPort("B"));
        }
    }
}