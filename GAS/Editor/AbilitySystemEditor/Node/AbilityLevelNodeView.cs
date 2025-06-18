using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using UnityEngine;
[NodeType(typeof(AbilityLevelNode))]
[Title("Ability System", "Ability", "Level")]
public class AbilityLevelNodeView : NodeView
{
    public AbilityLevelNodeView()
    {
        title = "AbilityLevel";
        Node = ScriptableObject.CreateInstance<AbilityLevelNode>();
        Output = CreateOutputPort();
        ShowLabel = true;
    }
}
