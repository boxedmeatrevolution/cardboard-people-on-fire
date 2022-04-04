using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;



    void Start() {
    }

    void Awake() {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
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
                float newVolume = s.source.volume + Time.deltaTime * s.fadeSpeed;
                if ((s.fadeSpeed > 0 && newVolume > s.targetVolume) || newVolume < s.targetVolume)
                {
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
        s.source.Play();
    }

    public void FadeVolume(string name, float targetVolume, float overTime) 
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.isFade = true;
        s.targetVolume = targetVolume;
        s.fadeSpeed = (targetVolume - s.source.volume) / overTime;
        s.elapsedTime = 0f;
    }
}
