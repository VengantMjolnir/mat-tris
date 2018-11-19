using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSelector : MonoBehaviour {
    private static SFXSelector _instance;
    public static SFXSelector Instance
    {
        get 
        {
            if (_instance == null){
                _instance = FindObjectOfType<SFXSelector>();
            }
            return _instance;
        }
    }

    private AudioSource _audioSource;

	// Use this for initialization
	void Start () {
        _instance = this;
        _audioSource = GetComponentInChildren<AudioSource>();
	}

    public void PlayRandomSound(SoundCollection collection, float overrideVolumeScale = 0f)
    {
        if (collection == null)
        {
            return;
        }
        float volumeScale = collection.volumeScale;
        if (overrideVolumeScale > 0)
        {
            volumeScale = overrideVolumeScale;
        }
        _audioSource.PlayOneShot(collection.getRandomClip(), volumeScale);
    }

    public void PlayNextSound(SoundCollection collection)
    {
        if (collection == null)
        {
            return;
        }
        _audioSource.PlayOneShot(collection.getNextClip(), collection.volumeScale);
    }
}
