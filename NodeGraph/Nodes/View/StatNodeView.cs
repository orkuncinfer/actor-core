using UnityEngine;

namespace Core.Editor
{
    [NodeType(typeof(StatNode))]
    [Title("StatSystem","Stat")]
    public class StatNodeView : NodeView
    {
        public StatNodeView()
        {
            title = "Stat";
            Node = ScriptableObject.CreateInstance<StatNode>();
            Output = CreateOutputPort();
            this.style.width = 130;
        }
    }
}