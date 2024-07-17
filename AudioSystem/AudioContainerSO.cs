using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using Sirenix.OdinInspector;
using UnityEngine;

[SOCreatable][Searchable]
public class AudioContainerSO : ScriptableObject
{
    [FoldoutGroup("InGame")]
    public SoundData BalloonPop;
    [FoldoutGroup("UI")]
    public SoundData ButtonClick;
    [FoldoutGroup("UI")]
    public SoundData ButtonDown;
    [FoldoutGroup("UI")]
    public SoundData ButtonUp;
}
