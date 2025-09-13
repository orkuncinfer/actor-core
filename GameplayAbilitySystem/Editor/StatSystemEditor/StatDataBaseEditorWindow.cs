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
    public StyleSheet StyleSheet;
    public VisualTreeAsset StatDataBaseEditorWindowUxml;
    
    private static StatDataBase _database;
    private StatCollectionEditor _currentCollectionEditor;
    private List<Button> _buttons = new List<Button>();
    private Color _buttonDefaultColor;
    
    // Track if we need to reinitialize the GUI
    private static bool _needsReinitialize = false;
    
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
        if (EditorUtility.InstanceIDToObject(instanceId) is StatDataBase statData)
        {
            _database = statData;
            _needsReinitialize = true;
            ShowWindow();
            return true;
        }

        return false;
    }

    private void OnSelectionChange()
    {
        // Only update database if the selection is actually a StatDataBase asset
        if (Selection.activeObject is StatDataBase selectedDatabase)
        {
            if (_database != selectedDatabase)
            {
                _database = selectedDatabase;
                _needsReinitialize = true;
                
                // Refresh the GUI with the new database
                if (rootVisualElement.childCount > 0)
                {
                    CreateGUI();
                }
            }
        }
        // Don't set _database to null when selecting other objects
    }

    private void OnFocus()
    {
        // Only update database if we have a valid StatDataBase selected
        if (Selection.activeObject is StatDataBase selectedDatabase && selectedDatabase != _database)
        {
            _database = selectedDatabase;
            _needsReinitialize = true;
            CreateGUI();
        }
    }

    private void Update()
    {
        // Handle deferred reinitialization
        if (_needsReinitialize && _database != null)
        {
            _needsReinitialize = false;
            CreateGUI();
        }
    }

    public void CreateGUI()
    {
        // Clear existing content
        rootVisualElement.Clear();
        _buttons.Clear();
        _currentCollectionEditor = null;
        
        // Validate database before proceeding
        if (_database == null)
        {
            CreateDatabaseSelectionPrompt();
            return;
        }
        
        VisualElement root = rootVisualElement;
        
        var visualTree = StatDataBaseEditorWindowUxml;
        if (visualTree == null)
        {
            Debug.LogError("StatDataBaseEditorWindowUxml is not assigned");
            return;
        }
        
        visualTree.CloneTree(root);
        
        if (StyleSheet != null)
        {
            root.styleSheets.Add(StyleSheet);
        }
        
        SetupCollectionEditors(root);
    }

    private void CreateDatabaseSelectionPrompt()
    {
        VisualElement root = rootVisualElement;
        
        var container = new VisualElement();
        container.style.flexGrow = 1;
        container.style.alignItems = Align.Center;
        container.style.justifyContent = Justify.Center;
        
        var label = new Label("No StatDataBase selected.\nSelect a StatDataBase asset in the Project window or assign one below.");
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.fontSize = 14;
        label.style.marginBottom = 20;
        
        var objectField = new ObjectField("StatDataBase");
        objectField.objectType = typeof(StatDataBase);
        objectField.value = _database;
        objectField.RegisterValueChangedCallback(evt =>
        {
            _database = evt.newValue as StatDataBase;
            if (_database != null)
            {
                CreateGUI();
            }
        });
        
        container.Add(label);
        container.Add(objectField);
        root.Add(container);
    }

    private void SetupCollectionEditors(VisualElement root)
    {
        try
        {
            // Setup Stats collection
            StatCollectionEditor stats = root.Q<StatCollectionEditor>("stats");
            if (stats != null)
            {
                stats.Initialize(_database, _database.Stats);
                SetupTabButton(root, "stats-tab", stats, true); // Default selection
            }
            
            // Setup Primary Stats collection
            StatCollectionEditor primaryStats = root.Q<StatCollectionEditor>("primary-stats");
            if (primaryStats != null)
            {
                primaryStats.Initialize(_database, _database.PrimaryStats);
                SetupTabButton(root, "primary-stats-tab", primaryStats, false);
            }
            
            // Setup Attributes collection
            StatCollectionEditor attributes = root.Q<StatCollectionEditor>("attributes");
            if (attributes != null)
            {
                attributes.Initialize(_database, _database.Attributes);
                SetupTabButton(root, "attributes-tab", attributes, false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error setting up collection editors: {ex.Message}");
        }
    }

    private void SetupTabButton(VisualElement root, string buttonName, StatCollectionEditor editor, bool isDefault)
    {
        Button tabButton = root.Q<Button>(buttonName);
        if (tabButton == null)
        {
            Debug.LogWarning($"Tab button '{buttonName}' not found in UXML");
            return;
        }
        
        _buttons.Add(tabButton);
        
        if (_buttons.Count == 1) // First button sets the default color
        {
            _buttonDefaultColor = tabButton.style.backgroundColor.value;
        }
        
        tabButton.clicked += () =>
        {
            SetButtonColorSelected(tabButton);
            
            if (_currentCollectionEditor != null)
            {
                _currentCollectionEditor.style.display = DisplayStyle.None;
            }
            
            editor.style.display = DisplayStyle.Flex;
            _currentCollectionEditor = editor;
        };
        
        if (isDefault)
        {
            _currentCollectionEditor = editor;
            SetButtonColorSelected(tabButton);
        }
        else
        {
            editor.style.display = DisplayStyle.None;
        }
    }

    private void SetButtonColorSelected(Button selected)
    {
        foreach (Button btn in _buttons)
        {
            btn.style.backgroundColor = _buttonDefaultColor;
        }
        selected.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
    }
}