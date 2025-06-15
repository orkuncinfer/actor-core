using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;


using Sirenix.OdinInspector;


using UnityEngine;
using UnityEngine.UIElements;


namespace Core.Editor
{
    public class NodeGraphEditorWindow : EditorWindow, ISearchWindowProvider
    {
        private NodeGraph _nodeGraph;
        private NodeGraphView _nodeGraphView;
        private VisualElement _infoPanel;
        private UnityEditor.Editor _editor;
        private Button _saveButton;
        
        private Texture2D m_Icon;
        
        private double nextSaveTime;
        private const float SaveInterval = 5f;
        private void OnEnable()
        {
            // Transparent icon to trick search window into indenting items
            m_Icon = new Texture2D(1, 1);
            m_Icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            m_Icon.Apply();

            EditorApplication.update += OnEditorUpdate;
        }
        private void OnDisable()
        {
            _nodeGraphView.Save();
            NodeGraphHelper.SaveScriptableObjects();
            
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnEditorUpdate()
        {
            if (NodeGraphHelper.SOSaveCache != null)
            {
                if(NodeGraphHelper.SOSaveCache.Count > 0)
                {
                    _saveButton.text = "Save*";
                    return;
                }
            }   
            _saveButton.text = "Save";
        }

        public static void ShowWindow(NodeGraph nodeGraph)
        {
            NodeGraphEditorWindow wnd = GetWindow<NodeGraphEditorWindow>();
            wnd.SelectNodeGraph(nodeGraph);
            wnd.titleContent = new GUIContent("NodeGraph");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is NodeGraph nodeGraph)
            {
                ShowWindow(nodeGraph);
                return true;
            }
            return false;
        }
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/actor-core/NodeGraph/NodeGraphEditorWindowX.uxml");
            visualTree.CloneTree(root);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/actor-core/NodeGraph/NodeGraphEditorWindow.uss");
            root.styleSheets.Add(styleSheet);
            
            _saveButton = root.Q<Button>("save-button");
            _saveButton.clickable.clicked += ClickableOnclicked;
            //_infoPanel = root.Q("info-panel");
            _nodeGraphView = root.Q<NodeGraphView>();
            _nodeGraphView.nodeCreationRequest += OnRequestNodeCreation;
            _nodeGraphView.onNodeSelected = OnNodeSelected;
        }

        private void ClickableOnclicked()
        {
            NodeGraphHelper.SaveScriptableObjects();
        }

        private void OnNodeSelected(NodeView nodeView)
        {
           /* _infoPanel.Clear();
            DestroyImmediate(_editor);
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (_editor && _editor.target)
                {
                    _editor.OnInspectorGUI();
                    _editor.Repaint();
                }
            });
            _infoPanel.Add(container);*/
        }

        private void OnRequestNodeCreation(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is NodeGraph nodeGraph)
            {
                SelectNodeGraph(nodeGraph);
            }
        }

        void SelectNodeGraph(NodeGraph nodeGraph)
        {
            _nodeGraph = nodeGraph;
            _nodeGraphView.PopulateView(_nodeGraph);
        }
        
         internal struct NodeEntry
        {
            public string[] title;
            public NodeView NodeView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var nodeEntries = new List<NodeEntry>();

            Type[] types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                assembly => assembly.GetTypes()).Where(type => typeof(NodeView).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract &&
                                                               type != typeof(NodeView) && type != typeof(ResultNodeView)).ToArray();
            foreach (Type type in types)
            {
                if (type.GetCustomAttributes(typeof(TitleAttribute), false) is TitleAttribute[] attrs && attrs.Length > 0)
                {
                    var node = (NodeView)Activator.CreateInstance(type);
                    nodeEntries.Add(new NodeEntry
                    {
                        NodeView = node,
                        title = attrs[0].Title
                    });
                    node.OnCreated();
                }
            }

            //* Build up the data structure needed by SearchWindow.

            // `groups` contains the current group path we're in.
            var groups = new List<string>();

            // First item in the tree is the title of the window.
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            foreach (var nodeEntry in nodeEntries)
            {
                // `createIndex` represents from where we should add new group entries from the current entry's group path.
                var createIndex = int.MaxValue;

                // Compare the group path of the current entry to the current group path.
                for (var i = 0; i < nodeEntry.title.Length - 1; i++)
                {
                    var group = nodeEntry.title[i];
                    if (i >= groups.Count)
                    {
                        // The current group path matches a prefix of the current entry's group path, so we add the
                        // rest of the group path from the currrent entry.
                        createIndex = i;
                        break;
                    }
                    if (groups[i] != group)
                    {
                        // A prefix of the current group path matches a prefix of the current entry's group path,
                        // so we remove everyfrom from the point where it doesn't match anymore, and then add the rest
                        // of the group path from the current entry.
                        groups.RemoveRange(i, groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }

                // Create new group entries as needed.
                // If we don't need to modify the group path, `createIndex` will be `int.MaxValue` and thus the loop won't run.
                for (var i = createIndex; i < nodeEntry.title.Length - 1; i++)
                {
                    var group = nodeEntry.title[i];
                    groups.Add(group);
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                // Finally, add the actual entry.
                tree.Add(new SearchTreeEntry(new GUIContent(nodeEntry.title.Last(), m_Icon)) { level = nodeEntry.title.Length, userData = nodeEntry });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var nodeEntry = (NodeEntry)entry.userData;
            var nodeView = nodeEntry.NodeView;
            
            nodeView.Node.name = nodeEntry.title[nodeEntry.title.Length - 1];
            Vector2 worldMousePosition = context.screenMousePosition - position.position;
            Vector2 mousePosition = _nodeGraphView.contentViewContainer.WorldToLocal(worldMousePosition);
            nodeView.Node.Guid = GUID.Generate().ToString();
            nodeView.Node.Position = mousePosition;
            nodeView.viewDataKey = nodeView.Node.Guid;
            nodeView.style.left = mousePosition.x;
            nodeView.style.top = mousePosition.y;
            _nodeGraph.AddNode(nodeView.Node);
            _nodeGraphView.AddNodeView(nodeView);
            return true;
        }
    }
}
#endif