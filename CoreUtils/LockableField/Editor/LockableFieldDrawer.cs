using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LockableFieldAttribute))]
public class LockableFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LockableFieldAttribute lockableFieldAttribute = (LockableFieldAttribute)attribute;

        // Find the boolean field that stores the lock state
        SerializedProperty lockStateProperty = property.serializedObject.FindProperty(lockableFieldAttribute.LockStateFieldName);
        if (lockStateProperty == null || lockStateProperty.propertyType != SerializedPropertyType.Boolean)
        {
            EditorGUI.LabelField(position, label.text, "Lock state field not found for :" + lockableFieldAttribute.LockStateFieldName);
            return;
        }

        // Retrieve the lock state
        bool isLocked = lockStateProperty.boolValue;

        // Calculate the positions of the lock button and property field
        Rect lockButtonRect = new Rect(position.x + position.width - 20, position.y, 20, position.height);
        Rect propertyRect = new Rect(position.x, position.y, position.width - 25, position.height);

        // Create a GUIStyle for the lock button
        GUIStyle lockButtonStyle = new GUIStyle(EditorStyles.miniButton);
        lockButtonStyle.padding = new RectOffset(2, 2, 2, 2);
        lockButtonStyle.alignment = TextAnchor.MiddleCenter;

        // Draw the lock button
        bool newLockState = GUI.Toggle(lockButtonRect, isLocked, GUIContent.none, lockButtonStyle);
        if (newLockState != isLocked)
        {
            lockStateProperty.boolValue = newLockState;
            property.serializedObject.ApplyModifiedProperties();
        }

        GUIContent lockIcon = new GUIContent(EditorGUIUtility.IconContent(isLocked ? "LockIcon-On" : "LockIcon").image);
        GUI.Label(lockButtonRect, lockIcon, lockButtonStyle);

        // Draw the property field as read-only if locked
        EditorGUI.BeginDisabledGroup(isLocked);
        EditorGUI.PropertyField(propertyRect, property, label);
        EditorGUI.EndDisabledGroup();
    }
}
