using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            (cos * v.x) - (sin * v.y),
            (sin * v.x) + (cos * v.y)
        );
    }
}

public class CrosshairController : MonoBehaviour
{
    [FormerlySerializedAs("t")] 
    public CrosshairTemplate crosshairDefinition;
    
    public RectTransform[][] crosshairParts;
    public float spread;
    public float SpreadMultiplier = 1;
    
    [HideInInspector] public Color color;
    [HideInInspector] public CrosshairElement[] elements;

    private Camera _mainCamera;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateCrosshairParts();
    }

    public void SetSpread(float newSpread)
    {
        spread = newSpread;
        UpdateCrosshairParts();
    }

    private float HorizontalFOV()
    {
        float radAngle = _mainCamera.fieldOfView * Mathf.Deg2Rad;
        return Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * _mainCamera.aspect);
    }

    public static Vector2 MaskFromEffects(EffectAxis effect)
    {
        return new Vector2(effect.HasFlag(EffectAxis.X) ? 1 : 0, effect.HasFlag(EffectAxis.Y) ? 1 : 0);
    }

    public void UpdateCrosshairParts()
    {
        float unitsPerDegree = Screen.width / HorizontalFOV();
        
        for (int i = 0; i < elements.Length; i++)
        {
            CrosshairElement element = elements[i]; // Cache element reference
            Vector2 scale = element.size;

            if (element.scaleWithSpread.HasFlag(EffectAxis.X))
                scale.x *= spread * SpreadMultiplier * unitsPerDegree;
            if (element.scaleWithSpread.HasFlag(EffectAxis.Y))
                scale.y *= spread * SpreadMultiplier * unitsPerDegree;

            Vector2 offset = element.offset;
            offset += MaskFromEffects(element.offsetFromCenter) * scale.y * 0.5f;

            if (element.offsetWithSpread)
            {
                offset += element.spreadOffsetDirection * spread * SpreadMultiplier * unitsPerDegree;
            }

            float directionRotation = -element.orbitStartAngle;
            float angleIncrease = element.orbitSize / element.count;

            for (int j = 0; j < element.count; j++)
            {
                directionRotation += angleIncrease;
                RectTransform crosshairPart = crosshairParts[i][j];

                crosshairPart.anchoredPosition = offset.Rotate(directionRotation);
                crosshairPart.rotation = Quaternion.Euler(0, 0, directionRotation - element.rotationOffset);
                crosshairPart.sizeDelta = scale;

                if (crosshairDefinition.elements[i].customPrefab == null)
                {
                    if (crosshairPart.TryGetComponent(out Image img))
                    {
                        img.color = element.overrideColor ? element.color : color;
                        img.sprite = element.sprite;
                    }
                }
            }
        }
    }

    public T GetComponentInCrosshair<T>() where T : MonoBehaviour
    {
        foreach (var layer in crosshairParts)
        {
            foreach (var part in layer)
            {
                if (part.TryGetComponent(out T comp))
                    return comp;
            }
        }
        return null;
    }

    public T GetComponentInCrosshairLayer<T>(int layer) where T : MonoBehaviour
    {
        foreach (var part in crosshairParts[layer])
        {
            if (part.TryGetComponent(out T comp))
                return comp;
        }
        return null;
    }

    public void SyncElementsToTemplate()
    {
        elements = crosshairDefinition.elements;
        color = crosshairDefinition.color;
    }

    private void Start()
    {
        SyncElementsToTemplate();
        int elementsLength = elements.Length;
        crosshairParts = new RectTransform[elementsLength][];

        for (int i = 0; i < elementsLength; i++)
        {
            CrosshairElement element = elements[i];
            crosshairParts[i] = new RectTransform[element.count];

            for (int j = 0; j < element.count; j++)
            {
                GameObject go;
                if (element.customPrefab != null)
                {
                    go = Instantiate(element.customPrefab);
                }
                else
                {
                    go = new GameObject("Crosshair part", typeof(RectTransform), typeof(Image));

                    if (go.TryGetComponent(out Image img))
                    {
                        img.color = element.overrideColor ? element.color : crosshairDefinition.color;
                        img.sprite = element.sprite;
                    }
                }

                Transform goTransform = go.transform;
                goTransform.SetParent(transform, false);

                crosshairParts[i][j] = go.GetComponent<RectTransform>();
            }
        }

        UpdateCrosshairParts();
    }
}
