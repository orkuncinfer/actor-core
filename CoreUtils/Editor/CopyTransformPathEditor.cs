using UnityEditor;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR

public static class CopyTransformPathEditor
{
    [MenuItem("GameObject/Copy Path", false, 0)]
    private static void CopyBonePath()
    {
        GameObject selectedObject = Selection.activeGameObject;
        
        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        // Build the path from root to selected bone
        StringBuilder pathBuilder = new StringBuilder(selectedObject.name);
        Transform currentTransform = selectedObject.transform;

        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
            pathBuilder.Insert(0, $"{currentTransform.name}/");
        }

        string fullPath = pathBuilder.ToString();
        EditorGUIUtility.systemCopyBuffer = fullPath;
        
        Debug.Log($"Copied path: {fullPath}");
    }

    // Context menu option
    [MenuItem("GameObject/Copy Path", true)]
    private static bool ValidateCopyBonePath()
    {
        return Selection.activeGameObject != null;
    }
}


#endif