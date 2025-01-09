using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class ExposedVariablesEditorWindow : OdinEditorWindow
{
    private List<ExposedFieldInfo> _exposedMembers = new List<ExposedFieldInfo>();
    private Dictionary<ExposedFieldInfo, object> _instances = new Dictionary<ExposedFieldInfo, object>();
    private Vector2 _scrollPosition;

    [MenuItem("Corex/Exposed Variables")]
    public static void Open()
    {
        GetWindow<ExposedVariablesEditorWindow>().Show();
        ExposedVariablesEditorWindow window = GetWindow<ExposedVariablesEditorWindow>("Exposed Variables");
        window.minSize = new Vector2(400, 1000);
        window.maxSize = window.minSize;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Clear and fetch exposed members
        RefreshExposedMembers();

        // Subscribe to Editor events
        EditorApplication.update += Repaint;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Unsubscribe from Editor events
        EditorApplication.update -= Repaint;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Re-fetch instances and exposed members when entering or exiting play mode
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            RefreshExposedMembers();
        }
    }

    private void RefreshExposedMembers()
    {
        _exposedMembers.Clear();
        _instances.Clear();

        // Find all assemblies
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            // Iterate over all types in the assembly
            System.Type[] types = assembly.GetTypes();
            foreach (System.Type type in types)
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                // Search for fields or properties with the custom attribute
                MemberInfo[] members = type.GetMembers(flags);
                foreach (MemberInfo member in members)
                {
                    ExposedFieldAttribute attribute = member.GetCustomAttribute<ExposedFieldAttribute>();
                    if (attribute != null)
                    {
                        // Try to find an instance of the type in the scene
                        object instance = FindInstanceInScene(type);
                        if (instance != null)
                        {
                            ExposedFieldInfo fieldInfo = new ExposedFieldInfo(member, attribute);
                            _exposedMembers.Add(fieldInfo);
                            _instances.Add(fieldInfo, instance);
                        }
                    }
                }
            }
        }
    }

    private object FindInstanceInScene(System.Type type)
    {
        // Attempt to find an instance of the specified type in the scene
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in allGameObjects)
        {
            Component component = gameObject.GetComponent(type);
            if (component != null)
            {
                return component;
            }
        }
        return null;
    }

    protected override void OnImGUI()
    {
        base.OnImGUI();

        // Add a scroll view to contain all exposed properties
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Exposed Properties", EditorStyles.boldLabel);
        

        foreach (ExposedFieldInfo exposedMember in _exposedMembers)
        {
            // Retrieve the associated instance
            if (_instances.TryGetValue(exposedMember, out object instance))
            {
                // Get the runtime value of the member
                object value = GetMemberValue(exposedMember.MemberInfo, instance);

                // Display the field name and value in the editor
                EditorGUILayout.LabelField(exposedMember.ExposedFieldAttribute.DisplayName, GUILayout.Width(200));
                EditorGUILayout.LabelField(value != null ? value.ToString() : "null", GUILayout.ExpandWidth(true));
            }
        }

        EditorGUILayout.EndVertical();
    }


    private object GetMemberValue(MemberInfo memberInfo, object instance)
    {
        if (memberInfo is FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(instance);
        }
        else if (memberInfo is PropertyInfo propertyInfo)
        {
            return propertyInfo.GetValue(instance);
        }
        return null;
    }
}



public struct ExposedFieldInfo
{
    public MemberInfo MemberInfo;
    public ExposedFieldAttribute ExposedFieldAttribute;
    public ExposedFieldInfo(MemberInfo info, ExposedFieldAttribute attribute)
    {
        MemberInfo = info;
        ExposedFieldAttribute = attribute;
    }
}
