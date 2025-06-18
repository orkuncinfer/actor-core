using System.Collections;
using System.Collections.Generic;
using Core;
using StatSystem;
using UnityEngine;

public class State_HealthManaChangeDisplay : MonoState
{
    private StatController _statController;
    [SerializeField] private GameObject _floatingTextPrefab;

    [SerializeField] private Color _healColor;
    [SerializeField] private Color _damageColor;

    private Service_GAS _gas;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gas = Owner.GetService<Service_GAS>();
        _statController = _gas.StatController;
        if(_statController.Stats.TryGetValue("Health", out Stat healthStat))
        {
            if (healthStat is Attribute attribute)
            {
                attribute.onAttributeChanged += OnHealthChanged;
            }
            
        }
        if(_statController.Stats.TryGetValue("Mana", out Stat manaStat))
        {
            if (manaStat is Attribute attribute)
            {
                attribute.onAttributeChanged += OnManaChanged;
            }
        }
    }

    private void OnManaChanged(int arg1, int arg2)
    {
        GameObject instance = PoolManager.SpawnObject(_floatingTextPrefab,Owner.transform.position+ Vector3.up,Quaternion.identity);
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
        GameObject instance = PoolManager.SpawnObject(_floatingTextPrefab,Owner.transform.position + Vector3.up * 2,Quaternion.identity);
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
