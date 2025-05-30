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
    
    public static bool overlaps(float min1, float max1, float min2, float max2) {
        return isBetween(min2, min1, max1) || isBetween(min1, min2, max2);
        static bool isBetween(float v, float min, float max) => min <= v && v <= max;
    }
    
    /// <summary>
    /// Checks whether two RectTransforms overlap in screen space, accounting for their rotation and scaling.
    /// </summary>
    public static bool OverlapsInScreenSpace(this RectTransform a, RectTransform b, Camera camera)
    {
        Vector2[] quadA = GetScreenSpaceQuad(a, camera);
        Vector2[] quadB = GetScreenSpaceQuad(b, camera);

        return QuadsOverlapSAT(quadA, quadB);
    }

    /// <summary>
    /// Gets the 4 corner points of a RectTransform projected to screen space.
    /// </summary>
    private static Vector2[] GetScreenSpaceQuad(RectTransform rectTransform, Camera camera)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        Vector2[] screenCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, worldCorners[i]);
        }

        return screenCorners;
    }

    /// <summary>
    /// Checks overlap of two quads using Separating Axis Theorem (SAT).
    /// </summary>
    private static bool QuadsOverlapSAT(Vector2[] quadA, Vector2[] quadB)
    {
        return !HasSeparatingAxis(quadA, quadB) && !HasSeparatingAxis(quadB, quadA);
    }

    /// <summary>
    /// Checks if there's a separating axis between the two quads.
    /// </summary>
    private static bool HasSeparatingAxis(Vector2[] quadA, Vector2[] quadB)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 edge = quadA[(i + 1) % 4] - quadA[i];
            Vector2 axis = new Vector2(-edge.y, edge.x).normalized;

            float minA, maxA, minB, maxB;
            ProjectQuadOnAxis(quadA, axis, out minA, out maxA);
            ProjectQuadOnAxis(quadB, axis, out minB, out maxB);

            if (maxA < minB || maxB < minA)
                return true; // Separation found
        }

        return false; // No separating axis
    }

    /// <summary>
    /// Projects a quad onto an axis and finds the min and max scalar projections.
    /// </summary>
    private static void ProjectQuadOnAxis(Vector2[] quad, Vector2 axis, out float min, out float max)
    {
        min = max = Vector2.Dot(quad[0], axis);

        for (int i = 1; i < 4; i++)
        {
            float projection = Vector2.Dot(quad[i], axis);
            if (projection < min) min = projection;
            if (projection > max) max = projection;
        }
    }
}