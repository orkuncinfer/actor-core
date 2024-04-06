using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Core.Editor
{
    [CreateAssetMenu(fileName = "NodeGraph", menuName = "Core/NodeGraph", order = 0)]
    public class NodeGraph : ScriptableObject
    {
        public CodeFunctionNode RootNode;
        public List<CodeFunctionNode> Nodes = new List<CodeFunctionNode>();
        public List<DSGroup> NodeGroups = new List<DSGroup>();
        public Dictionary<Group, DSGroup> TempGroupDictionary = new Dictionary<Group, DSGroup>();

        public float CalculateValue(GameObject source)
        {
            return RootNode.CalculateValue(source);
        }
        
        public void AddNode(CodeFunctionNode node)
        {
            Nodes.Add(node);
            AssetDatabase.AddObjectToAsset(node,this);
            NodeGraphHelper.SOSaveCache.Add(this);
        }

        public void AddGroup(DSGroup group)
        {
            NodeGroups.Add(group);
            AssetDatabase.AddObjectToAsset(group,this);
            NodeGraphHelper.SOSaveCache.Add(this);
        }

        public void DeleteGroup(DSGroup group)
        {
            NodeGroups.Remove(group);
            AssetDatabase.RemoveObjectFromAsset(group);
            NodeGraphHelper.SOSaveCache.Add(this);
        }
        
        public void DeleteNode(CodeFunctionNode node)
        {
            Nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            NodeGraphHelper.SOSaveCache.Add(this);
        }

        public void RemoveChild(CodeFunctionNode parent, CodeFunctionNode child, string portName)
        {
            if (parent is IntermediateNode intermediateNode)
            {
                intermediateNode.RemoveChild(child, portName);
                EditorUtility.SetDirty(intermediateNode);
            }
            else if (parent is ResultNode resultNode)
            {
                resultNode.Child = null;
                EditorUtility.SetDirty(resultNode);
            }
        }

        public void AddChild(CodeFunctionNode parent, CodeFunctionNode child, string portName)
        {
            if (parent is IntermediateNode intermediateNode)
            {
                intermediateNode.AddChild(child, portName);
                EditorUtility.SetDirty(intermediateNode);
            }
            else if (parent is ResultNode resultNode)
            {
                resultNode.Child = child;
                EditorUtility.SetDirty(resultNode);
            }
        }

        public List<T> FindNodesOfType<T>()
        {
            List<T> nodesOfType = new List<T>();
            foreach (CodeFunctionNode node in Nodes)
            {
                if (node is T nodeType)
                {
                    nodesOfType.Add(nodeType);
                }
            }
            return nodesOfType;
        }
    }
}