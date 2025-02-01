using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayTagTree : PopupWindowContent
{
    public static SerializedProperty tagNamesProperty;
    
    private GameplayTagsAsset gameplayTagsAsset;
    private Action<string> onTagSelectedCallback;

    public SerializedProperty ContainerProperty;
    public bool ShowToggle = true;
    public GameplayTagTree(Action<string> onTagSelectedCallback)
    {
        this.onTagSelectedCallback = onTagSelectedCallback;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, 500);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        VisualElement root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/actor-core/GameplayTags/Editor/TagSelectionWindow/GameplayTagTree.uxml").CloneTree();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        GameplayTagManger2.InitializeIfNeeded();
        gameplayTagsAsset = GameplayTagManger2.TagAssets[0];

        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/actor-core/GameplayTags/Editor/TagSelectionWindow/GameplayTagTree.uxml");
        visualTreeAsset.CloneTree(editorWindow.rootVisualElement);
        
        VisualElement rootVisualElement = editorWindow.rootVisualElement;
        var treeView = rootVisualElement.Q<TreeView>();
        treeView.fixedItemHeight = 25;
        GameplayTagManger2.InitializeIfNeeded();
        var tagHierarchy = TagHierarchyBuilder.BuildTagHierarchy(GameplayTagManger2.TagAssets[0].TagsCache);
        Debug.Log(tagHierarchy.Children.Count);
        var items = BuildTreeViewItems(tagHierarchy);

        treeView.SetRootItems(items);
        
        treeView.makeItem = () =>
        {
            var rowContainer = new VisualElement();
            rowContainer.style.flexDirection = FlexDirection.Row;
            rowContainer.AddToClassList("unity-tag-row");
  
            if(ShowToggle)
            {
                var toggle = new Toggle();
                toggle.name = "Toggle";
                toggle.RemoveFromClassList("unity-toggle__checkmark");
                //toggle.AddToClassList("gameplay-tag-toggle");
                rowContainer.Add(toggle);
            }
            
            var label = new Label();
            label.name = "NameLabel";
            label.AddToClassList("unity-tag-label");
            rowContainer.Add(label);
            
            

            return rowContainer;
        };
        treeView.bindItem = (VisualElement element, int index) =>
        {
            // Get the data item for the current row
            var itemData = treeView.GetItemDataForIndex<GameplayTagTreeItem>(index);
            
            itemData.RuntimeIndex = index;

            // Find the Label and Toggle in the row
            var label = element.Q<Label>("NameLabel");
            
            if(ShowToggle)
            {
                var toggle = element.Q<Toggle>("Toggle");
                toggle.UnregisterValueChangedCallback(evt => {});
                toggle.SetValueWithoutNotify(TagExists(itemData.HashCode));
                toggle.userData = itemData;
                toggle.RegisterValueChangedCallback(OnToggleChanged);
            }
           
                
            
            // Bind the data to the Label and Toggle
            label.text = itemData.Tag;
            //toggle.value = itemData.populated;
           
        };
        
        treeView.selectionChanged += selectedItems =>
        {
            foreach (var selectedItem in selectedItems)
            {
                Debug.Log("selected item is " + selectedItem.GetType());      
                if (selectedItem is GameplayTagTreeItem item)
                {
                    Debug.Log($"Selected Tag: {item.Tag}, HashCode: {item.HashCode}");
                    
                    
                    if(!ShowToggle)//if toggle is not shown, close the window
                    {
                        onTagSelectedCallback?.Invoke(item.HashCode);
                        editorWindow.Close();
                    }
                }
            }
        };
        treeView.Rebuild();

        var expandButton = rootVisualElement.Q<Button>("Expand");
        var collapseButton = rootVisualElement.Q<Button>("Collapse");

        expandButton.clicked += () => treeView.ExpandAll();
        collapseButton.clicked += () => treeView.CollapseAll();
    }

    private bool TagExists(string tag)
    {
        // Check if the tag already exists in the property
        bool tagExists = false;
        for (int i = 0; i < ContainerProperty.arraySize; i++)
        {
            SerializedProperty element = ContainerProperty.GetArrayElementAtIndex(i);
            if (element.stringValue == tag)
            {
                tagExists = true;
                return true;
                break;
            }
        }

        return false;
    }
    private void OnToggleChanged(ChangeEvent<bool> evt)
    {
        var toggle = evt.target as Toggle;
        var itemData = toggle?.userData as GameplayTagTreeItem;
        
        if (itemData != null)
        {
            Debug.Log($"Toggle for Tag '{itemData.Tag}' changed to: {evt.newValue}");
            onTagSelectedCallback?.Invoke(itemData.HashCode);
        }
        
    }

    private void ToggleValueChanged(ChangeEvent<bool> evt)
    {
        
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    
    private List<TreeViewItemData<GameplayTagTreeItem>> BuildTreeViewItems(TagTreeNode root)
    {
        var items = new List<TreeViewItemData<GameplayTagTreeItem>>();
        int idCounter = 0;

        foreach (var child in root.Children)
        {
            items.Add(BuildTreeViewItem(child, ref idCounter));
        }

        return items;
    }

    private TreeViewItemData<GameplayTagTreeItem> BuildTreeViewItem(TagTreeNode node, ref int idCounter)
    {
        int currentId = idCounter++;

        var children = new List<TreeViewItemData<GameplayTagTreeItem>>();
        foreach (var child in node.Children)
        {
            children.Add(BuildTreeViewItem(child, ref idCounter));
        }

        // Replace string with GameplayTagTreeItem
        //var tagInfo = GameplayTagManger2.TagAssets[0].TagsCache.Find(t => t.Tag == node.Tag);
        GameplayTag gameplayTag = GameplayTagManger2.RequestTag(node.FullTag);
        var gameplayTagItem = new GameplayTagTreeItem(node.Tag, gameplayTag.HashCode);

        return new TreeViewItemData<GameplayTagTreeItem>(currentId, gameplayTagItem, children);
    }

    private void OnTagSelected(IEnumerable<object> selectedItems)
    {
        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is TreeViewItemData<string> item)
            {
                Debug.Log($"Selected Tag: {item.data}");
            }
        }
    }

    private void ExpandAll(TreeView treeView, bool expand)
    {
        Debug.Log("clicked");
    }
}
public class GameplayTagTreeItem
{
    public string Tag { get; set; }        // Full tag string
    public string HashCode { get; set; }  // Hash tag

    public int RuntimeIndex;

    public GameplayTagTreeItem(string tag, string hashCode)
    {
        Tag = tag;
        HashCode = hashCode;
    }

    public override string ToString()
    {
        return Tag; // Default display in the TreeView
    }
}
