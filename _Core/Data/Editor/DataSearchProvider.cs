#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

public class DataTypeSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private Action<Type> onSelectCallback;
    private List<SearchTreeEntry> searchTreeEntries;
    private Texture2D icon;

    public void Initialize(Action<Type> onSelectCallback)
    {
        this.onSelectCallback = onSelectCallback;

        // Initialize the search tree entries with Data types
        var dataTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Data)) && !type.IsAbstract)
            .ToArray();

        searchTreeEntries = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Data Types"), 0)
        };

        foreach (var dataType in dataTypes)
        {
            searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(dataType.Name, icon))
            {
                userData = dataType,
                level = 1
            });
        }

        // Create a transparent icon for list entries
        icon = new Texture2D(1, 1);
        icon.SetPixel(0, 0, Color.clear);
        icon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        return searchTreeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (entry.userData is Type selectedType)
        {
            onSelectCallback?.Invoke(selectedType);
            return true;
        }
        return false;
    }
}

#endif