using UnityEngine;

public static class RectTransformExtensions
{

    public static bool Overlaps(this RectTransform a, RectTransform b) {
        return a.WorldRect().Overlaps(b.WorldRect());
    }
    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse) {
        return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
    }

    public static Rect WorldRect(this RectTransform rectTransform) {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        Vector3 position = rectTransform.position;
        return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
    }
    
    public static bool WorldRectOverlaps(RectTransform rectTransform1, RectTransform rectTransform2)
    {
        Rect rect1 = GetWorldRect(rectTransform1);
        Rect rect2 = GetWorldRect(rectTransform2);
        
        return rect1.Overlaps(rect2);
    }
    
    private static Rect GetWorldRect(RectTransform rectTransform)
    {
        // Array to store world corners: bottom-left, top-left, top-right, bottom-right
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Get the bottom-left and top-right corners to define the Rect
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];

        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        return new Rect(bottomLeft.x, bottomLeft.y, width, height);
    }
}