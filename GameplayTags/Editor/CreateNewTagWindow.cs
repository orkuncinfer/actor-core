using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

public class CreateNewTagWindow : EditorWindow
{
    public static void ShowWindow(EditorWindow parent,Rect buttonRect)
    {
        var window = CreateInstance<CreateNewTagWindow>();

        var posRect = GUIUtility.GUIToScreenRect(buttonRect);
        var windowPosition = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));
        window.position = new Rect(windowPosition, new Vector2(150, 200)); 
        window.wantsMouseMove = true;
        window.ShowAsDropDown(posRect,new Vector2(200,200));
    }
    
    private void OnGUI()
    {
        if (Event.current.type == EventType.MouseMove)
        {
            Repaint();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUI.Button(new Rect(4,4,200,200),titleContent))
        {
                Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
