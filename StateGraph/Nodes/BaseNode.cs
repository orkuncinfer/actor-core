using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BaseNode : ScriptableObject
{
    public Rect WindowRect;
    public string WindowName;
    
    public virtual void DrawWindow()
    {
        
    }

    public virtual void DrawCurve()
    {
        
    }
}
