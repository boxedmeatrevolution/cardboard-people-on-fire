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
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    public bool loop;

    [HideInInspector]
    public AudioSource source;


    [HideInInspector]
    public bool isFade = false;
    [HideInInspector]
    public float targetVolume = 0f;
    [HideInInspector]
    public float fadeSpeed = 0f;
    [HideInInspector]
    public float elapsedTime = 0f;
}
