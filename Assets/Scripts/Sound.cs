using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float defaultVolume;
    [Range(0f, 1f)]
    public float offVolume;
    [Range(.1f, 3f)]
    public float pitch;
    public bool startOff = true;
    public bool loop;

    [HideInInspector]
    public AudioSource source;


    [HideInInspector]
    public bool isFade = false;
    [HideInInspector]
    public float targetVolume = 0f;
}
