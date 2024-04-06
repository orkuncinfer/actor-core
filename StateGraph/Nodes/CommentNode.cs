using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommentNode : BaseNode
{
    public string Comment = "This is a comment";
    private Vector2 scrollPosition = Vector2.zero;
    private bool isTextAreaFocused = true;
    private GUIStyle myTextAreaStyle = new GUIStyle
    {
        fontSize = 15
    };
    public override void DrawWindow()
    {
        GUIStyle myTextAreaStyle = new GUIStyle();
        myTextAreaStyle.fontSize = 20;
        myTextAreaStyle.wordWrap = true;
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(190), GUILayout.Height(90));

        Comment = GUILayout.TextArea(Comment, myTextAreaStyle);

        GUILayout.EndScrollView();
    }
    
    public void FocusTextArea()
    {
        isTextAreaFocused = true;
    }
}
