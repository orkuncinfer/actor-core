using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class TagControllerEditor : Editor
{
    private VisualElement root { get; set; }

    private TextField namePropertyField;
    private IntegerField healthPropertyField;
    private SerializedProperty _propertyName;
    private SerializedProperty _propertyHealth;
    
    public override VisualElement CreateInspectorGUI()
    {
        FindProperties();
        InitializeEditor();
        Compose();
        return root;
    }

    private void FindProperties()
    {
        
    }

    private void InitializeEditor()
    {
        root = new VisualElement();
        root.style.flexDirection = FlexDirection.Row;

        namePropertyField = new TextField();
        namePropertyField.BindProperty(_propertyName);
        namePropertyField.style.flexGrow = 1;
        
        
        healthPropertyField = new IntegerField();
        healthPropertyField.BindProperty(_propertyHealth);
        healthPropertyField.style.flexGrow = 1;
    }

    private void Compose()
    {
        root.Add(namePropertyField);
        root.Add(healthPropertyField);
    }
}
