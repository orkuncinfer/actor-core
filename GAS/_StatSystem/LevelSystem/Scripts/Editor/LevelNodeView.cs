using Core;
using Core.Editor;
using UnityEngine;

namespace LevelSystem.Scripts.Editor
{
    [NodeType(typeof(LevelNode))]
    [Title("Level System", "Level")]
    public class LevelNodeView : NodeView
    {
        public LevelNodeView()
        {
            title = "Level";
            Node = ScriptableObject.CreateInstance<LevelNode>();
            Output = CreateOutputPort();
            ShowLabel = false;
        }
    }
}