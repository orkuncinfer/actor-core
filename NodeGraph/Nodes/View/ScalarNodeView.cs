using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor
{
    [NodeType(typeof(ScalarNode))]
    [Title("Math","Float")]
    public class ScalarNodeView : NodeView
    {
        public ScalarNodeView()
        {
            title = "Float";
            Node = ScriptableObject.CreateInstance<ScalarNode>();
            Output = CreateOutputPort();
        }
    }
}