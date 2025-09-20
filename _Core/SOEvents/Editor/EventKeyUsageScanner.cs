using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
#endif

namespace Heimdallr.Core
{
    [Serializable]
    public class EventKeyUsageInfo
    {
        public GameObject GameObject;
        public MonoBehaviour Component;
        public string FieldName;
        public string UsageType; // "Raise", "Listen", or "Both"
        public string AssetPath;
        public List<string> MethodsWhereUsed;
        
        public EventKeyUsageInfo(GameObject gameObject, MonoBehaviour component, string fieldName, string usageType, string assetPath)
        {
            GameObject = gameObject;
            Component = component;
            FieldName = fieldName;
            UsageType = usageType;
            AssetPath = assetPath;
            MethodsWhereUsed = new List<string>();
        }
    }

#if UNITY_EDITOR
    public static class EventKeyUsageScanner
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public static void ScanGameObjectForUsage(GameObject gameObject, EventKey targetKey, 
            List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            ScanGameObject(gameObject, targetKey, usageInfoList, assetPath);
        }
        
        public static List<EventKeyUsageInfo> ScanForEventKeyUsage(EventKey targetKey, System.Action<float, string> progressCallback = null)
        {
            var usageInfoList = new List<EventKeyUsageInfo>();
            
            if (targetKey == null)
            {
                Debug.LogError("Target EventKey is null");
                return usageInfoList;
            }
            
            progressCallback?.Invoke(0f, "Starting scan...");
            
            // Scan prefabs in project
            ScanPrefabs(targetKey, usageInfoList, progressCallback);
            
            // Scan loaded scenes
            ScanLoadedScenes(targetKey, usageInfoList, progressCallback);
            
            progressCallback?.Invoke(1f, "Scan complete!");
            
            return usageInfoList;
        }
        
        private static void ScanPrefabs(EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, System.Action<float, string> progressCallback)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int totalPrefabs = prefabGuids.Length;
            
            for (int i = 0; i < totalPrefabs; i++)
            {
                string guid = prefabGuids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    // Update progress
                    float progress = (float)i / totalPrefabs * 0.8f; // 80% of progress for prefabs
                    string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);
                    progressCallback?.Invoke(progress, $"Scanning prefabs... ({i + 1}/{totalPrefabs}): {prefabName}");
                    
                    ScanGameObject(prefab, targetKey, usageInfoList, path);
                }
            }
        }
        
        private static void ScanLoadedScenes(EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, System.Action<float, string> progressCallback)
        {
            int sceneCount = SceneManager.sceneCount;
            
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    // Update progress (scenes are the last 20%)
                    float progress = 0.8f + (float)i / sceneCount * 0.2f;
                    progressCallback?.Invoke(progress, $"Scanning scene: {scene.name}");
                    
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    foreach (GameObject root in rootObjects)
                    {
                        ScanGameObject(root, targetKey, usageInfoList, scene.path);
                    }
                }
            }
        }
        
        private static void ScanGameObject(GameObject gameObject, EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            MonoBehaviour[] components = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            
            foreach (MonoBehaviour component in components)
            {
                if (component == null) continue;
                
                Type componentType = component.GetType();
                
                // First check the MonoBehaviour itself
                var eventFields = FindEventFieldsWithKey(componentType, component, targetKey);
                
                foreach (var fieldInfo in eventFields)
                {
                    string usageType = AnalyzeFieldUsage(componentType, fieldInfo.Name);
                    
                    var usageInfo = new EventKeyUsageInfo(
                        gameObject,
                        component,
                        fieldInfo.Name,
                        usageType,
                        assetPath
                    );
                    
                    usageInfoList.Add(usageInfo);
                }
                
                // Now also check serializable fields within the MonoBehaviour
                ScanSerializableFields(component, componentType, gameObject, targetKey, usageInfoList, assetPath);
            }
        }
        
        private static void ScanSerializableFields(MonoBehaviour component, Type componentType, GameObject gameObject, 
            EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            FieldInfo[] fields = componentType.GetFields(BINDING_FLAGS);
            
            foreach (FieldInfo field in fields)
            {
                // Skip if it's directly an EventField (already handled)
                if (field.FieldType == typeof(EventField) || 
                    (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(EventField<>)))
                    continue;
                
                // Check if this field is a serializable class that might contain EventFields
                if (field.FieldType.IsClass && field.FieldType.IsSerializable && !field.FieldType.IsArray)
                {
                    try
                    {
                        object fieldValue = field.GetValue(component);
                        if (fieldValue != null)
                        {
                            ScanSerializableObject(fieldValue, field.FieldType, component, gameObject, 
                                targetKey, usageInfoList, assetPath, field.Name);
                        }
                    }
                    catch { }
                }
            }
        }
        
        private static void ScanSerializableObject(object obj, Type objType, MonoBehaviour parentComponent, 
            GameObject gameObject, EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, 
            string assetPath, string parentFieldName)
        {
            // Find EventFields in this serializable object
            var eventFields = FindEventFieldsInObject(obj, objType, targetKey);
            
            foreach (var fieldInfo in eventFields)
            {
                // Analyze usage in the serializable class's code
                string usageType = AnalyzeFieldUsage(objType, fieldInfo.Name);
                
                // Create a descriptive name showing the nested structure
                string fullFieldName = $"{parentFieldName}.{fieldInfo.Name}";
                
                var usageInfo = new EventKeyUsageInfo(
                    gameObject,
                    parentComponent,
                    fullFieldName,
                    usageType,
                    assetPath
                );
                
                usageInfoList.Add(usageInfo);
            }
        }
        
        private static List<FieldInfo> FindEventFieldsInObject(object obj, Type type, EventKey targetKey)
        {
            var matchingFields = new List<FieldInfo>();
            
            while (type != null && type != typeof(object))
            {
                FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
                
                foreach (FieldInfo field in fields)
                {
                    // Check for EventField (non-generic)
                    if (field.FieldType == typeof(EventField))
                    {
                        try
                        {
                            EventField eventField = (EventField)field.GetValue(obj);
                            EventKey key = eventField.GetEventKey();
                            
                            if (key == targetKey)
                            {
                                matchingFields.Add(field);
                            }
                        }
                        catch { }
                    }
                    // Check for EventField<T> (generic with one type argument)
                    else if (field.FieldType.IsGenericType && 
                             field.FieldType.GetGenericTypeDefinition() == typeof(EventField<>))
                    {
                        try
                        {
                            object eventFieldObj = field.GetValue(obj);
                            if (eventFieldObj != null)
                            {
                                Type eventFieldType = eventFieldObj.GetType();
                                FieldInfo keyField = eventFieldType.GetField("_eventKey", BindingFlags.NonPublic | BindingFlags.Instance);
                                
                                if (keyField != null)
                                {
                                    EventKey key = keyField.GetValue(eventFieldObj) as EventKey;
                                    
                                    if (key == targetKey)
                                    {
                                        matchingFields.Add(field);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                type = type.BaseType;
            }
            
            return matchingFields;
        }
        
        private static List<FieldInfo> FindEventFieldsWithKey(Type type, MonoBehaviour component, EventKey targetKey)
        {
            var matchingFields = new List<FieldInfo>();
            
            while (type != null && type != typeof(MonoBehaviour))
            {
                FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
                
                foreach (FieldInfo field in fields)
                {
                    // Check for EventField (non-generic)
                    if (field.FieldType == typeof(EventField))
                    {
                        try
                        {
                            EventField eventField = (EventField)field.GetValue(component);
                            EventKey key = eventField.GetEventKey();
                            
                            if (key == targetKey)
                            {
                                matchingFields.Add(field);
                            }
                        }
                        catch { }
                    }
                    // Check for EventField<T> (generic with one type argument)
                    else if (field.FieldType.IsGenericType && 
                             field.FieldType.GetGenericTypeDefinition() == typeof(EventField<>))
                    {
                        try
                        {
                            object eventFieldObj = field.GetValue(component);
                            if (eventFieldObj != null)
                            {
                                Type eventFieldType = eventFieldObj.GetType();
                                FieldInfo keyField = eventFieldType.GetField("_eventKey", BindingFlags.NonPublic | BindingFlags.Instance);
                                
                                if (keyField != null)
                                {
                                    EventKey key = keyField.GetValue(eventFieldObj) as EventKey;
                                    
                                    if (key == targetKey)
                                    {
                                        matchingFields.Add(field);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                type = type.BaseType;
            }
            
            return matchingFields;
        }
        
        private static string AnalyzeFieldUsage(Type componentType, string fieldName)
        {
            bool hasRaise = false;
            bool hasRegister = false;
            
            // Get the script file path
            MonoScript script = GetMonoScriptFromType(componentType);
            if (script == null) return "Unknown";
            
            string scriptPath = AssetDatabase.GetAssetPath(script);
            if (string.IsNullOrEmpty(scriptPath)) return "Unknown";
            
            try
            {
                string scriptContent = File.ReadAllText(scriptPath);
                
                // Remove all comments from script to avoid false positives
                string scriptWithoutComments = RemoveComments(scriptContent);
                
                // Split into lines for analysis
                string[] lines = scriptWithoutComments.Split('\n');
                
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    // Check for Raise pattern: fieldName.Raise(
                    if (ContainsMethodCall(trimmedLine, fieldName, "Raise"))
                    {
                        hasRaise = true;
                    }
                    
                    // Check for Register pattern: fieldName.Register(
                    if (ContainsMethodCall(trimmedLine, fieldName, "Register"))
                    {
                        hasRegister = true;
                    }
                    
                    // Check for Unregister pattern: fieldName.Unregister(
                    if (ContainsMethodCall(trimmedLine, fieldName, "Unregister"))
                    {
                        hasRegister = true; // Unregister implies it was registered
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not analyze script file: {scriptPath}. Error: {e.Message}");
                return "Unknown";
            }
            
            // Determine usage type based on what we found
            if (hasRaise && hasRegister)
                return "Both";
            else if (hasRaise)
                return "Raise";
            else if (hasRegister)
                return "Listen";
            else
                return "Unknown";
        }
        
        
        private static bool ContainsMethodCall(string line, string fieldName, string methodName)
        {
            // Check if line contains the field name
            if (!line.Contains(fieldName)) return false;
            if (!line.Contains(methodName)) return false;
            
            // Find the field position
            int fieldIndex = line.IndexOf(fieldName);
            if (fieldIndex == -1) return false;
            
            // Look for the pattern: fieldName.methodName or fieldName?.methodName
            // We need to find the method call that's actually on this field
            int currentIndex = fieldIndex;
            while (currentIndex != -1)
            {
                int afterFieldIndex = currentIndex + fieldName.Length;
                
                // Look for the method call pattern after the field
                int searchStart = afterFieldIndex;
                int methodIndex = line.IndexOf(methodName, searchStart);
                
                if (methodIndex != -1)
                {
                    // Check what's between field and method (should be . or ?. with possible spaces)
                    string between = line.Substring(afterFieldIndex, methodIndex - afterFieldIndex);
                    between = between.Replace(" ", ""); // Remove spaces
                    
                    if (between == "." || between == "?.")
                    {
                        // Check what's after the method name (should be '(' )
                        int afterMethodIndex = methodIndex + methodName.Length;
                        if (afterMethodIndex < line.Length)
                        {
                            // Check for opening parenthesis (may have spaces before it)
                            for (int i = afterMethodIndex; i < line.Length; i++)
                            {
                                char c = line[i];
                                if (c == '(') return true;
                                if (c != ' ') break; // If we hit something other than space or (, it's not a method call
                            }
                        }
                    }
                }
                
                // Look for next occurrence of fieldName
                currentIndex = line.IndexOf(fieldName, currentIndex + 1);
            }
            
            return false;
        }
        
        private static string RemoveComments(string code)
        {
            var result = new System.Text.StringBuilder();
            bool inString = false;
            bool inChar = false;
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;
            
            for (int i = 0; i < code.Length; i++)
            {
                char current = code[i];
                char next = i + 1 < code.Length ? code[i + 1] : '\0';
                
                // Handle string literals
                if (!inSingleLineComment && !inMultiLineComment && !inChar && current == '"')
                {
                    if (i == 0 || code[i - 1] != '\\')
                    {
                        inString = !inString;
                    }
                }
                
                // Handle char literals
                if (!inSingleLineComment && !inMultiLineComment && !inString && current == '\'')
                {
                    if (i == 0 || code[i - 1] != '\\')
                    {
                        inChar = !inChar;
                    }
                }
                
                // Handle comments
                if (!inString && !inChar)
                {
                    if (!inSingleLineComment && !inMultiLineComment && current == '/' && next == '/')
                    {
                        inSingleLineComment = true;
                        i++; // Skip next character
                        continue;
                    }
                    
                    if (!inSingleLineComment && !inMultiLineComment && current == '/' && next == '*')
                    {
                        inMultiLineComment = true;
                        i++; // Skip next character
                        continue;
                    }
                    
                    if (inSingleLineComment && current == '\n')
                    {
                        inSingleLineComment = false;
                    }
                    
                    if (inMultiLineComment && current == '*' && next == '/')
                    {
                        inMultiLineComment = false;
                        i++; // Skip next character
                        continue;
                    }
                }
                
                // Add character to result if not in comment
                if (!inSingleLineComment && !inMultiLineComment)
                {
                    result.Append(current);
                }
            }
            
            return result.ToString();
        }
        
        private static MonoScript GetMonoScriptFromType(Type type)
        {
            var scripts = MonoImporter.GetAllRuntimeMonoScripts();
            foreach (var script in scripts)
            {
                if (script != null && script.GetClass() == type)
                    return script;
            }
            return null;
        }
    }
#endif
}