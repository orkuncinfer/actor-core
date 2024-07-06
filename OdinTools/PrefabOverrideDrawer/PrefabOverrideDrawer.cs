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
        bool draw = Event.current.type == EventType.Repaint;

        bool overridden = false;
        bool childOrCollectionOverridden = false;

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

            if (overridden)
                GUIHelper.BeginLayoutMeasuring();
        }

        if (draw && childOrCollectionOverridden)
            GUIHelper.PushIsBoldLabel(true);

        CallNextDrawer(label);

        if (draw && childOrCollectionOverridden)
            GUIHelper.PopIsBoldLabel();

        if (!draw || !overridden)
            return;

        var rect = GUIHelper.EndLayoutMeasuring();

        var partOfCollection = Property.Parent != null && Property.Parent.ChildResolver is ICollectionResolver;

        if (partOfCollection)
            rect = GUIHelper.GetCurrentLayoutRect();

        GUIHelper.IndentRect(ref rect);

        GUIHelper.PushGUIEnabled(true);

        if (!partOfCollection && childOrCollectionOverridden)
            rect.height = EditorGUIUtility.singleLineHeight;

        rect.width = 2;
        rect.x -= 2;

        SirenixEditorGUI.DrawSolidRect(rect, OVERRIDE_MARGIN_COLOR);

        GUIHelper.PopGUIEnabled();
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
                                       valueEntry.DictionaryChangedFromPrefab ||
                                       IsSerializeReferenceValueOverridden(valueEntry)))
            {
                return true;
            }

            if (ChildValueOverridden(child))
                return true;
        }

        return false;
    }

    private static bool IsSerializeReferenceValueOverridden(IPropertyValueEntry valueEntry)
    {
        if (valueEntry.Property.Info.TypeOfValue.IsDefined(typeof(SerializeReference), true))
        {
            // Custom logic for SerializeReference properties
            // You may need to use SerializedObject and SerializedProperty to check the state of the reference
            var serializedObject = valueEntry.Property.Tree.UnitySerializedObject;
            var propertyPath = valueEntry.Property.Path;
            var serializedProperty = serializedObject.FindProperty(propertyPath);

            if (serializedProperty != null && serializedProperty.prefabOverride)
            {
                return true;
            }
        }

        return false;
    }
}
