using System;
using System.Collections.Generic;
using StatSystem;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class StatDataBaseEditorWindow : EditorWindow
{
    private static StatDataBase _database;
    private StatCollectionEditor _currentCollectionEditor;
    private List<Button> _buttons = new List<Button>();
    private Color _buttonDefaultColor;
    
    [MenuItem("Window/StatSystem/StatDatabase")]
    public static void ShowWindow()
    {
        StatDataBaseEditorWindow window = GetWindow<StatDataBaseEditorWindow>();
        window.minSize = new Vector2(800, 600);
        window.titleContent = new GUIContent("StatDatabase");
    }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (EditorUtility.InstanceIDToObject(instanceId) is StatDataBase _statData)
        {
            _database = _statData;
            ShowWindow();
            return true;
        }

        return false;
    }

    private void OnSelectionChange()
    {
        _database = Selection.activeObject as StatDataBase;
    }

    private void OnFocus()
    {
        if (Selection.activeObject is StatDataBase)
        {
            _database = Selection.activeObject as StatDataBase;
        }
    }

    public void CreateGUI()
    {
        OnSelectionChange();
        
        if (_database == null)
        {
            _database = Selection.activeObject as StatDataBase;
            Debug.LogWarning("Database is Null");
            return;
        }
        
        VisualElement root = rootVisualElement;
        
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/actor-core/_StatSystem/StatSystem/Editor/StatDataBaseEditorWindow.uxml");
        visualTree.CloneTree(root);
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/actor-core/_StatSystem/StatSystem/Editor/StatDataBaseEditorWindow.uss");
        root.styleSheets.Add(styleSheet);
        
        StatCollectionEditor stats = root.Q<StatCollectionEditor>("stats");
        stats.Initialize(_database,_database.Stats);
        Button statsTab = root.Q<Button>("stats-tab");
        _buttons.Add(statsTab);
        _buttonDefaultColor = statsTab.style.backgroundColor.value;
        statsTab.clicked += () =>
        {
            SetButtonColorSelected(statsTab);
            _currentCollectionEditor.style.display = DisplayStyle.None;
            stats.style.display = DisplayStyle.Flex;
            _currentCollectionEditor = stats;
        };
        
        StatCollectionEditor primaryStats = root.Q<StatCollectionEditor>("primary-stats");
        primaryStats.Initialize(_database,_database.PrimaryStats);
        Button primaryStatsTab = root.Q<Button>("primary-stats-tab");
        _buttons.Add(primaryStatsTab);
        primaryStatsTab.clicked += () =>
        {
            SetButtonColorSelected(primaryStatsTab);
            _currentCollectionEditor.style.display = DisplayStyle.None;
            primaryStats.style.display = DisplayStyle.Flex;
            _currentCollectionEditor = primaryStats;
        };
        
        StatCollectionEditor attributes = root.Q<StatCollectionEditor>("attributes");
        attributes.Initialize(_database,_database.Attributes);
        Button attributesTab = root.Q<Button>("attributes-tab");
        _buttons.Add(attributesTab);
        attributesTab.clicked += () =>
        {
            SetButtonColorSelected(attributesTab);
            _currentCollectionEditor.style.display = DisplayStyle.None;
            attributes.style.display = DisplayStyle.Flex;
            _currentCollectionEditor = attributes;
        };

        _currentCollectionEditor = stats;
        SetButtonColorSelected(statsTab);
    }

    void SetButtonColorSelected(Button selected)
    {
        foreach (Button btn in _buttons)
        {
            btn.style.backgroundColor = _buttonDefaultColor;
        }
        selected.style.backgroundColor = new StyleColor(new Color(0.17f,0.17f,0.17f));
    }
}