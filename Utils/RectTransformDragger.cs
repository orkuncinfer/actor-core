using UnityEngine;
using UnityEngine.EventSystems;

public class RectTransformDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform _target;

    private Vector2 _offset;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_target, eventData.position, eventData.pressEventCamera, out _offset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_target.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Vector2 targetPosition = localPoint - _offset;
            _target.localPosition = ClampToScreen(_target, targetPosition);
        }
    }

    private Vector2 ClampToScreen(RectTransform target, Vector2 targetPosition)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("ClampToScreen method currently supports only ScreenSpaceOverlay canvas render mode.");
            return targetPosition;
        }

        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 targetSize = target.sizeDelta;

        float minX = -canvasSize.x / 2 + targetSize.x / 2;
        float maxX = canvasSize.x / 2 - targetSize.x / 2;
        float minY = -canvasSize.y / 2 + targetSize.y / 2;
        float maxY = canvasSize.y / 2 - targetSize.y / 2;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        return targetPosition;
    }
}