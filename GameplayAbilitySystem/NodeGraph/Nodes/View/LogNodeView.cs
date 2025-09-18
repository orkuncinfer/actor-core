using UnityEngine;

#if UNITY_EDITOR

namespace Core.Editor
{
    [NodeType(typeof(LogNode))]
    [Title("Math","Log")]
    public class LogNodeView : NodeView
    {
        public LogNodeView()
        {
            title = "Log";
            Node = ScriptableObject.CreateInstance<LogNode>();
            Output = CreateOutputPort();
            Inputs.Add(CreateInputPort("A"));
        }
    }
}
#endif