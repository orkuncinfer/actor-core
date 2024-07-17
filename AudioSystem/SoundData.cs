using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem {
    [Serializable]
    public class SoundData {
        [BoxGroup]
        public AudioClip clip;
        [BoxGroup]
        public AudioMixerGroup mixerGroup;
        [BoxGroup]
        public bool loop;
        [BoxGroup]
        public bool playOnAwake;
        [BoxGroup]
        public bool frequentSound;
        [BoxGroup]
        public bool showDetails;
        [BoxGroup]
        #region Details
        [ShowIf("showDetails")][BoxGroup]
        public bool mute;
        [ShowIf("showDetails")][BoxGroup]
        public bool bypassEffects;
        [ShowIf("showDetails")][BoxGroup]
        public bool bypassListenerEffects;
        [ShowIf("showDetails")][BoxGroup]
        public bool bypassReverbZones;
        [ShowIf("showDetails")][BoxGroup]
        public int priority = 128;
        [ShowIf("showDetails")][BoxGroup][Range(0f,1f)]
        public float volume = 1f;
        [ShowIf("showDetails")][BoxGroup]
        public float pitch = 1f;
        [ShowIf("showDetails")][BoxGroup]
        public float panStereo;
        [ShowIf("showDetails")][BoxGroup]
        public float spatialBlend;
        [ShowIf("showDetails")][BoxGroup]
        public float reverbZoneMix = 1f;
        [ShowIf("showDetails")][BoxGroup]
        public float dopplerLevel = 1f;
        [ShowIf("showDetails")][BoxGroup]
        public float spread;
        [ShowIf("showDetails")][BoxGroup]
        public float minDistance = 1f;
        [ShowIf("showDetails")][BoxGroup]
        public float maxDistance = 500f;
        [ShowIf("showDetails")][BoxGroup]
        public bool ignoreListenerVolume;
        [ShowIf("showDetails")][BoxGroup]
        public bool ignoreListenerPause;
        [ShowIf("showDetails")][BoxGroup]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        #endregion
    }
}

public struct PersonStruct
{
    public string Name;
    public int Age;
}