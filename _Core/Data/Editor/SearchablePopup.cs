#if UNITY_EDITOR


using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

public class SearchablePopup : EditorWindow
{
    private string searchString = "";
    private List<string> filteredOptions = new List<string>();
    private Action<int> onSelectCallback;
    private string[] options;
    private int selectedIndex;

    public static void Show(Rect rect, string[] options, int selectedIndex, Action<int> onSelectCallback)
    {
        var window = CreateInstance<SearchablePopup>();
        window.options = options;
        window.selectedIndex = selectedIndex;
        window.onSelectCallback = onSelectCallback;
        window.filteredOptions = options.ToList();
        window.ShowAsDropDown(rect, new Vector2(rect.width, 200));
    }

    private void OnGUI()
    {
        // Search bar
        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
        if (GUILayout.Button("X", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            searchString = "";
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        // Filter options
        if (string.IsNullOrEmpty(searchString))
        {
            filteredOptions = options.ToList();
        }
        else
        {
            filteredOptions = options.Where(o => o.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        // Display options
        foreach (var option in filteredOptions)
        {
            if (GUILayout.Button(option, GUILayout.ExpandWidth(true)))
            {
                selectedIndex = Array.IndexOf(options, option);
                onSelectCallback?.Invoke(selectedIndex);
                Close();
            }
        }
    }
}
#endif