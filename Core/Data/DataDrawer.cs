#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class DataDrawer<T> : OdinValueDrawer<T> where T : Data
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        SirenixEditorGUI.BeginBox();

        this.CallNextDrawer(label);
        
        SirenixEditorGUI.EndBox();
        
    }
}
#endif
