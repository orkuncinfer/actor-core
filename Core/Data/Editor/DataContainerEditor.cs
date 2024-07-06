using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using UnityEditor.Experimental.GraphView;

[CustomEditor(typeof(DataContainer))]
public class DataContainerEditor : OdinEditor
{
    private SerializedProperty containerName;
    
    private SerializedProperty data1;
    private SerializedProperty showData1;
    
    private SerializedProperty data2;
    private SerializedProperty showData2;
    private SerializedProperty lastSelectedDataProperty;

    private DataTypeSearchProvider searchProvider;
    public GUISkin customGUISkin;

    private PropertyTree propertyTree;

    private new void OnEnable()
    {
        containerName = serializedObject.FindProperty("ContainerName");
        
        data1 = serializedObject.FindProperty("Data1");
        showData1 = serializedObject.FindProperty("showData1");
        
        data2 = serializedObject.FindProperty("Data2");
        showData2 = serializedObject.FindProperty("showData2");
        
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
        SirenixEditorGUI.BeginHorizontalToolbar();
        GUILayout.FlexibleSpace();
        containerName.stringValue = GUILayout.TextField(containerName.stringValue, GUIStyle.none);
        if (SirenixEditorGUI.ToolbarButton("▼"))
        {
            ExpandAll();
        }
        if (SirenixEditorGUI.ToolbarButton("▲"))
        {
            CollapseAll();
        }
        SirenixEditorGUI.EndHorizontalToolbar();
        GUILayout.Space(5);

        DrawProperty(data1, showData1, "Data1");
        DrawProperty(data2, showData2, "Data2");

        GUILayout.Space(20);

        if (GUILayout.Button("Add Data +"))
        {
            AddDataButton();
        }
        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
        
    }

    private void DrawProperty(SerializedProperty dataProperty, SerializedProperty showProperty, string label)
    {
        if (showProperty.boolValue)
        {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();

            if (GUILayout.Button($"{dataProperty.managedReferenceValue?.GetType().Name ?? "Select Type"}"))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), searchProvider);
                lastSelectedDataProperty = dataProperty;
            }

            if (GUILayout.Button("X", GUILayout.MaxWidth(25), GUILayout.MaxHeight(25)))
            {
                dataProperty.managedReferenceValue = null;
                // Make sure the changes are applied and the object is marked as dirty.
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            SirenixEditorGUI.EndBoxHeader();

            // Update the property tree and draw the property using Odin's PropertyTree
            propertyTree.UpdateTree();
            var property = propertyTree.GetPropertyAtPath(dataProperty.propertyPath);
            if (property != null)
            {
                property.Draw();
            }

            SirenixEditorGUI.EndBox();
        }
    }

    private void ExpandAll()
    {
        data1.isExpanded = true;
        data2.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void CollapseAll()
    {
        data1.isExpanded = false;
        data2.isExpanded = false;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void OnTypeSelected(Type selectedType)
    {
        Undo.RecordObject(target, "Change Data Type");
        lastSelectedDataProperty.managedReferenceValue = Activator.CreateInstance(selectedType);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void AddDataButton()
    {
        if (data1.managedReferenceValue == null)
        {
            showData1.boolValue = true;
        }
        else if (data2.managedReferenceValue == null)
        {
            showData2.boolValue = true;
        }
    }
}
