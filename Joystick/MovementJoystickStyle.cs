using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedOnScreenControls;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MovementJoystickStyle : MonoBehaviour
{
    [SerializeField] private EnhancedOnScreenStick _stick;

    [SerializeField] private Image _handle;
    [SerializeField] private Image _background;
    [SerializeField] private Image _background2;

    [SerializeField] private Color _pointerDownBgAlpha;
    private Color _defaultColor;
    private void Start()
    {
        _stick.onPointerDown += OnPointerDown;
        _stick.onPointerUp += OnPointerUp;
        _defaultColor = _background2.color;
        _handle.enabled = false;
    }

    private void OnPointerUp()
    {
        _handle.enabled = false;
        
        _background.color = _defaultColor;
        _background2.color = _defaultColor;
    }

    private void OnPointerDown()
    {
        _handle.enabled = true;
        _background2.color = _pointerDownBgAlpha;
        _background.color = _pointerDownBgAlpha;
    }
}
