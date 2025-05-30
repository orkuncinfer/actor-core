using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class CanvasManager : PersistentSingleton<CanvasManager>
{
    [ShowInInspector][HideInEditorMode]private readonly Dictionary<string,CanvasLayer> _layerRegistry = new Dictionary<string, CanvasLayer>();
    [ShowInInspector][HideInEditorMode]private CanvasLayer _defaultLayer;
    
    [ShowInInspector] public List<PanelActor> PanelStack = new List<PanelActor>();
    
    // Add these methods to maintain stack-like behavior when needed
    public void PushPanel(PanelActor panel)
    {
        PanelStack.Add(panel); // Add to end (top of stack)
    }
    
    public PanelActor PopPanel()
    {
        if (PanelStack.Count == 0)
            return null;
            
        int lastIndex = PanelStack.Count - 1;
        PanelActor panel = PanelStack[lastIndex];
        PanelStack.RemoveAt(lastIndex);
        return panel;
    }
    
    public PanelActor PeekPanel()
    {
        if (PanelStack.Count == 0)
            return null;
            
        return PanelStack[PanelStack.Count - 1];
    }
    
    public int GetPanelCount()
    {
        return PanelStack.Count;
    }
    
    public void RegisterLayer(string layerTag, CanvasLayer layer, bool isDefault = false)
    {
        if (_layerRegistry.ContainsKey(layerTag))
        {
            Debug.LogError($"Layer with tag {layerTag} already exists");
            return;
        }
        if (isDefault)
        {
            _defaultLayer = layer;
        }
        else
        {
            _layerRegistry.Add(layerTag,layer);
        }
    }
    
    public GameObject ShowPanel(string panelId, string layerTag = "Default")
    {
        return GetDesiredLayer(layerTag).ShowPanel(panelId);
    }
    
    public void HidePanel(string panelId, string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).HidePanel(panelId);
    }
    
    public GameObject ShowAdditive(string panelId,string layerTag = "Default")
    {
        return GetDesiredLayer(layerTag).ShowAdditive(panelId);
    }
    
    public GameObject ShowAdditive(PanelActor panelActor, string layerTag = "Default")
    {
        return GetDesiredLayer(layerTag).ShowAdditive(panelActor);
    }

    public void HideLastPanel(string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).HideLastPanel();
    }
    public void SetParent(PanelActor child, string parentPanelTag,string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).GetPanelInstance(parentPanelTag).SetParentOf(child);
    }

    public CanvasLayer GetDesiredLayer(string layerTag)
    {
        if (_layerRegistry.ContainsKey(layerTag))
        {
            return _layerRegistry[layerTag];
        }
        if (_defaultLayer != null && layerTag == "Default")
        {
            return _defaultLayer;
        }
        Debug.LogError($"Could not find layer with tag {layerTag} or default layer is not set.");
        return null;
    }
}
