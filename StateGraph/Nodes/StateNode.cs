using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Serialization;

public class StateNode : BaseNode
{
    private bool _collapse;
    public BE_State CurrentState;
    public BE_State PreviousState;


#if UNITY_EDITOR
    public List<BaseNode> Dependencies = new List<BaseNode>();
    
    
    public SerializedObject SerializedState;
    public ReorderableList ActionList;

    public override void DrawWindow()
    {
        if (CurrentState == null)
        {
            EditorGUILayout.LabelField("Add state to modify");
        }
        else
        {
            if (_collapse)
            {
                WindowRect.height = 100;
            }
            else
            {
                WindowRect.height = 300;
            }

            _collapse = EditorGUILayout.Toggle(" ", _collapse);
        }

        CurrentState = (BE_State) EditorGUILayout.ObjectField(CurrentState, typeof(BE_State), false);

        if (PreviousState != CurrentState)
        {
            SerializedState = null;
            
            PreviousState = CurrentState; // current is changed
            ClearReferences();

            for (int i = 0; i < CurrentState.Transitions.Count; i++)
            {
                AddAsDependency(BehaviourEditor.AddTransitionNode(i, CurrentState.Transitions[i], this));
            }
        }

        if (CurrentState != null)
        {
            if (SerializedState == null)
            {
                SerializedState = new SerializedObject(CurrentState);
                ActionList = new ReorderableList(SerializedState, SerializedState.FindProperty("Actions"),true,true,true,true);
            }

            if (!_collapse)
            {
                SerializedState.Update();
                HandleReordableList(ActionList, "Action");
                EditorGUILayout.LabelField("");
                ActionList.DoLayoutList();
                SerializedState.ApplyModifiedProperties();

                if (ActionList.count > 7)
                {
                    float standard = 300;
                    standard += (ActionList.count - 7) * 24;
                    WindowRect.height = standard;
                }
                
            }
        }
    }

    void HandleReordableList(ReorderableList list, string targetName)
    {
        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect,targetName);
        };

        list.drawElementCallback = ((rect, index, active, focused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.ObjectField(new Rect(rect.x,rect.y,rect.width,EditorGUIUtility.singleLineHeight), element,GUIContent.none);
        });
    }

    

    public override void DrawCurve()
    {
        
    }

    public void AddAsDependency(BaseNode node)
    {
        if (!Dependencies.Contains(node))
        {
            Dependencies.Add(node);
        }
    }
    public Transition AddTransition()
    {
        return CurrentState.AddTransition();
    }

    public void ClearReferences()
    {
        BehaviourEditor.ClearWindowsFromList(Dependencies);
        Dependencies.Clear();
    }
#endif
}
