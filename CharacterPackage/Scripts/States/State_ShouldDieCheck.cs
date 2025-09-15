using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_ShouldDieCheck : MonoState
{
    private Data_Living _livingData;
    private Service_GAS _gas;

    protected override void OnEnter()
    {
        base.OnEnter();
        _livingData = Owner.GetData<Data_Living>();
        _gas = Owner.GetService<Service_GAS>();
        
        _gas.StatController.GetAttribute("Health").onAttributeChanged += OnHealthChanged;

        if (_gas.StatController.GetAttribute("Health").CurrentValue <= 0)
        {
            _livingData.ShouldDieTrigger = true;
        }
    }

    protected override void OnExit()
    {
        base.OnExit();
        _gas.StatController.GetAttribute("Health").onAttributeChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float arg1, float arg2)
    {
        //Debug.Log($"HealthChnged {Owner.transform.name} = " + arg2 + $"Real Stat = {_gas.StatController.GetStat("Health").Value}");
        
        if (arg2 <= 0)
        {
            _livingData.ShouldDieTrigger = true;
        }
    }
}
