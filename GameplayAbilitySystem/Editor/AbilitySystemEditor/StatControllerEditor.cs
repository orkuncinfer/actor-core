using System.Collections.Generic;
using StatSystem;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[CustomEditor(typeof(StatController), true)]
public class StatControllerEditor : Editor
{
    private Texture2D icon;
    public VisualTreeAsset Uxml;
    private MultiColumnListView _statsListView;
    private MultiColumnListView _primaryStatsListView;
    private MultiColumnListView _attributesListView;
    private MultiColumnListView _currentListView;

    private Color _buttonDefaultColor;
    private List<Button> _buttons = new List<Button>();
    private UnityEditor.UIElements.ObjectField statField;
    
    // Track active fields to prevent callback conflicts
    private readonly HashSet<FloatField> _activeFields = new HashSet<FloatField>();
    
    private void OnEnable()
    {
        icon = Resources.Load<Texture2D>("stats-icon");
        EditorGUIUtility.SetIconForObject(target, icon);
        EditorApplication.update += MyUpdateFunction;
    }

    private void MyUpdateFunction()
    {
        if(_currentListView != null)
            _currentListView.RefreshItems();
    }

    private void OnDisable()
    {
        EditorApplication.update -= MyUpdateFunction;
        
        // Clean up all active fields
        foreach (var field in _activeFields)
        {
            if (field != null)
            {
                field.UnregisterCallback<ChangeEvent<float>>(OnAnyValueChanged);
            }
        }
        _activeFields.Clear();
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        StatController controller = (StatController)target;
        Texture2D greenDot = Resources.Load<Texture2D>("green-dot");
        Texture2D redDot = Resources.Load<Texture2D>("red-dot");
        
        if (Uxml == null)
        {
            Uxml = Resources.Load<VisualTreeAsset>("StatController");
        
            if (Uxml == null)
            {
                Debug.LogError($"UXML file not found for {nameof(StatControllerEditor)}. Please assign the UXML file or place it in Resources folder.");
                return root;
            }
        }
        
        Uxml.CloneTree(root);
        IMGUIContainer initIcon = root.Q<IMGUIContainer>("iconview");
        statField = root.Q<ObjectField>("data-field");
        statField.value = controller.DataBase;
        statField.RegisterValueChangedCallback(evt => controller.DataBase = evt.newValue as StatDataBase);
      
        _statsListView = root.Q<MultiColumnListView>("stats");
        _primaryStatsListView = root.Q<MultiColumnListView>("primary-stats");
        _attributesListView = root.Q<MultiColumnListView>("attributes");

        SetupToolbarButtons(root, controller);
        SetupInitializationIndicator(controller, initIcon, greenDot, redDot);
        
        // Setup list views with conditional editability
        SetupStatsListView(controller);
        SetupPrimaryStatsListView(controller);
        SetupAttributesListView(controller);
        
        SetupInitialViewState(controller);
        
        return root;
    }

    private void SetupToolbarButtons(VisualElement root, StatController controller)
    {
        Toolbar toolbarTabs = root.Q<Toolbar>("toolbar-tabs");
        Button tb_primarystats = root.Q<ToolbarButton>("tb-primarystats");
        Button tb_stats = root.Q<ToolbarButton>("tb-stats");
        Button tb_attributes = root.Q<ToolbarButton>("tb-attributes");
        
        _buttons.AddRange(new[] { tb_primarystats, tb_stats, tb_attributes });
        
        tb_primarystats.clicked += () => SwitchToListView(_primaryStatsListView, tb_primarystats);
        tb_stats.clicked += () => SwitchToListView(_statsListView, tb_stats);
        tb_attributes.clicked += () => SwitchToListView(_attributesListView, tb_attributes);
    }

    private void SetupInitializationIndicator(StatController controller, IMGUIContainer initIcon, Texture2D greenDot, Texture2D redDot)
    {
        initIcon.style.backgroundImage = controller.IsInitialized ? greenDot : redDot;
    }

    private void SwitchToListView(MultiColumnListView targetListView, Button selectedButton)
    {
        if (_currentListView != null)
            _currentListView.style.display = DisplayStyle.None;
            
        targetListView.style.display = DisplayStyle.Flex;
        _currentListView = targetListView;
        SetButtonColorSelected(selectedButton);
    }

    private void SetupStatsListView(StatController controller)
    {
        if (controller.StatList == null || controller.StatList.Count == 0) return;

        _statsListView.itemsSource = controller.StatList;
        var cols = _statsListView.columns;
        
        cols["name"].makeCell = () => CreateNameLabel();
        cols["starting-health"].makeCell = () => CreateValueCellForStatType(); // Always label for regular Stats
        cols["current-health"].makeCell = () => CreateCapLabel();

        cols["name"].bindCell = (element, index) => 
        {
            var label = element as Label;
            label.text = controller.StatList[index].Definition.name;
        };
        
        cols["starting-health"].bindCell = (element, index) => 
        {
            var stat = controller.StatList[index];
            
            // Regular Stats are always read-only labels
            var label = element as Label;
            label.text = stat.Value.ToString();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
        };
        
        cols["current-health"].bindCell = (element, index) => 
        {
            var label = element as Label;
            var cap = controller.StatList[index].Definition.Cap;
            label.text = cap == -1 ? "Infinite" : cap.ToString();
        };

        cols["name"].unbindCell = (element, index) => element.Unbind();
        cols["starting-health"].unbindCell = (element, index) => element.Unbind();
        cols["current-health"].unbindCell = (element, index) => element.Unbind();
    }

    private void SetupPrimaryStatsListView(StatController controller)
    {
        if (controller.PrimaryStatList == null || controller.PrimaryStatList.Count == 0) return;

        _primaryStatsListView.itemsSource = controller.PrimaryStatList;
        var cols = _primaryStatsListView.columns;
        
        cols["name"].makeCell = () => CreateNameLabel();
        cols["starting-health"].makeCell = () => CreateEditableField(); // Editable for PrimaryStats
        cols["current-health"].makeCell = () => CreateCapLabel();

        cols["name"].bindCell = (element, index) => 
        {
            var label = element as Label;
            label.text = controller.PrimaryStatList[index].Definition.name;
        };
        
        cols["starting-health"].bindCell = (element, index) => 
        {
            var field = element as FloatField;
            var stat = controller.PrimaryStatList[index];
            
            // PrimaryStats are editable
            field.SetValueWithoutNotify(stat.Value);
            field.userData = new StatFieldData 
            { 
                StatReference = stat, 
                StatType = StatType.PrimaryStat, 
                Index = index 
            };
            
            _activeFields.Add(field);
            field.RegisterCallback<ChangeEvent<float>>(OnAnyValueChanged);
        };
        
        cols["current-health"].bindCell = (element, index) => 
        {
            var label = element as Label;
            var cap = controller.PrimaryStatList[index].Definition.Cap;
            label.text = cap == -1 ? "Infinite" : cap.ToString();
        };

        cols["name"].unbindCell = (element, index) => element.Unbind();
        cols["starting-health"].unbindCell = UnbindField;
        cols["current-health"].unbindCell = (element, index) => element.Unbind();
    }

    private void SetupAttributesListView(StatController controller)
    {
        if (controller.AttributeList == null || controller.AttributeList.Count == 0) return;

        _attributesListView.itemsSource = controller.AttributeList;
        var cols = _attributesListView.columns;
        
        cols["name"].makeCell = () => CreateNameLabel();
        cols["starting-health"].makeCell = () => CreateEditableField(); // Editable for Attributes
        cols["current-health"].makeCell = () => CreateCapLabel();

        cols["name"].bindCell = (element, index) => 
        {
            var label = element as Label;
            label.text = controller.AttributeList[index].Definition.name;
        };
        
        cols["starting-health"].bindCell = (element, index) => 
        {
            var field = element as FloatField;
            var stat = controller.AttributeList[index];
            
            // Attributes are editable
            field.SetValueWithoutNotify(stat.CurrentValue);
            field.userData = new StatFieldData 
            { 
                StatReference = stat, 
                StatType = StatType.Attribute, 
                Index = index 
            };
            
            _activeFields.Add(field);
            field.RegisterCallback<ChangeEvent<float>>(OnAnyValueChanged);
        };
        
        cols["current-health"].bindCell = (element, index) => 
        {
            var label = element as Label;
            var cap = controller.AttributeList[index].Definition.Cap;
            label.text = cap == -1 ? "Infinite" : cap.ToString();
        };

        cols["name"].unbindCell = (element, index) => element.Unbind();
        cols["starting-health"].unbindCell = UnbindField;
        cols["current-health"].unbindCell = (element, index) => element.Unbind();
    }

    private Label CreateNameLabel()
    {
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.alignSelf = Align.Center;
        return label;
    }

    private VisualElement CreateValueCellForStatType()
    {
        // For regular Stats, return a read-only label
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        return label;
    }

    private FloatField CreateEditableField()
    {
        // For PrimaryStats and Attributes, return an editable field
        var field = new FloatField();
        field.style.unityTextAlign = TextAnchor.MiddleCenter;
        field.style.marginLeft = 2;
        field.style.marginRight = 2;
        return field;
    }

    private Label CreateCapLabel()
    {
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        return label;
    }

    private void UnbindField(VisualElement element, int index)
    {
        var field = element as FloatField;
        if (field != null)
        {
            _activeFields.Remove(field);
            field.UnregisterCallback<ChangeEvent<float>>(OnAnyValueChanged);
            field.userData = null;
        }
        element.Unbind();
    }

    private void OnAnyValueChanged(ChangeEvent<float> evt)
    {
        var field = evt.target as FloatField;
        if (field?.userData is StatFieldData data)
        {
            float newValue = evt.newValue;
            var stat = data.StatReference;
            
            // Validate against cap
            int cap = stat.Definition.Cap;
            if (cap != -1 && newValue > cap)
            {
                newValue = cap;
                field.SetValueWithoutNotify(newValue);
            }
            
            // Prevent negative values (optional)
            if (newValue < 0)
            {
                newValue = 0;
                field.SetValueWithoutNotify(newValue);
            }

            // Apply the value based on stat type - only for editable types
            if (stat is PrimaryStat primaryStat)
            {
                primaryStat.SetValue((int)newValue);
            }
            else if (stat is Attribute attribute)
            {
                attribute.SetValue((int)newValue);
            }
            // Note: Regular Stat types are not handled here since they're not editable

            EditorUtility.SetDirty(target);
        }
    }

    private void SetupInitialViewState(StatController controller)
    {
        if (!controller.IsInitialized)
        {
            HideAllListViews();
            return;
        }

        _currentListView = _primaryStatsListView;
        ShowOnlyCurrentListView();
        SetButtonColorSelected(_buttons[0]);
    }

    private void ShowOnlyCurrentListView()
    {
        _primaryStatsListView.style.display = _currentListView == _primaryStatsListView ? DisplayStyle.Flex : DisplayStyle.None;
        _statsListView.style.display = _currentListView == _statsListView ? DisplayStyle.Flex : DisplayStyle.None;
        _attributesListView.style.display = _currentListView == _attributesListView ? DisplayStyle.Flex : DisplayStyle.None;
        
        var toolbar = _currentListView?.parent?.Q<Toolbar>("toolbar-tabs");
        if (toolbar != null) toolbar.style.display = DisplayStyle.Flex;
    }

    private void HideAllListViews()
    {
        _primaryStatsListView.style.display = DisplayStyle.None;
        _statsListView.style.display = DisplayStyle.None;
        _attributesListView.style.display = DisplayStyle.None;
        
        var toolbar = _primaryStatsListView?.parent?.Q<Toolbar>("toolbar-tabs");
        if (toolbar != null) toolbar.style.display = DisplayStyle.None;
    }
    
    private void SetButtonColorSelected(Button selected)
    {
        foreach (Button btn in _buttons)
        {
            btn.style.backgroundColor = new StyleColor(new Color(0.23f, 0.23f, 0.23f, 1));
        }
        selected.style.backgroundColor = new StyleColor(new Color(0.43f, 0.42f, 0.42f, 1));
    }

    private enum StatType
    {
        Stat = 0,
        PrimaryStat = 1,
        Attribute = 2
    }

    private class StatFieldData
    {
        public dynamic StatReference;
        public StatType StatType;
        public int Index;
    }
}
#endif