using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(GameplayTagContainer2))]
public class GameplayTagContainerPropertyDrawer : PropertyDrawer
{
    private const float k_Gap = 2.0f;
      private const float k_ButtonsWidth = 110f;

      private static GUIContent s_TempContent = new();
      private static readonly GUIContent s_RemoveTagContent = new("-", "Remove tag");
      private static GUIContent s_EditTagsContent;

      public GameplayTagContainerPropertyDrawer()
      {
         s_EditTagsContent = new GUIContent("Edit Tags...", "Edit tags in a popup window.");
      }

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         SerializedProperty tagNamesProperty = property.FindPropertyRelative("_tagHashes");
         if (tagNamesProperty.hasMultipleDifferentValues)
         {
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
         }

         if (tagNamesProperty.arraySize > 0)
         {
            return Mathf.Max
            (
               tagNamesProperty.arraySize * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
               (/*EditorGUIUtility.singleLineHeight +*/ EditorGUIUtility.standardVerticalSpacing) * 2
            );
         }

         return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
      }
      private void TagSelectorPopup(Rect rect)
      {
         GameplayTagsAsset gameplayTagsAsset = Resources.Load<GameplayTagsAsset>("GameplayTagList");
         if (gameplayTagsAsset != null)
         {
            // List<string> dynamicTags = gameplayTagsAsset.tags;
            TagSelectorWindow.ShowWindow(rect, null, new GameplayTag(),gameplayTagsAsset,null);
         }
         else
         {
            Debug.LogError("Failed to load GameplayTags asset!");
         }
      }
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         label = EditorGUI.BeginProperty(position, label, property);

         position = EditorGUI.PrefixLabel(position, label);

         int oldIndentLevel = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;

         SerializedProperty explicitTagsProperty = property.FindPropertyRelative("_tagHashes");

         EditorGUI.BeginDisabledGroup(explicitTagsProperty.hasMultipleDifferentValues);

         Rect editButtonRect = position;
         editButtonRect.width = k_ButtonsWidth;
         editButtonRect.height = EditorGUIUtility.singleLineHeight;
         if (GUI.Button(editButtonRect, s_EditTagsContent, EditorStyles.popup))
         {
            /*GameplayTagContainerTreeView tagTreeView = new(new TreeViewState(), explicitTagsProperty);
            Rect activatorRect = editButtonRect;
            activatorRect.width = position.width;
            tagTreeView.ShowPopupWindow(activatorRect, 280f);*/
            //TagSelectorPopup(editButtonRect);
            //open gameplaytagtree window here
            GameplayTagTree treeWindow = new GameplayTagTree(tag =>
            {
               Debug.Log($"Callback received: {tag}");
               AddTagToProperty(explicitTagsProperty, tag);
            });
            treeWindow.ContainerProperty = explicitTagsProperty;
            PopupWindow.Show(editButtonRect, treeWindow);
         }

         EditorGUI.EndDisabledGroup();

         if (explicitTagsProperty.arraySize > 0)
         {
            //DrawClearAllButton(position, explicitTagsProperty);
         }

         if (explicitTagsProperty.hasMultipleDifferentValues)
         {
            OnMultipleValuesGUI(position, explicitTagsProperty);
         }
         else
         {
            OnAddedTagsGUI(position, explicitTagsProperty);
         }

         EditorGUI.indentLevel = oldIndentLevel;
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
      private static void OnMultipleValuesGUI(Rect position, SerializedProperty explicitTagsProperty)
      {
         s_TempContent.text = "Multiple tag values present.";

         Rect rect = position;
         rect.xMin += k_ButtonsWidth + k_Gap;
         rect.height = EditorGUIUtility.singleLineHeight;
         EditorStyles.label.CalcMinMaxWidth(s_TempContent, out _, out float labelWidth);
         rect.width = labelWidth;
         GUI.Label(rect, s_TempContent);
         DrawOutline(rect, new Color(1, 1, 1, 0.15f));
      }

      private static void OnAddedTagsGUI(Rect position, SerializedProperty explicitTagsProperty)
      {
         if (explicitTagsProperty.arraySize <= 0)
         {
            return;
         }

         Rect tagsRect = position;
         tagsRect.xMin += k_ButtonsWidth + k_Gap;
         tagsRect.width = 0;
         tagsRect.height = 0;

         Rect tagRect = tagsRect;
         tagRect.height = EditorGUIUtility.singleLineHeight;
         for (int i = 0; i < explicitTagsProperty.arraySize; i++)
         {
            SerializedProperty element = explicitTagsProperty.GetArrayElementAtIndex(i);
            GameplayTag tag = GameplayTagManger2.RequestTagHash(element.stringValue);

            s_TempContent.text = tag.FullTag;
            s_TempContent.tooltip = tag.Description ?? "No description";
            tagRect.width = EditorStyles.label.CalcSize(s_TempContent).x + 22;
            tagsRect.width = Mathf.Max(tagsRect.width, tagRect.width);
            tagsRect.yMax = Mathf.Max(tagRect.yMax, tagsRect.yMax);

            Rect removeButtonRect = tagRect;
            removeButtonRect.width = 14;
            removeButtonRect.yMax -= 2;
            removeButtonRect.yMin += 2;
            removeButtonRect.x += 2;
            if (GUI.Button(removeButtonRect, s_RemoveTagContent))
            {
               explicitTagsProperty.DeleteArrayElementAtIndex(i);
               Event.current.Use();
            }
            else
            {
               Rect labelRect = tagRect;
               labelRect.xMin = removeButtonRect.xMax;
               GUI.Label(labelRect, s_TempContent);
            }

            tagRect.y = tagRect.yMax;
         }

         DrawOutline(tagsRect, new Color(1, 1, 1, 0.15f));
      }

      private static void DrawClearAllButton(Rect positon, SerializedProperty explicitTagsProperty)
      {
         Rect clearButtonRect = new
         (
            positon.x,
            positon.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
            k_ButtonsWidth,
            EditorGUIUtility.singleLineHeight
         );

         if (GUI.Button(clearButtonRect, "Clear All"))
         {
            explicitTagsProperty.arraySize = 0;
         }
      }

      private static void DrawOutline(Rect rect, Color color, float thickness = 1)
      {
         if (Event.current.type != EventType.Repaint)
         {
            return;
         }

         EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
         EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
         EditorGUI.DrawRect(new Rect(rect.x, rect.y + thickness, thickness, rect.height - 2 * thickness), color);
         EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y + thickness, thickness, rect.height - 2 * thickness), color);
      }
}
