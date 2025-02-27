#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


public class RectTransformContextMenu
{
    [MenuItem("CONTEXT/RectTransform/Set Anchors To Rect", false, 151)]
    static void SetEasyAnchors()
    {
        var objs = Selection.gameObjects;

        foreach (var o in objs)
        {
            if (o != null && o.GetComponent<RectTransform>() != null)
            {
                var r = o.GetComponent<RectTransform>();
                var p = o.transform.parent.GetComponent<RectTransform>();

                var offsetMin = r.offsetMin;
                var offsetMax = r.offsetMax;
                var _anchorMin = r.anchorMin;
                var _anchorMax = r.anchorMax;

                var parent_width = p.rect.width;
                var parent_height = p.rect.height;

                var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                    _anchorMin.y + (offsetMin.y / parent_height));
                var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                    _anchorMax.y + (offsetMax.y / parent_height));

                UnityEditor.Undo.RecordObject(r, "SetAnchorsToPivot");
                r.anchorMin = anchorMin;
                r.anchorMax = anchorMax;

                r.offsetMin = new Vector2(0, 0);
                r.offsetMax = new Vector2(0, 0);
                r.pivot = new Vector2(0.5f, 0.5f);
                EditorUtility.SetDirty(r);
            }
        }
    }

    [MenuItem("CONTEXT/RectTransform/Set Anchors To Pivot", false, 151)]
    static void SetAnchorsToPivot()
    {
        var objs = Selection.gameObjects;

        foreach (var o in objs)
        {
            if (o != null && o.GetComponent<RectTransform>() != null)
            {
                var rectTransform = o.GetComponent<RectTransform>();
                var parentRectTransform = o.transform.parent.GetComponent<RectTransform>();

                Rect rect = rectTransform.rect;

                var offsetMin = rectTransform.offsetMin;
                var offsetMax = rectTransform.offsetMax;
                var _anchorMin = rectTransform.anchorMin;
                var _anchorMax = rectTransform.anchorMax;
                var sizeDelta = rectTransform.sizeDelta;

                var parent_width = parentRectTransform.rect.width;
                var parent_height = parentRectTransform.rect.height;

                var anchorMin = new Vector2((rectTransform.localPosition.x + parent_width / 2) / parent_width,
                    (rectTransform.localPosition.y + parent_height / 2) / parent_height);
                var anchorMax = new Vector2((rectTransform.localPosition.x + parent_width / 2) / parent_width,
                    (rectTransform.localPosition.y + parent_height / 2) / parent_height);
                UnityEditor.Undo.RecordObject(rectTransform, "SetAnchorsToPivot");
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;

                rectTransform.anchoredPosition = Vector3.zero;
                rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                EditorUtility.SetDirty(rectTransform);
            }
        }
    }

    [MenuItem("CONTEXT/RectTransform/Fit To Children Size", false, 152)]
    static void AdjustParentToFitChildren()
    {
        var objs = Selection.gameObjects;

        foreach (var o in objs)
        {
            if (o != null && o.TryGetComponent(out RectTransform parentRectTransform))
            {
                if (parentRectTransform.childCount == 0)
                    return;

                RectTransform[] children = new RectTransform[parentRectTransform.childCount];

                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

                for (int i = 0; i < parentRectTransform.childCount; i++)
                {
                    children[i] = parentRectTransform.GetChild(i) as RectTransform;

                    Vector3[] childCorners = new Vector3[4];
                    children[i].GetWorldCorners(childCorners);

                    for (int j = 0; j < 4; j++)
                    {
                        min = Vector3.Min(min, childCorners[j]);
                        max = Vector3.Max(max, childCorners[j]);
                    }
                }

                // Step 1: Calculate the exact center of all children
                Vector3 center = (min + max) / 2f;

                // Step 2: Unparent all children
                foreach (var child in children)
                {
                    child.SetParent(null, worldPositionStays: true);
                }

                // Step 3: Move the parent to the computed center
                Undo.RecordObject(parentRectTransform, "Adjust Parent To Fit Children");
                parentRectTransform.position = center;

                // Step 4: Resize the parent to match the bounding box
                Vector3 newMin = parentRectTransform.InverseTransformPoint(min);
                Vector3 newMax = parentRectTransform.InverseTransformPoint(max);
                Vector2 newSize = new Vector2(newMax.x - newMin.x, newMax.y - newMin.y);

                parentRectTransform.sizeDelta = newSize;

                // Step 5: Reparent children back
                foreach (var child in children)
                {
                    child.SetParent(parentRectTransform, worldPositionStays: true);
                }

                EditorUtility.SetDirty(parentRectTransform);
            }
        }
    }
}

#endif