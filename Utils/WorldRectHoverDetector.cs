using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldRectHoverDetector : MonoBehaviour
{
    public LayerMask IgnoredUILayers; // Layers we ignore when hovering over UI

    private bool _isHovering = false;
    
    private Camera _camera;
    
    public event Action<RectTransform> OnHoverEnter;
    
    public event Action<RectTransform> OnHoverExit;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public bool CheckHover(RectTransform targetRectTransform)
    {
        bool isInside =
            RectTransformUtility.RectangleContainsScreenPoint(targetRectTransform, Input.mousePosition, _camera);
        
        bool isBlocked = IsBlocked();
        
        if (isInside && !isBlocked)
        {
            if (!_isHovering)
            {
                _isHovering = true;
                OnEnterHover(targetRectTransform);
            }
            return true;
        }
        else
        {
            if (_isHovering)
            {
                _isHovering = false;
                OnExitHover(targetRectTransform);
            }
        }

        return false;
    }
    private bool IsBlocked()
    {
        bool blocked = false;
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (((1 << result.gameObject.layer) & IgnoredUILayers) != 0)
            {
                
            }
            else
            {
                //Debug.Log("Blocking by layer: " + LayerMask.LayerToName(result.gameObject.layer));
                blocked = true;
            }
        }

        return blocked;
    }
    private bool IsMouseInsideScreenRect(RectTransform rectTransform, Vector2 mouseScreenPos)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]); // Bottom-left
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]); // Top-right

        Rect screenRect = new Rect(min, max - min);
        return screenRect.Contains(mouseScreenPos);
    }
    private void OnEnterHover(RectTransform rectTransform)
    {
        OnHoverEnter?.Invoke(rectTransform);
    }

    private void OnExitHover(RectTransform rectTransform)
    {
        OnHoverExit?.Invoke(rectTransform);
    }
}
