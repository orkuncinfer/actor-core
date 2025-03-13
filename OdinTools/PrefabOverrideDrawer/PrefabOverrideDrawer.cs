#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[DrawerPriority(0, 10000, 0)]
public sealed class PrefabOverrideDrawer<T> : OdinValueDrawer<T>
{
    private static readonly Color OVERRIDE_MARGIN_COLOR = new Color(0.003921569f, 0.6f, 0.9215686f, 0.75f);

    protected override bool CanDrawValueProperty(InspectorProperty property)
    {
#if ODIN_INSPECTOR_3
        return !property.IsTreeRoot && property.SupportsPrefabModifications && property.State.Enabled &&
               property.State.Visible && property.Tree.PrefabModificationHandler.HasPrefabs &&
               GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModifiedValueBar;
#else
        return base.CanDrawValueProperty(property) && property.SupportsPrefabModifications &&
               property.Tree.PrefabModificationHandler.HasPrefabs;
#endif
    }
    
    /// <summary>
    /// Draws the property.
    /// </summary>
    protected override void DrawPropertyLayout(GUIContent label)
    {
        Event e = Event.current;
        bool draw = e.type == EventType.Repaint;
       
        bool overridden = false;
        bool childOrCollectionOverridden = false;
        GUIHelper.BeginLayoutMeasuring();
        if (draw)
        {
            var valueOverridden = Property.ValueEntry != null && Property.ValueEntry.ValueChangedFromPrefab;
            var childValueOverridden = false;
            var collectionChanged = false;

            if (!valueOverridden)
            {
                collectionChanged = Property.ValueEntry != null && (Property.ValueEntry.ListLengthChangedFromPrefab ||
                                                                    Property.ValueEntry.DictionaryChangedFromPrefab);

                if (!collectionChanged)
                    childValueOverridden = ChildValueOverridden(Property);
            }

            overridden = childOrCollectionOverridden = childValueOverridden || collectionChanged;
            //Debug.Log($"{overridden} value for {Property.Name}");
#if !ODIN_INSPECTOR_3
            overridden = overridden || valueOverridden;
#endif
            
        }
       
        if (draw && childOrCollectionOverridden)
            GUIHelper.PushIsBoldLabel(true);

        CallNextDrawer(label);

        if (draw && childOrCollectionOverridden)
            GUIHelper.PopIsBoldLabel();

        Rect rect = default;
        rect = GUIHelper.EndLayoutMeasuring();
     
       
        if ((!draw || !overridden) && e.type != EventType.MouseDown)
            return;
        
        

        var partOfCollection = Property.Parent != null && Property.Parent.ChildResolver is ICollectionResolver;

        if (partOfCollection)
            rect = GUIHelper.GetCurrentLayoutRect();
        
        GUIHelper.IndentRect(ref rect);
      
        if (!partOfCollection && childOrCollectionOverridden)
            rect.height = EditorGUIUtility.singleLineHeight;

        bool IsRootSerializeReference = false;
        
        if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
        {
            if (ChildValueOverridden(Property) && IsInsideSerializeReference(Property))
            {
                e.Use();
                ShowContextMenu(Property);
            }
        }
        
        GUIHelper.PushGUIEnabled(true);
        
        rect.width = 2;
        rect.x -= 2;

        SirenixEditorGUI.DrawSolidRect(rect, OVERRIDE_MARGIN_COLOR);
        

        GUIHelper.PopGUIEnabled();
       
    }
    private bool IsInsideSerializeReference(InspectorProperty property)
    {
        while (property.Parent != null)
        {
            if (property.Info?.TypeOfValue != null)
            {
                var serializedObject = property.Tree.UnitySerializedObject;
                var serializedProperty = serializedObject.FindProperty(property.Path);
                if (serializedProperty != null && serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    return true;
                }
            }
            property = property.Parent; // Move up the hierarchy
        }
        return false;
    }

    private static void ShowContextMenu(InspectorProperty property)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Apply to Prefab"), false, () => ApplyToPrefab(property));
        menu.AddItem(new GUIContent("Revert"), false, () => RevertToPrefab(property));
        menu.ShowAsContext();
    }
    private static void ApplyToPrefab(InspectorProperty property)
    {
        var targetObject = property.Tree.UnitySerializedObject.targetObject;
        var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(targetObject);
        
        var serializedObject = property.Tree.UnitySerializedObject;
        var propertyPath = property.Path;
        var serializedProperty = serializedObject.FindProperty(propertyPath);

        if (prefabSource == null)
        {
            Debug.LogWarning("No prefab found to apply changes.");
            return;
        }

        PrefabUtility.ApplyPropertyOverride(serializedProperty, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetObject), InteractionMode.UserAction);
        Debug.Log($"Applied {property.Name} to prefab.");
    }

    private static void RevertToPrefab(InspectorProperty property)
    {
        var serializedObject = property.Tree.UnitySerializedObject;
        var propertyPath = property.Path;
        /*if (property.Parent.IsTreeRoot == false)
            propertyPath = property.Parent.Path;
        */
        var serializedProperty = serializedObject.FindProperty(propertyPath);
        
        PrefabUtility.RevertPropertyOverride(serializedProperty, InteractionMode.UserAction);
        Debug.Log($"Reverted {property.Name} to prefab value.");
    }
    private static bool ChildValueOverridden(InspectorProperty property)
    {
        var children = property.Children;
        var count = children.Count;
        

        for (var index = 0; index < count; index++)
        {
            var child = children[index];
            var valueEntry = child.ValueEntry;

            if (valueEntry != null && (valueEntry.ValueChangedFromPrefab ||
                                       valueEntry.ListLengthChangedFromPrefab ||
                                       valueEntry.DictionaryChangedFromPrefab))
            {
                return true;
            }

            if (ChildValueOverridden(child))
                return true;
        }


        // Additional check: If this property itself is a SerializeReference, scan manually
        if (property.Info != null)
        {
            if (IsSerializeReferenceOverridden(property))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSerializeReferenceOverridden(InspectorProperty property)
    {
        var serializedObject = property.Tree.UnitySerializedObject;
        var propertyPath = property.Path;
        var serializedProperty = serializedObject.FindProperty(propertyPath);

        if (serializedProperty == null)
            return false;

        if (serializedProperty.prefabOverride)
        {
            return true;
        }

        // Check child properties manually
        var childProperty = serializedProperty.Copy();

        if (childProperty.prefabOverride) return true;


        return false;
    }

    private static object GetOriginalPrefabValue(InspectorProperty property)
    {
        var targetObject = property.Tree.UnitySerializedObject.targetObject;

        // Get the corresponding object in the prefab
        var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(targetObject);
        if (prefabSource == null)
            return null;

        // Create a SerializedObject for the prefab source
        using (var prefabSerializedObject = new SerializedObject(prefabSource))
        {
            var prefabProperty = prefabSerializedObject.FindProperty(property.Path);
            if (prefabProperty == null)
                return null;

            // Get the original value based on the type
            return GetSerializedPropertyValue(prefabProperty);
        }
    }

    private static object GetSerializedPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue;
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.Float:
                return property.floatValue;
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return property.colorValue;
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue;
            case SerializedPropertyType.LayerMask:
                return property.intValue;
            case SerializedPropertyType.Enum:
                return property.enumValueIndex;
            case SerializedPropertyType.Vector2:
                return property.vector2Value;
            case SerializedPropertyType.Vector3:
                return property.vector3Value;
            case SerializedPropertyType.Vector4:
                return property.vector4Value;
            case SerializedPropertyType.Rect:
                return property.rectValue;
            case SerializedPropertyType.ArraySize:
                return property.arraySize;
            case SerializedPropertyType.Character:
                return (char)property.intValue;
            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue;
            case SerializedPropertyType.Bounds:
                return property.boundsValue;
            case SerializedPropertyType.Quaternion:
                return property.quaternionValue;
            case SerializedPropertyType.ExposedReference:
                return property.exposedReferenceValue;
            case SerializedPropertyType.FixedBufferSize:
                return property.fixedBufferSize;
            case SerializedPropertyType.Vector2Int:
                return property.vector2IntValue;
            case SerializedPropertyType.Vector3Int:
                return property.vector3IntValue;
            case SerializedPropertyType.RectInt:
                return property.rectIntValue;
            case SerializedPropertyType.BoundsInt:
                return property.boundsIntValue;
            default:
                return null;
        }

        if (property.boxedValue != null) return property.boxedValue;
        if (property.managedReferenceValue != null) return property.managedReferenceValue;
    }
}
#endif