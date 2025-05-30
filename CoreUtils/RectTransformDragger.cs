using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enables precise UI element dragging without any initial position snapping.
/// Ensures the element maintains its exact relation to the cursor throughout the drag.
/// </summary>
public class RectTransformDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform _target;
    [SerializeField] private bool _clampToScreen = true;
    [SerializeField] [Range(0f, 1f)] private float _edgePadding = 0f;
    
    private Canvas _canvas;
    private RectTransform _canvasRectTransform;
    private Vector2 _grabOffset; // Offset from pointer to rect's position
    
    private void Start()
    {
        // If no target is set, use this object's RectTransform
        if (_target == null)
        {
            _target = GetComponent<RectTransform>();
            if (_target == null)
            {
                Debug.LogError("RectTransformDragger requires a RectTransform component.");
                enabled = false;
                return;
            }
        }

        // Get canvas references
        _canvas = _target.GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("RectTransformDragger requires a Canvas parent.");
            enabled = false;
            return;
        }
        
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Captures the precise offset between the pointer and the object's position
    /// to prevent any unwanted snapping when dragging begins.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Convert screen point to world point
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _target.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPoint
        );
        
        // Calculate the offset between the pointer's world position and the target's position
        _grabOffset = _target.position - worldPoint;
    }

    /// <summary>
    /// Maintains the exact same offset between pointer and object throughout the drag
    /// to prevent any snapping or position jumping.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null || _target == null) return;

        // Convert screen point to world point
        Vector3 worldPoint;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _target.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPoint))
        {
            // Calculate new position by adding the preserved offset to the current world point
            Vector3 newPosition = worldPoint + (Vector3)_grabOffset;
            
            // Set position directly in world space to maintain exact position
            _target.position = newPosition;
            
            // Apply clamping if needed
            if (_clampToScreen)
            {
                // Convert to local space for clamping
                Vector3 localPos = _target.localPosition;
                Vector2 clampedPos = ClampPositionToCanvas(new Vector2(localPos.x, localPos.y));
                _target.localPosition = new Vector3(clampedPos.x, clampedPos.y, localPos.z);
            }
        }
    }

    /// <summary>
    /// Clamps the position to keep the element within canvas boundaries.
    /// </summary>
    private Vector2 ClampPositionToCanvas(Vector2 position)
    {
        if (_canvas == null || _canvasRectTransform == null) return position;

        // Get parent RectTransform
        RectTransform parentRectTransform = _target.parent as RectTransform;
        if (parentRectTransform == null) return position;

        // Get the rect dimensions
        Rect parentRect = parentRectTransform.rect;
        
        // Calculate the element's size in local space
        Vector2 targetSize = new Vector2(
            _target.rect.width * _target.localScale.x,
            _target.rect.height * _target.localScale.y
        );
        
        // Calculate offsets based on pivot
        float pivotOffsetX = targetSize.x * _target.pivot.x;
        float pivotOffsetY = targetSize.y * _target.pivot.y;
        
        // Edge padding
        float paddingX = parentRect.width * _edgePadding;
        float paddingY = parentRect.height * _edgePadding;
        
        // Calculate min/max bounds
        float minX = parentRect.xMin + pivotOffsetX + paddingX;
        float maxX = parentRect.xMax - (targetSize.x - pivotOffsetX) - paddingX;
        float minY = parentRect.yMin + pivotOffsetY + paddingY;
        float maxY = parentRect.yMax - (targetSize.y - pivotOffsetY) - paddingY;
        
        // Apply clamping
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
}