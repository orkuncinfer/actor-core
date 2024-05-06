using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class PanelModel
{
    [InlineButton("Show")]
    public string PanelId;
    [InlineButton("Add")]
    public GameObject PanelPrefab;
    
    public void Show()
    {
        CanvasLayer.Instance.ShowPanel(PanelId);
    }

    public void Add()
    {
        CanvasLayer.Instance.ShowAdditive(PanelId);
    }
}
