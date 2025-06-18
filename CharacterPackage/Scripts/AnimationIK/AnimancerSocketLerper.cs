using System;
using System.Collections.Generic;
using Animancer;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling; // Add this namespace for Profiler API

[Serializable]
public class SocketLerpInfo
{
    public Transform _lerpTransform;
    public AnimationClip[] _lerpClips;
    public Vector2 _weightRange;
    public Vector3 LerpPosition;
    public Vector3 LerpRotation;
    public float LerpSpeed = 3f;
    public Vector3 _defaultPosition;
    public Vector3 _defaultRotation;
    [ReadOnly]public float RemappedWeight;
}

public class AnimancerSocketLerper : MonoBehaviour
{
    public List<SocketLerpInfo> _socketLerpInfos;
    [SerializeField] private AnimancerComponent _animancer;

    private void Start()
    {
        Profiler.BeginSample("SocketLerper Start Initialization");

        int count = _socketLerpInfos.Count;
        for (int i = 0; i < count; i++)
        {
            var socketLerpInfo = _socketLerpInfos[i];
            socketLerpInfo._defaultPosition = socketLerpInfo._lerpTransform.localPosition;
            socketLerpInfo._defaultRotation = socketLerpInfo._lerpTransform.localEulerAngles;
        }

        Profiler.EndSample();
    }

    private void LateUpdate()
    {
        Profiler.BeginSample("SocketLerper Update");

        int count = _socketLerpInfos.Count;
        bool anyPlaying = false;
        for (int i = 0; i < count; i++)
        {
            Profiler.BeginSample("Lerping Character");
            var socketLerpInfo = _socketLerpInfos[i];
            float weight;
            if (IsAnimationPlaying(socketLerpInfo, out weight))
            {
                float remappedWeight = MathUtils.StepRemap(0, 1, socketLerpInfo._weightRange.x, socketLerpInfo._weightRange.y, weight);
                socketLerpInfo.RemappedWeight = remappedWeight;
                Vector3 targetPosition = socketLerpInfo.LerpPosition;
                Vector3 targetRotation = socketLerpInfo.LerpRotation;
                Vector3 currentPos = socketLerpInfo._lerpTransform.localPosition;
                Vector3 currentRot = socketLerpInfo._lerpTransform.localEulerAngles;

                if (currentPos != targetPosition || currentRot != targetRotation)
                {
                    socketLerpInfo._lerpTransform.localPosition = Vector3.Lerp(currentPos, targetPosition, remappedWeight * Time.deltaTime * socketLerpInfo.LerpSpeed);
                    socketLerpInfo._lerpTransform.localRotation = Quaternion.Lerp(socketLerpInfo._lerpTransform.localRotation, Quaternion.Euler(targetRotation), remappedWeight * Time.deltaTime * socketLerpInfo.LerpSpeed);
                }
                Profiler.EndSample(); // End "Remap Weight and Lerp"
            }
            else
            {
                Vector3 currentPosition = socketLerpInfo._lerpTransform.localPosition;
                Vector3 currentRotation = socketLerpInfo._lerpTransform.localEulerAngles;

                if (currentPosition != socketLerpInfo._defaultPosition || currentRotation != socketLerpInfo._defaultRotation)
                {
                    socketLerpInfo._lerpTransform.localPosition = Vector3.Lerp(currentPosition, socketLerpInfo._defaultPosition, Time.deltaTime * socketLerpInfo.LerpSpeed);
                    socketLerpInfo._lerpTransform.localRotation = Quaternion.Lerp(socketLerpInfo._lerpTransform.localRotation, Quaternion.Euler(socketLerpInfo._defaultRotation), Time.deltaTime * socketLerpInfo.LerpSpeed);
                }
            }
            Profiler.EndSample(); // End "Lerping Character"
        }

        Profiler.EndSample(); // End "SocketLerper Update"
    }

    private bool IsAnimationPlaying(SocketLerpInfo lerpInfo, out float highestWeight)
    {
        Profiler.BeginSample("Check Animation Playing");
        highestWeight = 0;
        var states = _animancer.States.Current;
        if(states == null) return false;
        var enumerator = states.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var animancerState = enumerator.Current;
            for (int i = 0; i < lerpInfo._lerpClips.Length; i++)
            {
                if (animancerState.Clip == lerpInfo._lerpClips[i] && animancerState.IsPlaying)
                {
                    if (animancerState.Weight > highestWeight)
                    {
                        highestWeight = animancerState.Weight;
                    }
                }
            }
        }

        Profiler.EndSample(); // End "Check Animation Playing"
        return highestWeight > lerpInfo._weightRange.x;
    }
}
