using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

public class TagSelectorWindow : EditorWindow
{
    private static SerializedProperty tagProperty;
    private static SerializedProperty rootProperty;

    private Vector2 scrollPosition;
    private List<string> items = new List<string>();

   
    private GUIStyle itemStyle;
    private Color lightColor = new Color(0.5f, 0.5f, 0.5f);
    private Color darkColor = new Color(0.4f, 0.4f, 0.4f); 
    
    private List<TagItem> structuredItems = new List<TagItem>();
    
    private Texture2D lightTexture;
    private Texture2D darkTexture;
    private Texture2D hoverTexture;
    private Texture2D transparentTexture;
    private Texture2D backgroundBorderTexture;
    private Texture2D backgroundTexture;
    private Texture2D buttonBackgroundTexture;
    private string searchTerm = "";
    private static TagSelectorWindow _window;
    private static GameplayTag GameplayTag;
    private static Rect ParentButtonRect;
    private static GameplayTagsAsset GameplayTagsAsset;
    private bool _showCreateTagField;
    private Vector2 _createTagFieldPos;
    private string _createTagString;
    private string _createTagStringPrefix;

    private bool _showDeleteTagWindow;
    private Vector2 _deleteTagFieldPos;
    private string _deleteTagFieldTagName;
    private TagItem _deleteTagItem;

    public static void ShowWindow(Rect buttonRect, SerializedProperty property,GameplayTag gameplayTag,GameplayTagsAsset tagsAsset, SerializedProperty rootProp)
    {
        tagProperty = property;
        rootProperty = rootProp;
        GameplayTag = gameplayTag;
        ParentButtonRect = buttonRect;
        GameplayTagsAsset = tagsAsset;
        var window = CreateInstance<TagSelectorWindow>();

        var posRect = GUIUtility.GUIToScreenRect(buttonRect);
        var windowPosition = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));
        window.position = new Rect(windowPosition, new Vector2(150, 200)); 
        window.structureItems(tagsAsset.TagsCache);
        window.wantsMouseMove = true;
        window.ShowAsDropDown(posRect, new Vector2(300, 500));
        _window = window;
    }
    private void structureItems(List<GameplayTagInfo> tags, bool foldoutAll = false)
    {
        structuredItems.Clear();
        var itemLookup = new Dictionary<string, TagItem>();

        foreach (var tag in tags.OrderBy(tag => tag.Tag))
        {
            var parts = tag.Tag.Split('.');
            TagItem currentItem = null;
            string currentPath = "";

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}.{part}";

                TagItem foundItem;
                if (!itemLookup.TryGetValue(currentPath, out foundItem))
                {
                    foundItem = new TagItem(part, currentPath);
                    foundItem.HashCode = tag.HashCode;
                    foundItem.IsFoldedOut = foldoutAll;
                    if (i == 0)
                    {
                        structuredItems.Add(foundItem);
                    }
                    else
                    {
                        currentItem?.Children.Add(foundItem);
                    }
                    itemLookup[currentPath] = foundItem;
                }
                currentItem = foundItem;
            }
        }
        
    }

    private void InitTexturesAndStyles()
    {
        lightTexture = MakeTex(1, 1, lightColor);
        darkTexture = MakeTex(1, 1, darkColor);
        hoverTexture = MakeTex(1, 1, new Color(0.8f, 0.8f, 0.8f));
        backgroundBorderTexture = MakeTex(1, 1, new Color(0.16f, 0.16f, 0.16f));
        transparentTexture = MakeTex(1, 1, new Color(0, 0, 0, 0));
        buttonBackgroundTexture = MakeTex(1, 1, new Color(0.129f, 0.435f, 0.922f));
        itemStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { background = null, textColor = Color.white },
            //hover = { background = hoverTexture, textColor = Color.white }
        };
    }
    
    private void OnGUI()
    {
        Vector2 mousePosition = Event.current.mousePosition;
        if (Event.current.type == EventType.MouseMove)
        {
            Repaint();
        }
        if (itemStyle == null)
        {
            InitTexturesAndStyles();
        }
        var filteredItems =  FilterItemsWithChildren(structuredItems, searchTerm);
      
        GUI.DrawTexture(new Rect(0, 0, 500, 500), backgroundBorderTexture);
        foreach (var item in filteredItems)
        {
            GUI.DrawTexture(item.ButtonRect, lightTexture);
        }
        GUI.enabled = !_showCreateTagField && !_showDeleteTagWindow;
        
        // Search field
        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        searchTerm = EditorGUILayout.TextField(searchTerm, GUI.skin.FindStyle("ToolbarSearchTextField"));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            // Clear search term when the "X" is clicked
            searchTerm = "";
            GUI.FocusControl(null); // Remove focus from the search field
        }
        EditorGUILayout.EndHorizontal();
        //
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        Rect newTagButtonRect = GUILayoutUtility.GetRect(new GUIContent(), itemStyle, GUILayout.ExpandWidth(true));
        if (GUI.Button(newTagButtonRect,"CreateNewTag", GUI.skin.button))
        {
            _createTagString = "";
            _createTagFieldPos = new Vector2(newTagButtonRect.xMin,newTagButtonRect.y);
            _showCreateTagField = true;
        }
        foreach (var item in filteredItems)
        {
            int index = structuredItems.IndexOf(item);
            itemStyle.normal.textColor = Color.white; 
            itemStyle.normal.background =  index % 2 == 0 ? lightTexture : darkTexture;
            itemStyle.normal.background = transparentTexture;
            
            RenderItem(item, 0);
        }

        EditorGUILayout.EndScrollView();
        
        GUI.enabled = true;
        // create tag field
        if (_showCreateTagField)
        {
            Rect backRect = new Rect(_createTagFieldPos.x, _createTagFieldPos.y, 200, 30);
            EditorGUI.DrawRect(backRect, new Color(0.25f,0.25f,0.25f));
            float fieldRectY = backRect.y + (backRect.height - 25) / 2;

            GUIStyle createTagStyle = new GUIStyle(GUI.skin.textField);
            createTagStyle.fontSize = 15;
            Rect fieldRect = new Rect(backRect.x + 10, fieldRectY, 140, 25);
            GUI.SetNextControlName("textfield");
            _createTagString = EditorGUI.TextField(fieldRect, _createTagString,createTagStyle);
            GUI.FocusControl("textfield");
            if (Event.current.type == EventType.MouseDown)
            {
                if (fieldRect.Contains(mousePosition) == false)
                {
                    _showCreateTagField = false;
                    Repaint();
                }
            }
            if (GUI.GetNameOfFocusedControl() == "textfield")
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    _showCreateTagField = false;
                    string newTag = _createTagStringPrefix + _createTagString;
                    //GameplayTagsAsset.tags.Add(newTag);
                    GameplayTagInfo newTagInfo = new GameplayTagInfo();
                    newTagInfo.Tag = newTag;
                    GameplayTagsAsset.TagsCache.Add(newTagInfo);
                    newTagInfo.GenerateNewHashCode(GameplayTagsAsset);
                    structureItems(GameplayTagsAsset.TagsCache,true);
                    Repaint();
                }
            }
           
        }

        if (_showDeleteTagWindow)
        {
            Rect backRect = new Rect(_deleteTagFieldPos.x, _deleteTagFieldPos.y, 200, 30);
            EditorGUI.DrawRect(backRect, new Color(0.25f,0.25f,0.25f));
            float fieldRectY = backRect.y + (backRect.height - 25) / 2;

            GUIStyle createTagStyle = new GUIStyle(GUI.skin.textField);
            Rect fieldRect = new Rect(backRect.x + 140, fieldRectY, 50, 20);
            Rect labelRect = new Rect(backRect.x, fieldRectY, 150, 20);
            //_createTagString = EditorGUI.TextField(fieldRect, _createTagString,createTagStyle);
            GUI.Label(labelRect,"Delete tag : " + _deleteTagFieldTagName);
            if (GUI.Button(fieldRect, "Delete",GUI.skin.button))
            {
                foreach (var tagFetcher in GameplayTagsAsset.TagsCache)
                {
                    if (tagFetcher.HashCode == _deleteTagItem.HashCode)
                    {
                        GameplayTagsAsset.TagsCache.Remove(tagFetcher);
                        structureItems(GameplayTagsAsset.TagsCache,true);
                        _showDeleteTagWindow = false;
                        Repaint();
                        break;
                    }
                }
            }
            if (Event.current.type == EventType.MouseDown)
            {
                if (fieldRect.Contains(mousePosition) == false)
                {
                    _showDeleteTagWindow = false;
                    Repaint();
                }
            }
        }
    }
    private List<TagItem> FilterItemsWithChildren(List<TagItem> items, string searchTerm)
    {
        var filteredList = new List<TagItem>();

        if (string.IsNullOrEmpty(searchTerm))
        {
            return items;
        }
        // Convert the search term to lowercase for case-insensitive comparison.
        searchTerm = searchTerm.ToLowerInvariant();

        foreach (var item in items)
        {
            // Check if the current item or any of its children contains the search term.
            if (ItemOrDescendantContainsTerm(item, searchTerm))
            {
                filteredList.Add(item);
            }
        }

        return filteredList;
    }

    private bool ItemOrDescendantContainsTerm(TagItem item, string searchTerm)
    {
        // Check the current item's FullPath for the search term.
        if (item.FullPath.ToLowerInvariant().Contains(searchTerm))
        {
            return true;
        }

        // Recursively check children.
        foreach (var child in item.Children)
        {
            if (ItemOrDescendantContainsTerm(child, searchTerm))
            {
                item.IsFoldedOut = true;
                return true;
            }
        }

        return false;
    }

    private void RenderItem(TagItem item, int indentLevel)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Space(indentLevel * 30); 
        Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(item.Name), itemStyle, GUILayout.ExpandWidth(true));
        Rect backgroundRect = new Rect(buttonRect.x - 150, buttonRect.y, 600, buttonRect.height + 5);
        if (backgroundRect.Contains(Event.current.mousePosition))
        {
            GUI.DrawTexture(backgroundRect,buttonBackgroundTexture);
        }
       
        bool hasChildren = item.Children.Count > 0;
        if (hasChildren)
        {
            GUIStyle expandButtonStyle = new GUIStyle();
            expandButtonStyle.normal.background = MakeTex(1, 1, new Color(0,0,0,0));
            expandButtonStyle.hover.textColor = Color.black;
            expandButtonStyle.alignment = TextAnchor.MiddleCenter;
            expandButtonStyle.normal.textColor = Color.white;
            expandButtonStyle.stretchWidth = false;
            expandButtonStyle.fixedWidth = 14;
            expandButtonStyle.fontSize = 10;
            expandButtonStyle.margin = new RectOffset(0, 0, 5, 0);

            if (GUI.Button(new Rect(buttonRect.xMin,buttonRect.y,15,15),item.IsFoldedOut ? "\u25bc" : "\u25b6", expandButtonStyle))
            {
                item.IsFoldedOut = !item.IsFoldedOut;
                if (item.IsFoldedOut)
                {
                    if (Event.current.alt)
                    {
                        item.FoldoutChildren();
                    }
                }
                else
                {
                    if (Event.current.alt)
                    {
                        item.ShrinkChildred();
                    }
                }
            }
        }
        Vector2 mousePosition = Event.current.mousePosition;
        
        float plusButtonSize = 20;
        Rect plusButtonRect = new Rect(buttonRect.xMax - plusButtonSize, buttonRect.y+ 1, plusButtonSize, plusButtonSize);
        Rect itemButtonRect = new Rect(buttonRect.x + 15, buttonRect.y, buttonRect.width, buttonRect.height);
        item.ButtonRect = buttonRect;
        if (GUI.Button(itemButtonRect, item.Name, itemStyle))
        {
            if (Event.current.button == 1)
            {
                if(item.Children.Count > 0) return;
                _deleteTagItem = item;
                _deleteTagFieldTagName = item.Name;
                _deleteTagFieldPos =  new Vector2(buttonRect.xMin, buttonRect.yMax + 20);
                _showDeleteTagWindow = true;
            }
            else if (Event.current.button == 0)
            {
                if (plusButtonRect.Contains(mousePosition))
                {
                    _showCreateTagField = true;
                    _createTagFieldPos = new Vector2(buttonRect.xMin, buttonRect.y);
                    _createTagString = "";
                    _createTagStringPrefix = item.FullPath + ".";
                    DDebug.Log("Plus clicked");
                }
                else
                {
                    if (tagProperty == null)
                    {
                        
                    }
                    tagProperty.serializedObject.Update();
                    tagProperty.stringValue = item.FullPath;
                    rootProperty.FindPropertyRelative("hashCode").stringValue = item.HashCode;
                    //rootProperty.FindPropertyRelative("hashCode").objec
                    //Debug.Log("31" + rootProperty.name);
                    if (GetParent(tagProperty) is GameplayTag tag)
                    {
                        tag.SetTag(item.FullPath,item.HashCode);
                    }
                    tagProperty.serializedObject.ApplyModifiedProperties();
                    
                    Close();
                }
            }
            
        }
        GUIStyle plusButtonStyle = new GUIStyle();
        plusButtonStyle.fontSize = 15;
        plusButtonStyle.fontStyle = FontStyle.Bold;
        plusButtonStyle.normal.background = MakeTex(1, 1, lightColor);
        plusButtonStyle.hover.background = lightTexture;
        plusButtonStyle.alignment = TextAnchor.MiddleCenter;
        plusButtonStyle.normal.textColor = Color.white;
        if (GUI.Button(plusButtonRect, "+", plusButtonStyle)) { }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3);
        Rect rect = EditorGUILayout.GetControlRect(false, 1 );

        rect.height = 1;

        EditorGUI.DrawRect(rect, Color.black);
       
        if (item.IsFoldedOut && hasChildren)
        {
            foreach (var child in item.Children)
            {
                RenderItem(child, indentLevel + 1);
            }
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnLostFocus()
    {
        Close(); 
    }
   
    public object GetParent(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach(var element in elements.Take(elements.Length-1))
        {
            if(element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    public object GetValue(object source, string name)
    {
        if(source == null)
            return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if(f == null)
        {
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(p == null)
                return null;
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }

    public object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while(index-- >= 0)
            enm.MoveNext();
        return enm.Current;
    }
}

class TagItem
{
    public string Name; 
    public string FullPath;
    public string HashCode;
    public List<TagItem> Children = new List<TagItem>();
    public bool IsFoldedOut = false;
    public Rect ButtonRect;

    public void FoldoutChildren()
    {
        foreach (var tagItem in Children)
        {
            IsFoldedOut = true;
            tagItem.IsFoldedOut = true;
            tagItem.FoldoutChildren();
        }
    }
    public void ShrinkChildred()
    {
        foreach (var tagItem in Children)
        {
            IsFoldedOut = false;
            tagItem.IsFoldedOut = false;
            tagItem.ShrinkChildred();
        }
    }
    public TagItem(string name, string fullPath)
    {
        Name = name;
        FullPath = fullPath;
    }
}


