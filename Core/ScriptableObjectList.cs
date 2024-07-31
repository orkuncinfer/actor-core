using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptableObjectList<T> : ScriptableObject where T : ScriptableObject
{
    public List<T> Items = new List<T>();

#if UNITY_EDITOR
    /// <summary>
    /// Finds all assets of type T in the project and adds them to the list.
    /// </summary>
    ///
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        Items.Clear();

        // Get all assets of type T stored in the project
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}"); // Find all assets of type T
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T item = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (item != null)
            {
                Items.Add(item);
            }
        }
    }
#endif
}
