#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathDestination))]
public class PathDestinationEditor : Editor
{
    private void OnSceneGUI()
    {
        PathDestination targetScript = (PathDestination)target;

        // Check if the selected object is exactly the target (not its parent)
        Tools.hidden = Selection.activeTransform == targetScript.transform;

        // Use Handles.PositionHandle to allow moving the position
        EditorGUI.BeginChangeCheck();
        
        Vector3 newPosition = Handles.PositionHandle(targetScript.Destination, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetScript, "Move Vector Handle");
            targetScript.Destination = newPosition;
        }
    }
}
#endif
