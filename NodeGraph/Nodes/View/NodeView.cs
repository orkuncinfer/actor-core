using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Editor
{
    public class NodeView : Node
    {
        public CodeFunctionNode Node;
        public List<Port> Inputs = new List<Port>();
        public Port Output;

        public Action<NodeView> onNodeSelected;
        public TextField ValueField;
        public NodeGraphView NodeGraphView;
        public NodeGraph NodeGraph;
        public bool ShowLabel = true;
        
        public NodeView()
        {
            this.RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(menuEvent => menuEvent.menu.AppendAction("DetachFromParent", actionEvent => DetachFromGroup(actionEvent)));
            return contextualMenuManipulator;
        }

        private void DetachFromGroup(DropdownMenuAction actionEvent)
        {
            Node.Group.RemoveElement(this);
            NodeGraph.TempGroupDictionary[Node.Group].GroupedNodes.Remove(Node);
            Node.Group = null;
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Node.onValueValidate -= OnValueValidate;
        }
        public void OnCreated()
        {
            if(Node is ResultNode) return;
            ValueField = new TextField("");
           
            if (Node is IntermediateNode)
            {
                ValueField.isReadOnly = true;
                ValueField.focusable = false;
                ValueField.SetEnabled(false);
                ValueField.style.backgroundColor = new StyleColor(Color.gray);
            }
            if(!ShowLabel)return;
            var q = ValueField.Q("unity-text-input");
            q.visible = ShowLabel;
            q.style.fontSize = 18;
            q.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
           
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/actor-core/NodeGraph/NodeGraphEditorWindow.uss"); // Replace with the actual path
            ValueField.styleSheets.Add(styleSheet);
            if (ShowLabel)
            {
                ValueField.AddToClassList("myTextFieldStyle");
            }
            
            
            if (Node.Value % 1 != 0) 
            {
                ValueField.value = Node.Value.ToString("F2"); 
            }
            else
            {
                ValueField.value = Node.Value.ToString();
            }
            
            
            if (Node is IntermediateNode)
            {
                q.style.color = new StyleColor(Color.green);
                q.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            }else if (Node is StatNode statNode)
            {
                ValueField.style.width = new StyleLength(120);
                ValueField.value = statNode.StatName;
            }else if (Node is AbilityLevelNode abilityLevelNode)
            {
                ValueField.style.width = new StyleLength(120);
                ValueField.value = abilityLevelNode.AbilityName;
            }
          
            
            ValueField.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out float newValue))
                {
                    Node.Value = newValue; 
                }

                if (Node is StatNode statNode)
                {
                    statNode.StatName = evt.newValue;
                }
                if (Node is AbilityLevelNode abilityLevelNode)
                {
                    abilityLevelNode.AbilityName = evt.newValue;
                }
            });
            Node.onValueValidate += OnValueValidate;
            extensionContainer.Add(ValueField);
            RefreshExpandedState();
            RefreshPorts();
        }
        

        protected Port CreateOutputPort(string portName = "", Port.Capacity capacity = Port.Capacity.Single)
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(float));
            outputPort.portName = portName;
            outputContainer.Add(outputPort);
            RefreshPorts();
            
           
            return outputPort;
        }
        

        private void OnValueValidate(float newValue)
        {
            //ValueField.value = newValue.ToString("F2"); 
            if (newValue % 1 != 0)  // Check if the number is fractional
            {
                ValueField.value = newValue.ToString("F2");  // Format with two decimal places
            }
            else
            {
                ValueField.value = newValue.ToString();
            }
        }

        protected Port CreateInputPort(string portName = "", Port.Capacity capacity = Port.Capacity.Single)
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, typeof(float));
            inputPort.portName = portName;
            inputContainer.Add(inputPort);
            RefreshPorts();
            return inputPort;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onNodeSelected?.Invoke(this);
        }

        public override void SetPosition(Rect newPosition)
        {
            base.SetPosition(newPosition);
            Node.Position.x = newPosition.xMin;
            Node.Position.y = newPosition.yMin;
            EditorUtility.SetDirty(Node);
        }
    }
}

