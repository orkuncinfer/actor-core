
#if false
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using UnityEditor.Experimental.GraphView;


[CustomEditor(typeof(DataSingle))]
public class DataSingleDrawer : OdinEditor
{
     private SerializedProperty containerName;
    
    private SerializedProperty data1;
    
    private SerializedProperty lastSelectedDataProperty;

    private DataTypeSearchProvider searchProvider;
    public GUISkin customGUISkin;

    private PropertyTree propertyTree;

    private new void OnEnable()
    {
        containerName = serializedObject.FindProperty("ChangeName");
        
        data1 = serializedObject.FindProperty("Data");
        
        
        // Initialize the search provider
        searchProvider = ScriptableObject.CreateInstance<DataTypeSearchProvider>();
        searchProvider.Initialize(OnTypeSelected);

        // Initialize the property tree
        propertyTree = PropertyTree.Create(serializedObject);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (customGUISkin != null)
        {
            GUI.skin = customGUISkin;
        }
        
        DrawProperty(data1, "Data1");
    }

    private void DrawProperty(SerializedProperty dataProperty, string label)
    {
        if (true)
        {
            EditorGUI.BeginChangeCheck();
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 20;
            
            if (GUILayout.Button($"{dataProperty.managedReferenceValue?.GetType().Name ?? "Select Type"}", buttonStyle,GUILayout.Height(30)))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), searchProvider);
                lastSelectedDataProperty = dataProperty;
            }

            if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
            {
                dataProperty.managedReferenceValue = null;
                // Make sure the changes are applied and the object is marked as dirty.
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            

            SirenixEditorGUI.EndBoxHeader();

            // Update the property tree and draw the property using Odin's PropertyTree
        
          
            var property = propertyTree.GetPropertyAtPath(dataProperty.propertyPath);
            if (property != null)
            {
                GUIHelper.PushGUIEnabled(true);
                property.Draw();
                GUIHelper.PopGUIEnabled();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("degisiklikk");
                Undo.RecordObject(target, "Change Data Type");
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            propertyTree.UpdateTree();
            SirenixEditorGUI.EndBox();
        }
    }

    private void OnTypeSelected(Type selectedType)
    {
        Undo.RecordObject(target, "Change Data Type");
        lastSelectedDataProperty.managedReferenceValue = Activator.CreateInstance(selectedType);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
#endif