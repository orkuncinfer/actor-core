using UnityEngine;

#if UNITY_EDITOR
namespace Core.Editor
{
    [NodeType(typeof(SqrtNode))]
    [Title("Math","Sqrt")]
    public class SqrtNodeView : NodeView
    {
        public SqrtNodeView()
        {
            title = "Sqrt";
            Node = ScriptableObject.CreateInstance<SqrtNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
        }
    }
}
#endif