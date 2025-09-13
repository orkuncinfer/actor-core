using System;
using UnityEngine;

[Serializable]
public class DS_PurchasableItem : Data
{
    [SerializeField] private bool _isPersistent;

    [SerializeField] private AnimationCurve _costCurve;
    
    [SerializeField] private int _cost;

    public int GetCostForLevel(int level)
    {
        if (_costCurve != null)
        {
            return Mathf.RoundToInt(_costCurve.Evaluate(level) * _cost);
        }
        return _cost;
    }
}
