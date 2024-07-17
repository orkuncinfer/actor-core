#if UNITY_EDITOR
/*using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using AudioSystem;
using Rowlan.Tools.QuickNav;
using UnityEngine.Audio;

[CustomPropertyDrawer(typeof(SoundData))]
public class SoundDataDrawer : OdinValueDrawer<SoundData>
{
    private bool foldout = true;
    private bool showDetails = false;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        SoundData value = this.ValueEntry.SmartValue;
        var parentAsset = this.ValueEntry.WeakSmartValue as ScriptableObject;

        foldout = SirenixEditorGUI.Foldout(foldout, label, style: SirenixGUIStyles.Foldout);

        if (foldout)
        {
            SirenixEditorGUI.BeginBox();
            // Draw the basic properties
            EditorGUI.BeginChangeCheck();
            value.clip = (AudioClip)SirenixEditorFields.UnityObjectField("Clip", value.clip, typeof(AudioClip), true);
            value.mixerGroup = (AudioMixerGroup)SirenixEditorFields.UnityObjectField("Mixer Group", value.mixerGroup, typeof(AudioMixerGroup), true);
            value.loop = EditorGUILayout.Toggle("Loop", value.loop);
            value.playOnAwake = EditorGUILayout.Toggle("Play On Awake", value.playOnAwake);
            value.frequentSound = EditorGUILayout.Toggle("Frequent Sound", value.frequentSound);

            // Draw the toggle for showing/hiding details
            showDetails = EditorGUILayout.Toggle("Show Details", showDetails);

            if (showDetails)
            {
                // Draw the details properties
                SirenixEditorGUI.BeginIndentedVertical();

                value.mute = EditorGUILayout.Toggle("Mute", value.mute);
                value.bypassEffects = EditorGUILayout.Toggle("Bypass Effects", value.bypassEffects);
                value.bypassListenerEffects = EditorGUILayout.Toggle("Bypass Listener Effects", value.bypassListenerEffects);
                value.bypassReverbZones = EditorGUILayout.Toggle("Bypass Reverb Zones", value.bypassReverbZones);

                value.priority = EditorGUILayout.IntSlider("Priority", value.priority, 0, 256);
                value.volume = EditorGUILayout.Slider("Volume", value.volume, 0f, 1f);
                value.pitch = EditorGUILayout.Slider("Pitch", value.pitch, -3f, 3f);
                value.panStereo = EditorGUILayout.Slider("Pan Stereo", value.panStereo, -1f, 1f);
                value.spatialBlend = EditorGUILayout.Slider("Spatial Blend", value.spatialBlend, 0f, 1f);
                value.reverbZoneMix = EditorGUILayout.Slider("Reverb Zone Mix", value.reverbZoneMix, 0f, 1.1f);
                value.dopplerLevel = EditorGUILayout.Slider("Doppler Level", value.dopplerLevel, 0f, 5f);
                value.spread = EditorGUILayout.Slider("Spread", value.spread, 0f, 360f);

                value.minDistance = EditorGUILayout.FloatField("Min Distance", value.minDistance);
                value.maxDistance = EditorGUILayout.FloatField("Max Distance", value.maxDistance);

                value.ignoreListenerVolume = EditorGUILayout.Toggle("Ignore Listener Volume", value.ignoreListenerVolume);
                value.ignoreListenerPause = EditorGUILayout.Toggle("Ignore Listener Pause", value.ignoreListenerPause);

                value.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("Rolloff Mode", value.rolloffMode);

                SirenixEditorGUI.EndIndentedVertical();
            }
            SirenixEditorGUI.EndBox();
        }
        
        this.ValueEntry.SmartValue = value;
    }

    protected override void Initialize()
    {
        base.Initialize();
        foldout = false;
        showDetails = false;
    }
}*/
#endif