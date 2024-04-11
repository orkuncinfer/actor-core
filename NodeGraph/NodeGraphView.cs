#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor
{
    public class NodeGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<NodeGraphView,UxmlTraits>{}

        private NodeGraph _nodeGraph;
        public Action<NodeView> onNodeSelected;

        public Action onGraphPopulated;

        private Vector2 _mousePosition;
        
        public NodeGraphView()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateGroupContextualMenu());
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/actor-core/NodeGraph/NodeGraphEditorWindow.uss");
            styleSheets.Add(styleSheet);

            OnElementsDeleted();
            OnGroupElementsRemoved();
            OnGroupElementsAdded();
        }


        private void OnMouseMove(MouseMoveEvent e)
        {
            _mousePosition = e.localMousePosition;
        }
        
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Group", actionEvent))));
            return contextualMenuManipulator;
        }

        public void Save()
        {
            if (_nodeGraph.NodeGroups != null && _nodeGraph.NodeGroups.Count > 0)
            {
                for (int i = 0; i < _nodeGraph.NodeGroups.Count; i++)
                {
                    _nodeGraph.NodeGroups[i].Position = _nodeGraph.NodeGroups[i].GroupInstance.GetPosition().position;
                    _nodeGraph.NodeGroups[i].Title = _nodeGraph.NodeGroups[i].GroupInstance.title;
                }
            }
        }
        
        private Group CreateGroup(string title, DropdownMenuAction actionEvent)
        {
            
            Vector2 mousePos = actionEvent.eventInfo.mousePosition;
            Group group = new Group();
            group.title = title;
            group.SetPosition(new Rect(_mousePosition, Vector2.zero));
            
            DSGroup dsGroup = ScriptableObject.CreateInstance<DSGroup>();
            if (!_nodeGraph.NodeGroups.Contains(dsGroup))
            {
                dsGroup.name = title;
                dsGroup.GroupInstance = group;
                dsGroup.Position = mousePos;
                dsGroup.Title = title;
                _nodeGraph.AddGroup(dsGroup);
                _nodeGraph.TempGroupDictionary.Add(group,dsGroup);
            }
            return group;
        }

        #region CALLBACKS
        
        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                
            };
        }
        
        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                IEnumerator<GraphElement> enumerator = elements.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    GraphElement currentElement = enumerator.Current;
                    NodeView nw = (NodeView) currentElement;
                    DSGroup dsGroup = _nodeGraph.TempGroupDictionary[group];

                    if (!dsGroup.GroupedNodes.Contains(nw.Node))
                    {
                        dsGroup.GroupedNodes.Add(nw.Node);
                        nw.Node.Group = dsGroup.GroupInstance;
                        Debug.Log("1");
                    }
                         
                    
                    EditorUtility.SetDirty(dsGroup);
                    AssetDatabase.SaveAssets();
                }
            };
        }
        
        
        private void OnElementsDeleted()
        {
            
            deleteSelection = (operationName, user) =>
            {
                List<Group> groupsToDelete = new List<Group>();
                List<NodeView> nodeViewsToDelete = new List<NodeView>();
                List<Edge> edgesToDelete = new List<Edge>();
                
                foreach (GraphElement selectedElement in selection)
                {
                    if (selectedElement is Group grp)
                    {
                        foreach (CodeFunctionNode node in _nodeGraph.TempGroupDictionary[grp].GroupedNodes)
                        {
                            NodeView nw = FindNodeView(node);
                            grp.RemoveElement(nw);
                        }
                        
                        groupsToDelete.Add(grp);
                        _nodeGraph.DeleteGroup(_nodeGraph.TempGroupDictionary[grp]);
                        NodeGraphHelper.SOSaveCache.Add(_nodeGraph);
                    }
                    if (selectedElement is NodeView nodeView)
                    {
                        _nodeGraph.DeleteNode(nodeView.Node);
                        nodeViewsToDelete.Add(nodeView);

                        if (nodeView.Node.Group != null)
                        {
                            Group group = nodeView.Node.Group;
                            if (_nodeGraph.TempGroupDictionary.ContainsKey(group))
                            {
                                Debug.Log(nodeView.Node.Group);
                                _nodeGraph.TempGroupDictionary[group].GroupedNodes.Remove(nodeView.Node);
                            }
                        }
                    }
                    else if (selectedElement is Edge edge)
                    {
                        NodeView parentView = edge.input.node as NodeView;
                        NodeView childView = edge.output.node as NodeView;
                        _nodeGraph.RemoveChild(parentView.Node, childView.Node, edge.input.portName);
                        edgesToDelete.Add(edge);
                    }
                }

                foreach (NodeView nodeView in nodeViewsToDelete)
                {
                    foreach (Port input in nodeView.Inputs)
                    {
                        IEnumerable<Edge> enumerator = input.connections;
                        foreach (Edge edge in enumerator)
                        {
                            edgesToDelete.Add(edge);
                        }
                    }
                    IEnumerable<Edge> output = nodeView.Output.connections;
                    foreach (var edge in output)
                    {
                        edgesToDelete.Add(edge);
                    }

                } // Detect connection edges and delete

                foreach (var nodeView in nodeViewsToDelete)
                {
                    
                }
                DeleteElements(groupsToDelete);
                DeleteElements(nodeViewsToDelete);
                DeleteElements(edgesToDelete);

            };
        }
        
        #endregion
   
        
        internal void PopulateView(NodeGraph nodeGraph)
        {
            _nodeGraph = nodeGraph;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            
            
            if (_nodeGraph.RootNode == null)
            {
                _nodeGraph.RootNode = ScriptableObject.CreateInstance<ResultNode>();
                _nodeGraph.RootNode.name = nodeGraph.name = nodeGraph.RootNode.GetType().Name;
                _nodeGraph.RootNode.Guid = GUID.Generate().ToString();
                _nodeGraph.AddNode(_nodeGraph.RootNode);
            }
            
            _nodeGraph.Nodes.ForEach(n => CreateAndAddNodeView(n));
            
            _nodeGraph.Nodes.ForEach(n =>
            {
                if (n is IntermediateNode intermediateNode)
                {
                    NodeView parentView = FindNodeView(n);
                    for (int i = 0; i < intermediateNode.children.Count; i++)
                    {
                        NodeView childView = FindNodeView(intermediateNode.children[i]);
                        Edge edge = parentView.Inputs[i].ConnectTo(childView.Output);
                        AddElement(edge);
                    }
                }
                else if (n is ResultNode rootNode)
                {
                    if (rootNode.Child != null)
                    {
                        NodeView parentView = FindNodeView(n);
                        NodeView childView = FindNodeView(rootNode.Child);
                        Edge edge = parentView.Inputs[0].ConnectTo(childView.Output);
                        AddElement(edge);
                    }
                }
            });
            
            if (nodeGraph.NodeGroups.Count > 0)
            {
                for (int i = 0; i < nodeGraph.NodeGroups.Count; i++)
                {
                    DSGroup dsGroup = nodeGraph.NodeGroups[i];
                    Group group = new Group();
                    dsGroup.GroupInstance = group;
                    group.title = dsGroup.Title;
                    group.SetPosition(new Rect(dsGroup.Position, Vector2.zero));
                    AddElement(group);
                    _nodeGraph.TempGroupDictionary.Add(group,dsGroup);

                    for (int j = 0; j < dsGroup.GroupedNodes.Count; j++)
                    {
                        CodeFunctionNode groupedNode = dsGroup.GroupedNodes[j];
                        NodeView nodeView = FindNodeView(groupedNode);
                        if (nodeView != null)
                        {
                            if (!group.ContainsElement(nodeView))
                            {
                                group.AddElement(nodeView);
                                groupedNode.Group = group;
                                Debug.Log("2");
                            }
                        }
                    }
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    if (element is NodeView nodeView)
                    {
                        _nodeGraph.DeleteNode(nodeView.Node);
                    }
                    else if (element is Edge edge)
                    {
                        NodeView parentView = edge.input.node as NodeView;
                        NodeView childView = edge.output.node as NodeView;
                        _nodeGraph.RemoveChild(parentView.Node, childView.Node, edge.input.portName);
                    }
                    else if (element is Group)
                    {
                        
                        
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.input.node as NodeView;
                    NodeView childView = edge.output.node as NodeView;
                    _nodeGraph.AddChild(parentView.Node, childView.Node, edge.input.portName);
                });
            }

            return graphViewChange;
        }

        private void CreateAndAddNodeView(CodeFunctionNode codeFunctionNode)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type =>  
                typeof(NodeView).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToArray();

            foreach (Type type in types)
            {
                if (type.GetCustomAttributes(typeof(NodeType), false) is NodeType[] attrs && attrs.Length > 0)
                {
                    if (attrs[0].Type == codeFunctionNode.GetType())
                    {
                        NodeView nodeView = (NodeView) Activator.CreateInstance(type);
                        nodeView.NodeGraphView = this;
                        nodeView.NodeGraph = _nodeGraph;
                        nodeView.Node = codeFunctionNode;
                        nodeView.Node.Group = null;
                        nodeView.viewDataKey = codeFunctionNode.Guid;
                        nodeView.style.left = codeFunctionNode.Position.x;
                        nodeView.style.top = codeFunctionNode.Position.y;
                        AddNodeView(nodeView);
                        nodeView.OnCreated();
                    }
                }
            }
            
            onGraphPopulated?.Invoke();
        }

        internal void AddNodeView(NodeView nodeView)
        {
            nodeView.onNodeSelected = onNodeSelected;
            AddElement(nodeView);
        }

        private NodeView FindNodeView(CodeFunctionNode node)
        {
            return GetNodeByGuid(node.Guid) as NodeView;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endport => endport.direction != startPort.direction&& endport.node != startPort.node).ToList();
        }
    }
}
#endif