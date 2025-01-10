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
            TagSelectorPopup(controlRect, fullTagProperty,gameplayTag,rootProperty);
        }

        EditorGUI.EndProperty();
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
