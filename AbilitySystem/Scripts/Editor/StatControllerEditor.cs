using System.Collections.Generic;
using StatSystem;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if UNITY_EDITOR


[CustomEditor(typeof(StatController),true)]
public class StatControllerEditor : Editor
{
    private Texture2D icon;
    public VisualTreeAsset Uxml;
    private MultiColumnListView _statsListView;
    private MultiColumnListView _primaryStatsListView;
    private MultiColumnListView _attributesListView;
    private MultiColumnListView _currentListView;
    public StatController[] ctrl;

    private PropertyField _statField;
    private SerializedProperty _statProperty;
    
    private Color _buttonDefaultColor;
    private List<Button> _buttons = new List<Button>();

    private UnityEditor.UIElements.ObjectField statField;
    
    private void OnEnable()
    {
        icon = Resources.Load<Texture2D>("stats-icon");
        EditorGUIUtility.SetIconForObject(target, icon);
        EditorApplication.update += MyUpdateFunction;
    }

    private void MyUpdateFunction()
    {
        if(_currentListView!= null)
            _currentListView.RefreshItems();

        if (statField == null)
        {
           // Debug.Log("nulll");
        }
    }


    private void OnDisable()
    {
        EditorApplication.update -= MyUpdateFunction;
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        StatController controller = (StatController)target;
        Texture2D greenDot = Resources.Load<Texture2D>("green-dot");
        Texture2D redDot = Resources.Load<Texture2D>("red-dot");
        
        Uxml.CloneTree(root);
        IMGUIContainer initIcon = root.Q<IMGUIContainer>("iconview");
        statField = root.Q<ObjectField>("data-field");
        statField.value = controller.DataBase;
        statField.RegisterValueChangedCallback(evt => controller.DataBase = evt.newValue as StatDataBase);
      
        
        _statsListView = root.Q<MultiColumnListView>("stats");
        _primaryStatsListView = root.Q<MultiColumnListView>("primary-stats");
        _attributesListView = root.Q<MultiColumnListView>("attributes");

        Toolbar toolbarTabs = root.Q<Toolbar>("toolbar-tabs");
        Button tb_primarystats = root.Q<ToolbarButton>("tb-primarystats");
        Button tb_stats = root.Q<ToolbarButton>("tb-stats");
        Button tb_attributes = root.Q<ToolbarButton>("tb-attributes");
        
        _buttons.Add(tb_primarystats);
        _buttons.Add(tb_stats);
        _buttons.Add(tb_attributes);
        
        tb_primarystats.clicked += () =>
        {
            SetButtonColorSelected(tb_primarystats);
            _currentListView.style.display = DisplayStyle.None;
            _primaryStatsListView.style.display = DisplayStyle.Flex;
            _currentListView = _primaryStatsListView;
        };
        tb_stats.clicked += () =>
        {
            SetButtonColorSelected(tb_stats);
            _currentListView.style.display = DisplayStyle.None;
            _statsListView.style.display = DisplayStyle.Flex;
            _currentListView = _statsListView;
        };
        tb_attributes.clicked += () =>
        {
            SetButtonColorSelected(tb_attributes);
            _currentListView.style.display = DisplayStyle.None;
            _attributesListView.style.display = DisplayStyle.Flex;
            _currentListView = _attributesListView;
        };
        
        if (controller.IsInitialized)
        {
            initIcon.style.backgroundImage = greenDot;
           
            _primaryStatsListView.style.display = DisplayStyle.Flex;
            toolbarTabs.style.display = DisplayStyle.Flex;
            _statsListView.style.display = DisplayStyle.None;
            _attributesListView.style.display = DisplayStyle.None;

            tb_primarystats.style.backgroundColor = new StyleColor(new Color(0.43f,0.42f,0.42f,1));
            tb_attributes.style.backgroundColor = new StyleColor(new Color(0.23f,0.23f,0.23f,1));
            tb_stats.style.backgroundColor = new StyleColor(new Color(0.23f,0.23f,0.23f,1));
            
        }
        else
        {
            initIcon.style.backgroundImage = redDot;
            
            toolbarTabs.style.display = DisplayStyle.None;
            _primaryStatsListView.style.display = DisplayStyle.None;
            _statsListView.style.display = DisplayStyle.None;
            _attributesListView.style.display = DisplayStyle.None;
        }
        if (controller.StatList != null && controller.StatList.Count > 0) // stat list
        {
            _statsListView.itemsSource = controller.StatList;
            var cols = _statsListView.columns;
            cols["name"].makeCell = () => new Label();
            cols["starting-health"].makeCell = () => new Label();
            cols["current-health"].makeCell = () => new Label();

            cols["name"].bindCell = (VisualElement e, int index) =>
            {
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.LowerCenter);
                (e as Label).style.alignItems = new StyleEnum<Align>(Align.Center);
                (e as Label).style.alignContent = new StyleEnum<Align>(Align.Center);
                (e as Label).style.alignSelf = new StyleEnum<Align>(Align.Center);
                (e as Label).text = controller.StatList[index].Definition.name;
            };
            cols["starting-health"].bindCell = (VisualElement e, int index) =>
            {
                //(e as Label).text = "null";
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                (e as Label).text = controller.StatList[index].Value.ToString();
                
               // (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                //(e as Label).text = controller._statsList[index].CurrentValue.ToString();
            };
            cols["current-health"].bindCell = (VisualElement e, int index) =>
            {
                if (controller.StatList[index].Definition.Cap == -1)
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = "Infinite";
                }
                else
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = controller.StatList[index].Definition.Cap.ToString();
                }
                
            };

            cols["name"].unbindCell = (VisualElement e, int i) => e.Unbind();
            cols["starting-health"].unbindCell = (VisualElement e, int i) => e.Unbind();
        }
     
        if (controller.PrimaryStatList != null && controller.PrimaryStatList.Count > 0) // primary-stat list
        {
            _primaryStatsListView.itemsSource = controller.PrimaryStatList;
            var cols = _primaryStatsListView.columns;
            cols["name"].makeCell = () => new Label();
            cols["starting-health"].makeCell = () => new Label();
            cols["current-health"].makeCell = () => new Label();

            cols["name"].bindCell = (VisualElement e, int index) =>
            {
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                (e as Label).text = controller.PrimaryStatList[index].Definition.name;
            };
            cols["starting-health"].bindCell = (VisualElement e, int index) =>
            {
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                (e as Label).text = controller.PrimaryStatList[index].Value.ToString();
            };
            cols["current-health"].bindCell = (VisualElement e, int index) =>
            {
                if (controller.PrimaryStatList[index].Definition.Cap == -1)
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = "Infinite";
                }
                else
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = controller.PrimaryStatList[index].Definition.Cap.ToString();
                }
                
            };

            cols["name"].unbindCell = (VisualElement e, int i) => e.Unbind();
            cols["starting-health"].unbindCell = (VisualElement e, int i) => e.Unbind();
        }
        
        if (controller.AttributeList != null && controller.AttributeList.Count > 0) // attributes list
        {
            _attributesListView.itemsSource = controller.AttributeList;
            var cols = _attributesListView.columns;
            cols["name"].makeCell = () => new Label();
            cols["starting-health"].makeCell = () => new Label();
            cols["current-health"].makeCell = () => new Label();

            cols["name"].bindCell = (VisualElement e, int index) =>
            {
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                (e as Label).text = controller.AttributeList[index].Definition.name;
            };
            cols["starting-health"].bindCell = (VisualElement e, int index) =>
            {
                (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                (e as Label).text = controller.AttributeList[index].CurrentValue.ToString();
            };
            cols["current-health"].bindCell = (VisualElement e, int index) =>
            {
                if (controller.AttributeList[index].Definition.Cap == -1)
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = "Infinite";
                }
                else
                {
                    (e as Label).style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    (e as Label).text = controller.AttributeList[index].Definition.Cap.ToString();
                }
                
            };

            cols["name"].unbindCell = (VisualElement e, int i) => e.Unbind();
            cols["starting-health"].unbindCell = (VisualElement e, int i) => e.Unbind();
        }
        
        
        if (_currentListView == null && controller.IsInitialized)
        {
            _currentListView = _primaryStatsListView;
            _currentListView.style.display = DisplayStyle.Flex;
            SetButtonColorSelected(tb_primarystats);
        }else if (_currentListView != null)
        {
            _currentListView.RefreshItems();
        }
       _attributesListView.RefreshItems();
        
        return root;
    }
    
    void SetButtonColorSelected(Button selected)
    {
        foreach (Button btn in _buttons)
        {
            btn.style.backgroundColor = _buttonDefaultColor;
        }
        selected.style.backgroundColor = new StyleColor(new Color(0.43f,0.42f,0.42f,1));
    }
}
#endif