using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;

public class BehaviourEditor : EditorWindow
{
    #region Variables

    private static List<BaseNode> Windows = new List<BaseNode>();
    private Vector2 _mousePosition;
    private bool _makeTransition;
    private bool _clickedOnWindow;
    private BaseNode _selectedNode;
    static Color BezierColor = new Color(1, 0.6f, 0);
    
    public enum UserActions
    {
        AddState,AddTransitionNode,DeleteNode,CommentNode   
    }
    #endregion

    #region Init

    [MenuItem("Behaviour Editor/Editor")]
    static void ShowEditor()
    {
        BehaviourEditor editor = EditorWindow.GetWindow<BehaviourEditor>();
        editor.minSize = new Vector2(800, 600);
    }
    #endregion

    #region GUIMethods

    private void OnGUI()
    {
       

        Event e = Event.current;
        _mousePosition = e.mousePosition;
        UserInput(e);
        DrawWindows();
        
    }

    void DrawWindows()
    {
        BeginWindows();
        foreach (BaseNode n in Windows)
        {
            n.DrawCurve();
        }

        for (int i = 0; i < Windows.Count; i++)
        {
            Windows[i].WindowRect = GUI.Window(i, Windows[i].WindowRect, DrawNodeWindow, Windows[i].WindowName);
        }
        EndWindows();
    }

    void DrawNodeWindow(int id)
    {
        Windows[id].DrawWindow();
        GUI.DragWindow();
    }

    void UserInput(Event e)
    {
        if (e.button == 1 && !_makeTransition)
        {
           
            if (e.type == EventType.MouseDown)
            {
               
                RightClick(e);
            }
        }

        if (e.button == 0 && !_makeTransition)
        {
            if (e.type == EventType.MouseDown)
            {
                LeftClick(e);
            }
        }
        
    }

    private void LeftClick(Event e)
    {
        bool isInsideWindow = false;
        _selectedNode = null;
        for (int i = 0; i < Windows.Count; i++)
        {
            if (Windows[i].WindowRect.Contains(e.mousePosition))
            {
                isInsideWindow = true;
                _selectedNode = Windows[i];
            }
        }

        if (!isInsideWindow)
        {
            GUI.FocusControl("");
            Repaint();
        }
    }

    private void RightClick(Event e)
    {
        _clickedOnWindow = false;
        _selectedNode = null;
        for (int i = 0; i < Windows.Count; i++)
        {
            if (Windows[i].WindowRect.Contains(e.mousePosition))
            {
                _clickedOnWindow = true;
                _selectedNode = Windows[i];
                break;
            }
        }
  
        if (!_clickedOnWindow)
        {
            AddNewNode(e);
        }
        else
        {
           
            ModifyNode(e);
        }
    }

    void AddNewNode(Event e)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("AddState"), false , ContextCallbacks, UserActions.AddState);
        menu.AddItem(new GUIContent("AddComment"), false , ContextCallbacks, UserActions.CommentNode);
        menu.ShowAsContext();
        e.Use();
    }

    void ModifyNode(Event e)
    {
        GenericMenu menu = new GenericMenu();
        if (_selectedNode is StateNode)
        {
            StateNode stateNode = (StateNode) _selectedNode;
            if (stateNode.CurrentState != null)
            {
                menu.AddItem(new GUIContent("AddTransition"), false , ContextCallbacks, UserActions.AddTransitionNode);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("AddTransition"));
            }
            
            
            menu.AddItem(new GUIContent("Delete"), false , ContextCallbacks, UserActions.DeleteNode);
        }

        if (_selectedNode is CommentNode)
        {
            menu.AddItem(new GUIContent("Delete"), false , ContextCallbacks, UserActions.DeleteNode);
        }

        if (_selectedNode is TransitionNode)
        {
            menu.AddItem(new GUIContent("Delete"), false , ContextCallbacks, UserActions.DeleteNode);
        }
        
        menu.ShowAsContext();
        e.Use();
    }
    void ContextCallbacks(object o)
    {
       
        UserActions actions = (UserActions) o;

        switch (actions)
        {
            case UserActions.AddState:
              
                StateNode stateNode = new StateNode
                {
                    WindowRect = new Rect(_mousePosition.x, _mousePosition.y, 200, 300),
                    WindowName = "State"
                };
                Windows.Add(stateNode);
                break;
            case UserActions.CommentNode:
                CommentNode commentNode = new CommentNode()
                {
                    WindowRect = new Rect(_mousePosition.x, _mousePosition.y, 200, 100),
                    WindowName = "Comment"
                };
                Windows.Add(commentNode);
                break;
           
            case UserActions.AddTransitionNode:
                if (_selectedNode is StateNode)
                {
                    StateNode from = (StateNode)_selectedNode;
                    Transition transition = from.AddTransition();
                    TransitionNode transNode = AddTransitionNode(from.CurrentState.Transitions.Count, transition, from);
                    from.AddAsDependency(transNode);
                }
                break;
            default:
                break;
            case UserActions.DeleteNode:
                TryDeleteSelectedNode();
                break;
        }
    }

    #endregion

    #region HelperMethods

    public  void TryDeleteSelectedNode()
    {
        Debug.Log("hello1");
        if (_selectedNode is StateNode)
        {
            Debug.Log("hello1");
            StateNode stn = (StateNode) _selectedNode;
            stn.ClearReferences();
            Windows.Remove(stn);
        }
        if (_selectedNode is TransitionNode)
        {
            TransitionNode transNode = (TransitionNode) _selectedNode;
            Windows.Remove(transNode);

            if (transNode.EnterState.CurrentState.Transitions.Contains(transNode.TargetTransition))
            {
                transNode.EnterState.CurrentState.Transitions.Remove(transNode.TargetTransition);
            }
        }
        if (_selectedNode is CommentNode)
        {
            CommentNode cmn = (CommentNode) _selectedNode;
            Windows.Remove(cmn);
        }
    }

    public static void ClearWindowsFromList(List<BaseNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (Windows.Contains(nodes[i]))
            {
                Windows.Remove(nodes[i]);
            }
        }
    }
    public static TransitionNode AddTransitionNode(int index, Transition transition, StateNode from)
    {
        Rect fromRect = from.WindowRect;
        fromRect.x += 50;
        float targetY = fromRect.y - fromRect.height;
        if (from.CurrentState != null)
        {
            targetY += index * 100;
        }

        fromRect.y = targetY;

        TransitionNode transitionNode = CreateInstance<TransitionNode>();
        transitionNode.Init(from,transition);
        transitionNode.WindowRect = new Rect(fromRect.x + 200 + 100, fromRect.y + (fromRect.height * .7f), 200, 80);
        transitionNode.WindowName = "Condition Check";
        Windows.Add(transitionNode);
        return transitionNode;
    }
    public static void DrawNodeCurve(Rect start, Rect end, bool left)
    {
        Vector3 startPos = new Vector3(
            left ? start.x + start.width : start.x,
            start.y + (start.height * .5f),
            0);

        Vector3 endPos = new Vector3(end.x + (end.width * .5f), end.y + (end.height * .5f), 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;

        Color shadow = new Color(1, 1, 1, 0.1f);
        for (int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos,endPos,startTan,endTan,shadow,null,(i+1)* 4);
        }
        Handles.DrawBezier(startPos,endPos,startTan,endTan,Color.green, null,4); 
    }
    
    #endregion
}