#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

[CustomPropertyDrawer(typeof(GenericKey))]
public class GenericKeyDrawer : PropertyDrawer
{
    private bool _isCreatingNewKey = false;
    private string _newKeyName = "GK_";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (!_isCreatingNewKey)
        {
            Rect fieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (property.objectReferenceValue == null)
            {
                if (GUI.Button(buttonRect, "Create"))
                {
                    _isCreatingNewKey = true;
                }
            }
        }
        else
        {
            Rect textFieldRect = new Rect(position.x, position.y, position.width - 60, position.height);
            Rect createButtonRect = new Rect(position.x + position.width - 55, position.y, 55, position.height);

            _newKeyName = EditorGUI.TextField(textFieldRect, "New Key Name", _newKeyName);

            if (GUI.Button(createButtonRect, "Create"))
            {
                if (!string.IsNullOrEmpty(_newKeyName))
                {
                    const string baseFolderPath = "Assets/_PrototypeMobile/Script Assets/GenericKeys";
                    EnsureFolderExists(baseFolderPath);

                    GenericKey newKey = ScriptableObject.CreateInstance<GenericKey>();
                    newKey.ID = _newKeyName;

                    string assetPath = $"{baseFolderPath}/{_newKeyName}.asset";
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                    AssetDatabase.CreateAsset(newKey, assetPath);
                    AssetDatabase.SaveAssets();

                    property.objectReferenceValue = newKey;
                    property.serializedObject.ApplyModifiedProperties();

                    _isCreatingNewKey = false;
                    _newKeyName = "";
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Name", "Please enter a name for the new GenericKey asset.", "OK");
                }
            }
        }

        EditorGUI.EndProperty();
    }

    private void EnsureFolderExists(string fullAssetPath)
    {
        string[] folders = fullAssetPath.Split('/');
        string currentPath = "Assets";

        for (int i = 1; i < folders.Length; i++)
        {
            string folderToCheck = $"{currentPath}/{folders[i]}";
            if (!AssetDatabase.IsValidFolder(folderToCheck))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }

            currentPath = folderToCheck;
        }
    }
}

#endif
