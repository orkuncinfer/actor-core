using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NodeGraphHelper
{
    public static List<ScriptableObject> SOSaveCache = new List<ScriptableObject>();
    
    public static void SaveScriptableObjects()
    {
#if UNITY_EDITOR
        if(SOSaveCache.Count == 0) return;
        foreach (var so in SOSaveCache)
        {
            if (so != null)
            {
                EditorUtility.SetDirty(so);
            }
        }
        AssetDatabase.SaveAssets();
        SOSaveCache.Clear();
#endif
        
    }
}
