using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransitionNode : BaseNode
{
    public Transition TargetTransition;
    public StateNode EnterState;
    public StateNode TargetState;

    public void Init(StateNode enterState, Transition transition)
    {
        EnterState = enterState;
        TargetTransition = transition;
    }
#if UNITY_EDITOR

    public override void DrawWindow()
    {
        base.DrawWindow();
        
        if(TargetTransition == null) return;
        
        EditorGUILayout.LabelField("");
        TargetTransition.Condition = (Condition)EditorGUILayout.ObjectField(TargetTransition.Condition, typeof(Condition), false);

        if (TargetTransition.Condition == null)
        {
            EditorGUILayout.LabelField("No Condition!");
        }
        else
        {
            TargetTransition.Disable = EditorGUILayout.Toggle("Disable", TargetTransition.Disable);
        }
    }

    public override void DrawCurve()
    {
        base.DrawCurve();
        if (EnterState)
        {
            Rect rect = WindowRect;
            rect.y += WindowRect.height * .5f;
            rect.width = 1;
            rect.height = 1;
            
            BehaviourEditor.DrawNodeCurve(EnterState.WindowRect,rect,true);
        }
    }
        
#endif
}