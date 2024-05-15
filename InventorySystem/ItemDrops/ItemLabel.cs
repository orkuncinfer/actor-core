using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemLabel : MonoBehaviour
{
    public RectTransform rectTransform; // Set this in the inspector or find it on start
    public Vector3 worldOffset = new Vector3(0, 1, 0); // Offset in the world space to position the label
    public Vector2 screenOffset = new Vector2(0, 50);

    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private TextMeshProUGUI textComponent;
    public float padding = 20f;
    private void Start()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        AdjustWidth();
    }

    private void UpdatePosition(Vector3 pos)
    {
        Vector3 worldPosition = transform.parent.position + worldOffset; // Assumes the parent is the item
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.localPosition = pos;
    }

    public void CheckOverlap(RectTransform other)
    {
        if (other == rectTransform) return; 
        if (RectTransformExtensions.Overlaps(rectTransform, other))
        {
            Debug.Log("overlap31");
            // Move the label down to avoid overlap
            screenOffset += new Vector2(0, rectTransform.rect.height);
            UpdatePosition(rectTransform.localPosition + (Vector3.up * other.sizeDelta.y));;
        }
    }
    [Button]
    void AdjustWidth()
    {
        // Force the text component to update its layout immediately
        textComponent.ForceMeshUpdate();

        // Get the rendered bounds of the text mesh
        var textBounds = textComponent.textBounds;

        // Update the sizeDelta to encapsulate the text plus padding
        textRectTransform.sizeDelta = new Vector2(textBounds.size.x + padding * 2, textRectTransform.sizeDelta.y);
    }
}