using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(false, null, null, "Data_Animancer")]
public class Data_Animancer : Data
{
    [SerializeField] private AnimancerComponent _animancerComponent;
    public AnimancerComponent AnimancerComponent => _animancerComponent;
    
    [SerializeField] private AnimancerController _animancerController;
    public AnimancerController AnimancerController => _animancerController;
    
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;
}
