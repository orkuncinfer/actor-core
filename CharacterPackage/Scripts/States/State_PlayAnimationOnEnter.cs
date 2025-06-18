using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PlayAnimationOnEnter : MonoState
{
    private Data_Animancer _dataAnimancer;
    
    [SerializeField] private AnimationClip _animationClip;

    protected override void OnEnter()
    {
        base.OnEnter();
        
        if (_animationClip)
        {
            _dataAnimancer = Owner.GetData<Data_Animancer>();
            _dataAnimancer.AnimancerComponent.Play(_animationClip);
        }
            
    }
}
