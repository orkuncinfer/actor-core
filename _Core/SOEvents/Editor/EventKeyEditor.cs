using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
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
        private bool _isScanning = false;
        private float _scanProgress = 0f;
        private string _scanStatus = "";
        private IEnumerator _scanCoroutine;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EventKey eventKey = (EventKey)target;
            
            EditorGUILayout.Space(20);
            
            // Usage Scanner Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Event Usage Scanner", EditorStyles.boldLabel);
            
            // Show progress bar if scanning
            if (_isScanning)
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Height(20)), _scanProgress, _scanStatus);
                
                if (GUILayout.Button("Cancel Scan", GUILayout.Height(25)))
                {
                    StopScan();
                }
                
                // Continue the coroutine
                if (_scanCoroutine != null)
                {
                    _scanCoroutine.MoveNext();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Scan for Usage", GUILayout.Height(30)))
                {
                    StartScan(eventKey);
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
            }
            
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
                    var bothUsages = new List<EventKeyUsageInfo>();
                    var unknownUsages = new List<EventKeyUsageInfo>();
                    
                    foreach (var usage in _cachedUsageInfo)
                    {
                        if (usage.UsageType == "Raise")
                            raiseUsages.Add(usage);
                        else if (usage.UsageType == "Listen")
                            listenUsages.Add(usage);
                        else if (usage.UsageType == "Both")
                            bothUsages.Add(usage);
                        else
                            unknownUsages.Add(usage);
                    }
                    
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(400));
                    
                    // Display Both usages
                    if (bothUsages.Count > 0)
                    {
                        GUI.color = new Color(1f, 0.9f, 0.7f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField($"🔶 Both Raising & Listening ({bothUsages.Count})", EditorStyles.boldLabel);
                        DisplayColumnHeaders();
                        foreach (var usage in bothUsages)
                        {
                            DisplayUsageInfo(usage);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                    
                    // Display Raise usages
                    if (raiseUsages.Count > 0)
                    {
                        GUI.color = new Color(1f, 0.8f, 0.8f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField($"🔴 Raising Event ({raiseUsages.Count})", EditorStyles.boldLabel);
                        DisplayColumnHeaders();
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
                        DisplayColumnHeaders();
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
                        DisplayColumnHeaders();
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
            
            // Force repaint while scanning
            if (_isScanning)
            {
                Repaint();
            }
        }
        
        private void StartScan(EventKey eventKey)
        {
            _isScanning = true;
            _scanProgress = 0f;
            _scanStatus = "Initializing scan...";
            _cachedUsageInfo = new List<EventKeyUsageInfo>();
            
            _scanCoroutine = ScanCoroutine(eventKey);
        }
        
        private void StopScan()
        {
            _isScanning = false;
            _scanCoroutine = null;
            _scanProgress = 0f;
            _scanStatus = "";
            EditorUtility.ClearProgressBar();
        }
        
        private IEnumerator ScanCoroutine(EventKey eventKey)
        {
            // Get all prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int totalItems = prefabGuids.Length + SceneManager.sceneCount;
            int currentItem = 0;
            
            // Scan prefabs
            foreach (string guid in prefabGuids)
            {
                if (!_isScanning) yield break;
                
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);
                    _scanStatus = $"Scanning: {prefabName}";
                    _scanProgress = (float)currentItem / totalItems;
                    
                    EventKeyUsageScanner.ScanGameObjectForUsage(prefab, eventKey, _cachedUsageInfo, path);
                }
                
                currentItem++;
                
                // Yield every few items to keep UI responsive
                if (currentItem % 5 == 0)
                {
                    yield return null;
                }
            }
            
            // Scan loaded scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (!_isScanning) yield break;
                
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    _scanStatus = $"Scanning scene: {scene.name}";
                    _scanProgress = (float)currentItem / totalItems;
                    
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    foreach (GameObject root in rootObjects)
                    {
                        EventKeyUsageScanner.ScanGameObjectForUsage(root, eventKey, _cachedUsageInfo, scene.path);
                    }
                }
                
                currentItem++;
                yield return null;
            }
            
            // Finish
            _isScanning = false;
            _showUsageInfo = true;
            _scanProgress = 1f;
            _scanStatus = "Scan complete!";
            Debug.Log($"Found {_cachedUsageInfo.Count} usages of EventKey '{eventKey.name}'");
            
            yield return null;
            
            _scanProgress = 0f;
            _scanStatus = "";
        }
        
        private void DisplayColumnHeaders()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Headers with bold style
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.fontStyle = FontStyle.Bold;
            
            EditorGUILayout.LabelField("Prefab", headerStyle, GUILayout.Width(150));
            EditorGUILayout.LabelField("Class", headerStyle, GUILayout.Width(150));
            EditorGUILayout.LabelField("Property", headerStyle, GUILayout.Width(150));
            EditorGUILayout.LabelField("Path", headerStyle);
            
            EditorGUILayout.EndHorizontal();
            
            // Add a separator line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
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
        
        public void OnDisable()
        {
            // Clean up progress bar if editor is closed during scan
            EditorUtility.ClearProgressBar();
            StopScan();
        }
    }
#endif
}