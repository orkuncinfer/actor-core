using System.Collections;
using System.Collections.Generic;
using Animancer;
using ECM2;
using UnityEngine;

public class State_BlendToIdleAnim : MonoState
{
    public float VelocityThreshold;
    public float BlendDuration;
    
    private AnimancerComponent _animancerComponent;
    private Character _character;

    public string CurrentState;
    protected override void OnEnter()
    {
        base.OnEnter();
        _character = Owner.GetComponent<Character>();
        _animancerComponent = Owner.GetComponentInChildren<AnimancerComponent>();
        
        StaticUpdater.onUpdate += StaticUpdate;
    }

    protected override void OnExit()
    {
        base.OnExit();
        StaticUpdater.onUpdate -= StaticUpdate;
    }

    private void StaticUpdate()
    {
        CurrentState = _animancerComponent.States.Current.ToString();
    }
    
    private bool IsAnimationPlaying(AnimationClip[] clips, out float highestWeight)
    {
        highestWeight = 0;
        var states = _animancerComponent.States.Current;
        var enumerator = states.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var animancerState = enumerator.Current;
            for (int i = 0; i < clips.Length; i++)
            {
                if (animancerState.Clip == clips[i] && animancerState.IsPlaying)
                {
                    if (animancerState.Weight > highestWeight)
                    {
                        highestWeight = animancerState.Weight;
                    }
                }
            }
        }
        return highestWeight > 0.12f;
    }
}
