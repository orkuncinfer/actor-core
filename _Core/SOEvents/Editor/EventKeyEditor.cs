using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
#endif

namespace Heimdallr.Core
{
#if UNITY_EDITOR
    [CustomEditor(typeof(EventKey))]
    public class EventKeyEditor : OdinEditor
    {
        private List<EventKeyUsageInfo> _cachedUsageInfo;
        private bool _showUsageInfo = false;
        private Vector2 _scrollPosition;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EventKey eventKey = (EventKey)target;
            
            EditorGUILayout.Space(20);
            
            // Usage Scanner Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Event Usage Scanner", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Scan for Usage", GUILayout.Height(30)))
            {
                _cachedUsageInfo = EventKeyUsageScanner.ScanForEventKeyUsage(eventKey);
                _showUsageInfo = true;
                Debug.Log($"Found {_cachedUsageInfo.Count} usages of EventKey '{eventKey.name}'");
            }
            
            if (_cachedUsageInfo != null && _cachedUsageInfo.Count > 0)
            {
                if (GUILayout.Button(_showUsageInfo ? "Hide Results" : "Show Results", GUILayout.Width(100)))
                {
                    _showUsageInfo = !_showUsageInfo;
                }
                
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    _cachedUsageInfo = null;
                    _showUsageInfo = false;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Display results
            if (_showUsageInfo && _cachedUsageInfo != null)
            {
                EditorGUILayout.Space(10);
                
                if (_cachedUsageInfo.Count == 0)
                {
                    EditorGUILayout.HelpBox("No usages found for this EventKey.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField($"Found {_cachedUsageInfo.Count} Usage(s):", EditorStyles.boldLabel);
                    
                    // Group by usage type
                    var raiseUsages = new List<EventKeyUsageInfo>();
                    var listenUsages = new List<EventKeyUsageInfo>();
                    var unknownUsages = new List<EventKeyUsageInfo>();
                    
                    foreach (var usage in _cachedUsageInfo)
                    {
                        if (usage.UsageType == "Raise")
                            raiseUsages.Add(usage);
                        else if (usage.UsageType == "Listen")
                            listenUsages.Add(usage);
                        else
                            unknownUsages.Add(usage);
                    }
                    
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(400));
                    
                    // Display Raise usages
                    if (raiseUsages.Count > 0)
                    {
                        GUI.color = new Color(1f, 0.8f, 0.8f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField($"🔴 Raising Event ({raiseUsages.Count})", EditorStyles.boldLabel);
                        foreach (var usage in raiseUsages)
                        {
                            DisplayUsageInfo(usage);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                    
                    // Display Listen usages
                    if (listenUsages.Count > 0)
                    {
                        GUI.color = new Color(0.8f, 1f, 0.8f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField($"🟢 Listening to Event ({listenUsages.Count})", EditorStyles.boldLabel);
                        foreach (var usage in listenUsages)
                        {
                            DisplayUsageInfo(usage);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                    
                    // Display Unknown usages
                    if (unknownUsages.Count > 0)
                    {
                        GUI.color = new Color(0.9f, 0.9f, 0.9f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField($"❓ Unknown Usage ({unknownUsages.Count})", EditorStyles.boldLabel);
                        foreach (var usage in unknownUsages)
                        {
                            DisplayUsageInfo(usage);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DisplayUsageInfo(EventKeyUsageInfo usage)
        {
            EditorGUILayout.BeginHorizontal();
            
            // GameObject/Prefab name with ping button
            if (GUILayout.Button(usage.GameObject.name, EditorStyles.linkLabel, GUILayout.Width(150)))
            {
                if (usage.GameObject != null)
                {
                    EditorGUIUtility.PingObject(usage.GameObject);
                    Selection.activeGameObject = usage.GameObject;
                }
            }
            
            // Component type
            string componentName = usage.Component != null ? usage.Component.GetType().Name : "Unknown";
            EditorGUILayout.LabelField(componentName, GUILayout.Width(150));
            
            // Field name
            EditorGUILayout.LabelField($"[{usage.FieldName}]", GUILayout.Width(150));
            
            // Asset path (truncated)
            string displayPath = usage.AssetPath;
            if (displayPath.Length > 30)
            {
                displayPath = "..." + displayPath.Substring(displayPath.Length - 30);
            }
            EditorGUILayout.LabelField(displayPath, EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}