using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GenericKey))]  // Automatically applies to all GenericKey fields
public class GenericKeyDrawer : PropertyDrawer
{
    private bool _isCreatingNewKey = false;  // Track if we’re in the process of creating a new key
    private string _newKeyName = "GK_";         // Temporary name for the new asset

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (!_isCreatingNewKey)
        {
            // Normal display: GenericKey field with "Create" button if null
            Rect fieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (property.objectReferenceValue == null)
            {
                if (GUI.Button(buttonRect, "Create"))
                {
                    // Enter creation mode
                    _isCreatingNewKey = true;
                }
            }
        }
        else
        {
            // Display the input field for the new asset name and "Create" button
            Rect textFieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
            Rect createButtonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);

            // Display text field for asset name input
            _newKeyName = EditorGUI.TextField(textFieldRect, "New Key Name", _newKeyName);

            // Display the "Create" button for finalizing creation
            if (GUI.Button(createButtonRect, "Create"))
            {
                // Check if the name is not empty
                if (!string.IsNullOrEmpty(_newKeyName))
                {
                    // Create the new GenericKey asset with the provided name
                    GenericKey newKey = ScriptableObject.CreateInstance<GenericKey>();
                    string path = $"Assets/_Project/Script Assets/GenericKeys/{_newKeyName}.asset";
                    newKey.ID = _newKeyName;

                    // Ensure a unique path for the new asset
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                    AssetDatabase.CreateAsset(newKey, path);
                    AssetDatabase.SaveAssets();

                    // Assign the newly created asset to the property
                    property.objectReferenceValue = newKey;
                    property.serializedObject.ApplyModifiedProperties();

                    // Reset state
                    _isCreatingNewKey = false;
                    _newKeyName = ""; // Clear the temporary name
                }
                else
                {
                    // Display a warning if the name is empty
                    EditorUtility.DisplayDialog("Invalid Name", "Please enter a name for the new GenericKey asset.", "OK");
                }
            }
        }

        EditorGUI.EndProperty();
    }
}
