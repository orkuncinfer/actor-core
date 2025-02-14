using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCenter : PersistentSingleton<AudioCenter>
{
    public AudioSource Source;

    public void Play(AudioClip clip, float volume = 1f,bool randomizePitch = false)
    {
        Source.PlayOneShot(clip,volume);

        float randomPitch = Random.Range(0.85f, 1.1f);
        if (randomizePitch)
        {
            Source.pitch = randomPitch;
        }
    }
}
