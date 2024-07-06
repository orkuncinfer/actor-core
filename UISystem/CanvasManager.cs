using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CanvasManager : Singleton<CanvasManager>
{
    [ShowInInspector][HideInEditorMode]private readonly Dictionary<string,CanvasLayer> _layerRegistry = new Dictionary<string, CanvasLayer>();
    [ShowInInspector][HideInEditorMode]private CanvasLayer _defaultLayer;
    
    [ShowInInspector] public Stack<PanelInstanceView> PanelStack = new Stack<PanelInstanceView>();
    
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
    
    public void ShowPanel(string panelId, string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).ShowPanel(panelId);
    }
    
    public void HidePanel(string panelId, string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).HidePanel(panelId);
    }
    
    public void ShowAdditive(string panelId,string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).ShowAdditive(panelId);
    }

    public void HideLastPanel(string layerTag = "Default")
    {
        GetDesiredLayer(layerTag).HideLastPanel();
    }

    private CanvasLayer GetDesiredLayer(string layerTag)
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
