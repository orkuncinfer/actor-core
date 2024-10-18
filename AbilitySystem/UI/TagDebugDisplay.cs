using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TagDebugDisplay : MonoBehaviour
{
    /*private Camera _camera;
    private TextMeshProUGUI _text;
    public TagController TagController;
    private bool _initialized;

    public void Initialize(TagController tagController)
    {
        TagController = tagController;
        _initialized = true;
    }

    private void Awake()
    {
        _camera = Camera.main;
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!_initialized)
            return;
        transform.forward = _camera.transform.forward;

        string tagText = "";
        foreach (GameplayTag tag in TagController._gameplayTags)
        {
            tagText += tag.FullTag + "\n";
        }
        
        _text.text = tagText;
    }*/
}
