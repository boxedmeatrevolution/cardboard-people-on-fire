using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;
    public float fadeSpeed = 1f / 5f;

    string currBgSound;



    void Start() {
    }

    void Awake() {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.defaultVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Update()
    {
        foreach (Sound s in sounds) 
        { 
            if (s.isFade) 
            {
                float dir = s.source.volume < s.targetVolume ? 1 : -1;
                float newVolume = s.source.volume + Time.deltaTime * fadeSpeed * dir;
                float newDir = newVolume < s.targetVolume ? 1 : -1;

                if (newDir != dir) { 
                    newVolume = s.targetVolume;
                    s.isFade = false;
                }
                s.source.volume = newVolume;
            }
        }
    }

    public void Play(string name) 
    {
        Debug.Log(name);
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s.startOff) {
            s.source.volume = s.offVolume;
        }
        s.source.Play();
    }
}
