using System;
using Core;
using FishNet.Object;
using StatSystem;
using UnityEngine;
using Attribute = StatSystem.Attribute;

public class Network_HealthChangeDisplayer : NetworkBehaviour
{
    [SerializeField] private GameObject _floatingTextPrefab;

    [SerializeField] private Color _healColor;
    [SerializeField] private Color _damageColor;
    
    private StatController _statController;

    private StatSystem.Attribute _healthAttribute;
    private StatSystem.Attribute _manaAttribute;
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if(!base.Owner.IsLocalClient) return;
        _statController = GetComponentInChildren<StatController>();
        if(_statController.Stats.TryGetValue("Health", out Stat healthStat))
        {
            if (healthStat is Attribute attribute)
            {
                _healthAttribute = attribute;
                _healthAttribute.onAttributeChanged += OnHealthChanged;
            }
            
        }
        if(_statController.Stats.TryGetValue("Mana", out Stat manaStat))
        {
            if (manaStat is Attribute attribute)
            {
                _manaAttribute = attribute;
                _manaAttribute.onAttributeChanged += OnManaChanged;
            }
        }
    }

    private void OnDestroy()
    {
        if(!base.Owner.IsLocalClient) return;
        if (_healthAttribute != null)
            _healthAttribute.onAttributeChanged -= OnHealthChanged;
        if (_manaAttribute != null)
            _manaAttribute.onAttributeChanged -= OnManaChanged;
    }

    private void OnManaChanged(int arg1, int arg2)
    {
        GameObject instance = PoolManager.SpawnObject(_floatingTextPrefab,transform.position+ Vector3.up,Quaternion.identity);
        FloatingText floatingText = instance.GetComponent<FloatingText>();

        int value = arg2 - arg1;
        if (value > 0)
        {
            floatingText.Set("+" + value.ToString(),Color.blue);
            floatingText.Animate();
        }
        else
        {
            floatingText.Set( value.ToString(),Color.blue);
            floatingText.Animate();
        }
    }

    private void OnHealthChanged(int arg1, int arg2)
    {
        GameObject instance = PoolManager.SpawnObject(_floatingTextPrefab,transform.position + Vector3.up * 2,Quaternion.identity);
        FloatingText floatingText = instance.GetComponent<FloatingText>();

        int value = arg2 - arg1;
        if (value > 0)
        {
            floatingText.Set("+" + value.ToString(),_healColor);
            floatingText.Animate();
        }
        else
        {
            floatingText.Set(value.ToString(),_damageColor);
            floatingText.Animate();
        }
    }
}
