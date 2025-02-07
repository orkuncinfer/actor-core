using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayTagTree : PopupWindowContent
{
    public static SerializedProperty tagNamesProperty;

    private GameplayTagsAsset gameplayTagsAsset;
    private Action<string> onTagSelectedCallback;

    public SerializedProperty ContainerProperty;
    public bool ShowToggle = true;
    private TreeView _treeView;
    private TagTreeNode _tagHierarchy;

    public GameplayTagTree(Action<string> onTagSelectedCallback)
    {
        this.onTagSelectedCallback = onTagSelectedCallback;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, 500);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        GameplayTagManager.InitializeIfNeeded();
        gameplayTagsAsset = GameplayTagManager.TagAssets[0];

        var visualTreeAsset =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Plugins/actor-core/GameplayTags/Editor/TagSelectionWindow/GameplayTagTree.uxml");
        visualTreeAsset.CloneTree(editorWindow.rootVisualElement);

        VisualElement rootVisualElement = editorWindow.rootVisualElement;
        _treeView = rootVisualElement.Q<TreeView>();
        _treeView.fixedItemHeight = 25;

        // Build initial tag hierarchy
        _tagHierarchy = TagHierarchyBuilder.BuildTagHierarchy(GameplayTagManager.TagAssets[0].TagsCache);

        var objectField = rootVisualElement.Q<ObjectField>();
        objectField.value = GameplayTagManager.TagAssets[0];

        // Set up the search bar
        var searchBar = rootVisualElement.Q<TextField>();
        searchBar.RegisterValueChangedCallback(evt => ApplySearchFilter(evt.newValue));

        // Initialize tree view
        var items = BuildTreeViewItems(_tagHierarchy);
        _treeView.SetRootItems(items);

        // Define how each row is created
        _treeView.makeItem = () =>
        {
            var rowContainer = new VisualElement();
            rowContainer.style.flexDirection = FlexDirection.Row;
            rowContainer.AddToClassList("unity-tag-row");

            if (ShowToggle)
            {
                var toggle = new Toggle { name = "Toggle" };
                toggle.RemoveFromClassList("unity-toggle__checkmark");
                rowContainer.Add(toggle);
            }

            var label = new Label { name = "NameLabel" };
            label.AddToClassList("unity-tag-label");
            rowContainer.Add(label);
            
           /* VisualElement line = new VisualElement();
            line.name = "vertical-line";
            line.AddToClassList("line-style");
            line.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            line.style.left =-10;
            line.style.top = 0;
            rowContainer.Add(line);*/
            
            return rowContainer;
        };

        _treeView.bindItem = (VisualElement element, int index) =>
        {
            var itemData = _treeView.GetItemDataForIndex<GameplayTagTreeItem>(index);
            itemData.RuntimeIndex = index;

            var label = element.Q<Label>("NameLabel");
            label.text = itemData.Tag;

            if (ShowToggle)
            {
                var toggle = element.Q<Toggle>("Toggle");
                toggle.UnregisterValueChangedCallback(evt => { });
                toggle.SetValueWithoutNotify(TagExists(itemData.HashCode));
                toggle.userData = itemData;
                toggle.RegisterValueChangedCallback(OnToggleChanged);
            }
        };

        _treeView.selectionChanged += selectedItems =>
        {
            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem is GameplayTagTreeItem item)
                {
                    if (!ShowToggle) // Close window if toggles are not shown
                    {
                        onTagSelectedCallback?.Invoke(item.HashCode);
                        editorWindow.Close();
                    }
                }
            }
        };

        _treeView.Rebuild();

        var expandButton = rootVisualElement.Q<Button>("Expand");
        var collapseButton = rootVisualElement.Q<Button>("Collapse");

        expandButton.clicked += () => _treeView.ExpandAll();
        collapseButton.clicked += () => _treeView.CollapseAll();
    }

    private void ApplySearchFilter(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _treeView.SetRootItems(BuildTreeViewItems(_tagHierarchy));
            _treeView.Rebuild();
            return;
        }

        searchText = searchText.ToLowerInvariant();
        var filteredItems = new List<TreeViewItemData<GameplayTagTreeItem>>();
        int idCounter = 0;

        // Recursively find matching nodes but exclude the main root
        foreach (var child in _tagHierarchy.Children)
        {
            var matchedChild = FilterTreeWithParents(child, searchText, ref idCounter);
            if (matchedChild.HasValue)
            {
                filteredItems.Add(matchedChild.Value);
            }
        }

        _treeView.SetRootItems(filteredItems);
        _treeView.Rebuild();
        _treeView.ExpandAll(); // Ensure parents are expanded
    }

    private TreeViewItemData<GameplayTagTreeItem>? FilterTreeWithParents(TagTreeNode node, string searchText,
        ref int idCounter)
    {
        bool matches = FuzzyMatch(node.Tag, searchText);
        var filteredChildren = new List<TreeViewItemData<GameplayTagTreeItem>>();

        foreach (var child in node.Children)
        {
            var matchedChild = FilterTreeWithParents(child, searchText, ref idCounter);
            if (matchedChild.HasValue)
            {
                filteredChildren.Add(matchedChild.Value);
            }
        }

        if (matches || filteredChildren.Count > 0)
        {
            int currentId = idCounter++;
            GameplayTag gameplayTag = GameplayTagManager.RequestTag(node.FullTag);
            var gameplayTagItem = new GameplayTagTreeItem(node.Tag, gameplayTag.HashCode);

            return new TreeViewItemData<GameplayTagTreeItem>(currentId, gameplayTagItem, filteredChildren);
        }

        return null;
    }

    private bool FuzzyMatch(string tag, string searchText)
    {
        tag = tag.ToLowerInvariant();

        if (tag.Contains(searchText))
            return true;

        var words = tag.Split(new char[] { '.', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (word.StartsWith(searchText))
                return true;
        }

        return false;
    }


    private bool TagExists(string tag)
    {
        for (int i = 0; i < ContainerProperty.arraySize; i++)
        {
            SerializedProperty element = ContainerProperty.GetArrayElementAtIndex(i);
            if (element.stringValue == tag)
            {
                return true;
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
            onTagSelectedCallback?.Invoke(itemData.HashCode);
        }
    }

    private List<TreeViewItemData<GameplayTagTreeItem>> BuildTreeViewItems(TagTreeNode root)
    {
        var items = new List<TreeViewItemData<GameplayTagTreeItem>>();
        int idCounter = 0;

        foreach (var child in root.Children)
        {
            items.Add(BuildTreeViewItem(child, 0, ref idCounter)); // Start with depth = 0
        }

        return items;
    }

    private TreeViewItemData<GameplayTagTreeItem> BuildTreeViewItem(TagTreeNode node, int depth, ref int idCounter)
    {
        int currentId = idCounter++;

        var children = new List<TreeViewItemData<GameplayTagTreeItem>>();
        foreach (var child in node.Children)
        {
            children.Add(BuildTreeViewItem(child, depth + 1, ref idCounter)); // Increment depth for child
        }

        GameplayTag gameplayTag = GameplayTagManager.RequestTag(node.FullTag);
        var gameplayTagItem = new GameplayTagTreeItem(node.Tag, gameplayTag.HashCode) { Depth = depth }; // Store depth in GameplayTagTreeItem

        return new TreeViewItemData<GameplayTagTreeItem>(currentId, gameplayTagItem, children);
    }
}

public class GameplayTagTreeItem
{
    public string Tag { get; set; }        // Full tag string
    public string HashCode { get; set; }  // Hash tag
    public int Depth { get; set; }        // Depth in the tree

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
