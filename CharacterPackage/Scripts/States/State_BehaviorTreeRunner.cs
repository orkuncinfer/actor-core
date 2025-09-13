using Opsive.BehaviorDesigner.Runtime;
using UnityEngine;

public class State_BehaviorTreeRunner : MonoState
{
    [SerializeField] private BehaviorTree  _behaviorTree;

    protected override void OnEnter()
    {
        base.OnEnter();
        _behaviorTree.StartBehavior();
    }

    protected override void OnExit()
    {
        base.OnExit();
        _behaviorTree.StopBehavior();
    }
}
