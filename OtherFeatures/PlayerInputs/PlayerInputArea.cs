using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputArea : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private PlayerInputDefinition _playerInputDefinition;
    private bool _dragging;
    private Vector2 _initialTouchPosition;
    private Vector2 _lastMousePosition;
    

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        _initialTouchPosition = eventData.position; // Store initial touch position
        _lastMousePosition = _initialTouchPosition; // Initialize the last position
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
        _playerInputDefinition.MoveInput = Vector2.zero;
    }

    private void Update()
    {
        if (_dragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            if (currentMousePosition == _lastMousePosition)
            {
                _playerInputDefinition.MoveInput = Vector2.zero;
                return;
            }
            Vector2 deltaPosition = currentMousePosition - _lastMousePosition;
            _lastMousePosition = currentMousePosition;
            _playerInputDefinition.MoveInput = deltaPosition;
        }
        else
        {
            _playerInputDefinition.MoveInput = Vector2.zero;
        }
    }
}
