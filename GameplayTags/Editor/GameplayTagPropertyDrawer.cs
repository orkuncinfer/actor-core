using System.Collections.Generic;
using System.Linq;
using DG.DemiEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(GameplayTag))]
public class GameplayTagPropertyDrawer : PropertyDrawer
{
    private bool isInitialized = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty fullTagProperty = property.FindPropertyRelative("fullTag");
        SerializedProperty rootProperty = property;
        
        if (!isInitialized)
        {
            // Initialization code here
            if (ReflectionUtils.GetParent(fullTagProperty) is GameplayTag tag)
            {
                GameplayTagsAsset gameplayTagsAsset = Resources.Load<GameplayTagsAsset>("GameplayTagList");
                tag.Fetch(gameplayTagsAsset);
            }
            isInitialized = true;
        }
        EditorGUI.BeginProperty(position, label, property);

        GameplayTag gameplayTag = (GameplayTag) property.boxedValue;

        
       
       
        Rect controlRect = EditorGUI.PrefixLabel(position, label);

        GUIStyle btnStyle = new GUIStyle(EditorStyles.textField)
        {
            alignment = TextAnchor.MiddleLeft
        };
        string fieldText = "";
        if (fullTagProperty.stringValue.IsNullOrEmpty())
        {
            fieldText = "Click to assign a tag";
            btnStyle.fontStyle = FontStyle.Italic;
            btnStyle.normal.textColor = Color.red;
        }
        else
        {
            btnStyle.normal.textColor = Color.cyan;
            btnStyle.fontStyle = FontStyle.Normal;
            fieldText = fullTagProperty.stringValue;
        }
        if (GUI.Button(controlRect, fieldText, btnStyle))
        {
            //TagSelectorPopup(controlRect, fullTagProperty,gameplayTag,rootProperty);
            GameplayTagTree treeWindow = new GameplayTagTree(tag =>
            {
                Debug.Log($"Callback received: {tag}");
                GameplayTag gTag = GameplayTagManger2.RequestTagHash(tag);
                //apply gameplaytag to the property
                property.FindPropertyRelative("hashCode").stringValue = gTag.HashCode;
                fullTagProperty.stringValue = gTag.FullTag;
                
                fullTagProperty.serializedObject.ApplyModifiedProperties();
            });
            treeWindow.ShowToggle = false;
            //treeWindow.ContainerProperty = explicitTagsProperty;
            UnityEditor.PopupWindow.Show(controlRect, treeWindow);
        }

        
        EditorGUI.EndProperty();
    }
    private void AddTagToProperty(SerializedProperty property, string tag)
    {
        bool tagExists = false;
        int tagIndex = -1;

        // Check if the tag already exists in the property
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            if (element.stringValue == tag)
            {
                tagExists = true;
                tagIndex = i; // Store the index of the existing tag
                break;
            }
        }

        if (tagExists)
        {
            // Remove the existing tag
            property.DeleteArrayElementAtIndex(tagIndex);
            Debug.Log($"Tag '{tag}' removed from the list.");
        }
        else
        {
            // Add the tag if it doesn't exist
            property.arraySize++;
            SerializedProperty newElement = property.GetArrayElementAtIndex(property.arraySize - 1);
            newElement.stringValue = tag;
            Debug.Log($"Tag '{tag}' added to the list.");
        }

        // Apply changes to the serialized object
        property.serializedObject.ApplyModifiedProperties();
    }
    private void TagSelectorPopup(Rect rect, SerializedProperty fullTagProperty,GameplayTag gameplayTag,SerializedProperty rootProperty)
    {
        GameplayTagsAsset gameplayTagsAsset = Resources.Load<GameplayTagsAsset>("GameplayTagList");
        if (gameplayTagsAsset != null)
        {
           // List<string> dynamicTags = gameplayTagsAsset.tags;
            TagSelectorWindow.ShowWindow(rect, fullTagProperty, gameplayTag,gameplayTagsAsset,rootProperty);
        }
        else
        {
            Debug.LogError("Failed to load GameplayTags asset!");
        }
    }

    private void DropdownMenuOnOnValueSelected(string obj)
    {
        DDebug.Log(obj);
    }

    private void SetValue(string selectedType, SerializedProperty fullTagProperty)
    {
        fullTagProperty.stringValue = selectedType;
        fullTagProperty.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
}
