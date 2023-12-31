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

    public GameObject PanelPrefab;

    
    public void Show()
    {
        CanvasLayer.Instance.ShowPanel(PanelId);
    }
}
