using UnityEngine;

#if UNITY_EDITOR


namespace Core.Editor
{
    [NodeType(typeof(DivideNode))]
    [Title("Math", "Divide")]
    public class DivideNodeView : NodeView
    {
        public DivideNodeView()
        {
            title = "Divide";
            Node = ScriptableObject.CreateInstance<DivideNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
            Inputs.Add(CreateInputPort("B"));
        }
    }
}
#endif