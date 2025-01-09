using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AutoCompleteAttribute))] // AutoCompleteAttribute is a custom attribute you'll define
public class AutoCompleteDrawer : PropertyDrawer
{
    // Assume AutoCompleteStringPicker is already implemented as mentioned before
    AutoCompleteStringPicker autoCompletePicker = new AutoCompleteStringPicker();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);
            string currentValue = property.stringValue;
            string newValue = autoCompletePicker.Draw( label.text, currentValue);
            if (newValue != currentValue)
            {
                property.stringValue = newValue;
            }
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}


public class AutoCompleteStringPicker
{
    // Example list of suggestions. In practice, this should be dynamically populated and managed.
    private List<string> suggestions = new List<string> { "Example1", "Example2", "Example3" };
    
    // Track the current index of the selected suggestion
    private int selectedIndex = -1;

    public string Draw(string label, string currentValue)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);

        // Register a control name to manage keyboard focus
        GUI.SetNextControlName("AutoCompleteTextField");
        string newValue = EditorGUILayout.TextField(currentValue);

        // Check if the text field has keyboard focus
        if (GUI.GetNameOfFocusedControl() == "AutoCompleteTextField")
        {
            // Optional: Implement your filtering logic here to update the 'suggestions' list based on 'newValue'
            
            // Check for arrow key input for navigating suggestions (up/down keys)
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    selectedIndex = Mathf.Min(selectedIndex + 1, suggestions.Count - 1);
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    selectedIndex = Mathf.Max(selectedIndex - 1, 0);
                    Event.current.Use();
                }
            }

            // Display suggestions if there are any and the text field is focused
            if (suggestions.Count > 0 && selectedIndex >= 0)
            {
                // Implement a basic selection mechanism
                newValue = suggestions[selectedIndex];
            }
        }
        EditorGUILayout.EndHorizontal();

        // Optional: Display the suggestions in a more sophisticated manner (e.g., dropdown, list below field)

        return newValue;
    }
}
