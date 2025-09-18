using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Heimdallr.Core
{
    [Serializable]
    public class EventKeyUsageInfo
    {
        public GameObject GameObject;
        public MonoBehaviour Component;
        public string FieldName;
        public string UsageType; // "Raise" or "Listen"
        public string AssetPath;
        
        public EventKeyUsageInfo(GameObject gameObject, MonoBehaviour component, string fieldName, string usageType, string assetPath)
        {
            GameObject = gameObject;
            Component = component;
            FieldName = fieldName;
            UsageType = usageType;
            AssetPath = assetPath;
        }
    }

#if UNITY_EDITOR
    public static class EventKeyUsageScanner
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public static List<EventKeyUsageInfo> ScanForEventKeyUsage(EventKey targetKey)
        {
            var usageInfoList = new List<EventKeyUsageInfo>();
            
            if (targetKey == null)
            {
                Debug.LogError("Target EventKey is null");
                return usageInfoList;
            }
            
            // Scan prefabs in project
            ScanPrefabs(targetKey, usageInfoList);
            
            // Scan loaded scenes
            ScanLoadedScenes(targetKey, usageInfoList);
            
            return usageInfoList;
        }
        
        private static void ScanPrefabs(EventKey targetKey, List<EventKeyUsageInfo> usageInfoList)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    ScanGameObject(prefab, targetKey, usageInfoList, path);
                }
            }
        }
        
        private static void ScanLoadedScenes(EventKey targetKey, List<EventKeyUsageInfo> usageInfoList)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
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
                ScanType(componentType, component, targetKey, usageInfoList, assetPath);
            }
        }
        
        private static void ScanType(Type type, MonoBehaviour component, EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            // Get all fields including inherited ones
            while (type != null && type != typeof(MonoBehaviour))
            {
                FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
                
                foreach (FieldInfo field in fields)
                {
                    // Check for EventField (non-generic)
                    if (field.FieldType == typeof(EventField))
                    {
                        CheckEventField(field, component, targetKey, usageInfoList, assetPath);
                    }
                    // Check for EventField<T> (generic with one type argument)
                    else if (field.FieldType.IsGenericType && 
                             field.FieldType.GetGenericTypeDefinition() == typeof(EventField<>))
                    {
                        CheckEventFieldGeneric(field, component, targetKey, usageInfoList, assetPath);
                    }
                }
                
                type = type.BaseType;
            }
        }
        
        private static void CheckEventField(FieldInfo field, MonoBehaviour component, EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            try
            {
                EventField eventField = (EventField)field.GetValue(component);
                EventKey key = eventField.GetEventKey();
                
                if (key == targetKey)
                {
                    string usageType = DetermineUsageType(component.GetType(), field.Name);
                    usageInfoList.Add(new EventKeyUsageInfo(
                        component.gameObject,
                        component,
                        field.Name,
                        usageType,
                        assetPath
                    ));
                }
            }
            catch
            {
                // Silently handle any reflection errors
            }
        }
        
        private static void CheckEventFieldGeneric(FieldInfo field, MonoBehaviour component, EventKey targetKey, List<EventKeyUsageInfo> usageInfoList, string assetPath)
        {
            try
            {
                object eventFieldObj = field.GetValue(component);
                if (eventFieldObj == null) return;
                
                // Use reflection to get _eventKey field from generic EventField<T>
                Type eventFieldType = eventFieldObj.GetType();
                FieldInfo keyField = eventFieldType.GetField("_eventKey", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (keyField != null)
                {
                    EventKey key = keyField.GetValue(eventFieldObj) as EventKey;
                    
                    if (key == targetKey)
                    {
                        string usageType = DetermineUsageType(component.GetType(), field.Name);
                        usageInfoList.Add(new EventKeyUsageInfo(
                            component.gameObject,
                            component,
                            field.Name,
                            usageType,
                            assetPath
                        ));
                    }
                }
            }
            catch
            {
                // Silently handle any reflection errors
            }
        }
        
        private static string DetermineUsageType(Type componentType, string fieldName)
        {
            // Simple heuristic: check field name and search for method references
            string fieldNameLower = fieldName.ToLower();
            
            if (fieldNameLower.Contains("raise") || fieldNameLower.Contains("send") || 
                fieldNameLower.Contains("trigger") || fieldNameLower.Contains("fire") ||
                fieldNameLower.Contains("emit") || fieldNameLower.Contains("invoke"))
            {
                return "Raise";
            }
            
            if (fieldNameLower.Contains("listen") || fieldNameLower.Contains("receive") || 
                fieldNameLower.Contains("on") || fieldNameLower.Contains("handle") ||
                fieldNameLower.Contains("subscribe"))
            {
                return "Listen";
            }
            
            // Check methods in the component for Register/Raise calls
            MethodInfo[] methods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (MethodInfo method in methods)
            {
                try
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null)
                    {
                        string methodName = method.Name.ToLower();
                        if (methodName.Contains("register") || methodName.Contains("subscribe") || 
                            methodName.Contains("awake") || methodName.Contains("onenable") || 
                            methodName.Contains("start"))
                        {
                            return "Listen";
                        }
                    }
                }
                catch
                {
                    // Skip methods that can't be analyzed
                }
            }
            
            return "Unknown";
        }
    }
#endif
}