using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class EventCreatorWindow : EditorWindow
{
    private string typeName = "String";
    private bool isWaitingForCompile = false;

    [MenuItem("Window/Event Creator")]
    public static void ShowWindow()
    {
        GetWindow<EventCreatorWindow>("Event Creator");
    }

    void OnGUI()
    {
        typeName = EditorGUILayout.TextField("Type Name:", typeName);

        if (GUILayout.Button("Generate and Create GameEvent Asset"))
        {
            GenerateGameEventClass(typeName);
            isWaitingForCompile = true;
            EditorApplication.update += WaitForCompile;
        }

        if (isWaitingForCompile)
        {
            EditorGUILayout.LabelField("Waiting for script compilation to finish...");
        }
    }

    void WaitForCompile()
    {
        if (!EditorApplication.isCompiling)
        {
            EditorApplication.update -= WaitForCompile;
            CreateGameEventAsset(typeName);
            isWaitingForCompile = false;
        }
    }

    private void CreateGameEventAsset(string typeName)
    {
        string className = typeName + "GameEvent";
        System.Type gameEventType = System.Type.GetType(className + ",Assembly-CSharp");
        if (gameEventType != null)
        {
            ScriptableObject gameEvent = ScriptableObject.CreateInstance(gameEventType);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{className}.asset");
            AssetDatabase.CreateAsset(gameEvent, assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameEvent;
            Debug.Log($"GameEvent<{typeName}> created and asset saved: {assetPath}");
        }
        else
        {
            Debug.LogError("Failed to create GameEvent type. Ensure scripts are compiled correctly.");
        }
    }

    private void GenerateGameEventClass(string typeName)
    {
        string fileName = typeName + "GameEvent.cs";
        string directoryPath = Path.Combine(Application.dataPath, "Scripts/GeneratedGameEvents");
        Directory.CreateDirectory(directoryPath);  // Ensure the directory exists
        string filePath = Path.Combine(directoryPath, fileName);

        string template = @"
using UnityEngine;
using System;  // Add more namespaces here if needed

[CreateAssetMenu(fileName = ""New {0} GameEvent"", menuName = ""Game Events/{0} Game Event"")]
public class {0}GameEvent : GameEvent<{1}> {{ }}
";
        string content = string.Format(template, typeName, typeName);
        File.WriteAllText(filePath, content);
        AssetDatabase.Refresh();
    }
}
