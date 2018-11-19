using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SoundCollection : ScriptableObject {

    public string name;
    public float volumeScale = 1.0f;
    public List<AudioClip> audioClips = new List<AudioClip>();

    private int index = 0;

    public AudioClip getRandomClip()
    {
        return audioClips[Random.Range(0, audioClips.Count)];
    }

    public AudioClip getNextClip()
    {
        AudioClip clip = audioClips[index];
        index = (index + 1) % audioClips.Count;
        return clip;
    }
}
