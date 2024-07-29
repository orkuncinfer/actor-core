using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_SetTimeScale : MonoState
{
    [SerializeField] private float _timeScale = 0;
    [SerializeField] private bool _returnDefaultOnExit = true;
    
    private float _defaultTimeScale;
    protected override void OnEnter()
    {
        base.OnEnter();
        _defaultTimeScale = Time.timeScale;
        Time.timeScale = _timeScale;
    }

    protected override void OnExit()
    {
        base.OnExit();
        if (_returnDefaultOnExit)
        {
            Time.timeScale = _defaultTimeScale;
        }
    }
}
