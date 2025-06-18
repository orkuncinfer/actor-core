using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Oddworm.Framework;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;

public class FootPlacement : MonoBehaviour
{
    [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightFoot;
    [SerializeField] private Transform _leftFootTarget;
    [SerializeField] private Transform _rightFootTarget;
    [SerializeField] private float _fadeOutSpeed;
    string leftFoot = "LeftFoot";
    string rightFoot = "RightFoot";
    
    private Animator _animator;
    
    private bool _rightFootPlaced;
    private bool _lefttFootPlaced;
    private FullBodyBipedIK _ik;

    private IKEffector _currentEffector;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        AnimancerComponent animancerComponent = GetComponent<AnimancerComponent>();
        _ik = GetComponent<FullBodyBipedIK>();
    }

    private void Update()
    {
        DbgDraw.WireSphere(_rightFoot.position,Quaternion.identity, Vector3.one * 0.3f, Color.red);
        DbgDraw.WireSphere(_leftFoot.position,Quaternion.identity, Vector3.one *  0.3f, Color.red);

        if (_rightFootPlaced)
        {
            RightFootPlaced();
            _rightFootPlaced = false;
            _ik.solver.rightFootEffector.positionWeight =1;
        }else if (_lefttFootPlaced)
        {
            LeftFootPlaced();
            _lefttFootPlaced = false;
            _ik.solver.leftFootEffector.positionWeight =1;
        }
        
        if(_currentEffector != null)
        {
            if (_currentEffector == _ik.solver.rightFootEffector)
            {
                _ik.solver.leftFootEffector.positionWeight -= Time.deltaTime * _fadeOutSpeed;
            }
            else if(_currentEffector == _ik.solver.leftFootEffector)
            {
                _ik.solver.rightFootEffector.positionWeight -= Time.deltaTime * _fadeOutSpeed;
            }
        }
    }
    private void LeftFootPlaced()
    {
        _currentEffector = _ik.solver.leftFootEffector;
        _leftFootTarget.position = _leftFoot.position;
        _ik.solver.leftFootEffector.positionWeight =1;
    }

    [Button]
    public void RightFootPlaced()
    {
        _currentEffector = _ik.solver.rightFootEffector;
        _rightFootTarget.position = _rightFoot.position;
        _ik.solver.rightFootEffector.positionWeight =1;
    }
    public void AnimEvent(AnimationEvent evt)
    {
        string eventName = evt.stringParameter;
        if(evt.animatorClipInfo.weight < 0.5f)
        {
            return;
        }
        if (eventName.EndsWith("_Start", StringComparison.OrdinalIgnoreCase))
        {
            int index = eventName.IndexOf("_Start", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                string newEventName = eventName.Substring(0, index);
                if(newEventName == leftFoot)
                {
                    Debug.Log("Left Foot Start");
                    _lefttFootPlaced = true;
                }
                else if(newEventName == rightFoot)
                {
                    Debug.Log("Right Foot Start");
                    _rightFootPlaced = true;
                }
            }
        }
        
        if (eventName.EndsWith("_End", StringComparison.OrdinalIgnoreCase))
        {
            int index = eventName.IndexOf("_End", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                string newEventName = eventName.Substring(0, index);
                if(newEventName == leftFoot)
                {
                    Debug.Log("Left Foot End");
                }
                else if(newEventName == rightFoot)
                {
                    Debug.Log("Right Foot End");
                }
            }
        }
    }
}
