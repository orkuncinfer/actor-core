using System.Collections;
using System.Collections.Generic;
using StatSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/*
[CustomEditor(typeof(AbilityController),true)]
public class AbilityControllerEditor : Editor
{
#if UNITY_EDITOR
    

    private Texture2D icon;
    public VisualTreeAsset Uxml;
    public StatController[] ctrl;

    private PropertyField _statField;
    private SerializedProperty _statProperty;

    private UnityEditor.UIElements.ObjectField statField;
    
    private void OnEnable()
    {
        icon = Resources.Load<Texture2D>("ability-icon");
        EditorGUIUtility.SetIconForObject(target, icon);
        EditorApplication.update += MyUpdateFunction;
    }
    private void MyUpdateFunction()
    {
       
    }
    private void OnDisable()
    {
        EditorApplication.update -= MyUpdateFunction;
    }
    
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        AbilityController controller = (AbilityController)target;
        Texture2D greenDot = Resources.Load<Texture2D>("green-dot");
        Texture2D redDot = Resources.Load<Texture2D>("red-dot");
        if (greenDot == null)
        {
            
        }
        
        Uxml.CloneTree(root);
        IMGUIContainer initIcon = root.Q<IMGUIContainer>("iconview");
        Button primaryStatsTab = root.Q<Button>("test-button");
        primaryStatsTab.clicked += () =>
        {
            TestAbility();
        };
        
        ListView listView = root.Q<ListView>();
        listView.selectionChanged += ListViewOnselectionChanged;
       // statField = root.Q<ObjectField>("target-field");
        //statField.value = controller.DataBase;
        //statField.RegisterValueChangedCallback(evt => controller.DataBase = evt.newValue as StatDataBase);
       
        if (controller.IsInitialized)
        {
            initIcon.style.backgroundImage = greenDot;
        }
        else
        {
            initIcon.style.backgroundImage = redDot;
        }
        return root;
    }

    private void ListViewOnselectionChanged(IEnumerable<object> obj)
    {
        //todo implement
        Debug.Log("Chosen" + obj.ToString());
        foreach (var element in obj)
        {
            var selectedAbility = obj as AbilityDefinition;
            if (selectedAbility is AbilityDefinition def)
            {
                Debug.Log("Selected: " + def.name);
            }
            if (selectedAbility != null)
            {
                Debug.Log("Selected: " + selectedAbility.name);
            }
        }
    }


    private void TestAbility()
    {
        AbilityController controller = (AbilityController)target;
        controller.TryActiveAbilityWithDefinition(controller.TestAbility);
    }
}
#endif*/
