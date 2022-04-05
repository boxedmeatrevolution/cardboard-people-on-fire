using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;
    public float fadeSpeed = 1f / 5f;

    public string currBgSound = "city-music";

    bool jinglep1 = false;
    bool jinglep2 = false;
    bool maint = false;

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
        Sound j1 = Array.Find(sounds, sound => sound.name == "j1");
        Sound j2 = Array.Find(sounds, sound => sound.name == "j2");
        if (!jinglep1) {
            jinglep1 = true;
            j1.source.Play();
        }
        else if (!jinglep2 && !j1.source.isPlaying)
        {
            jinglep2 = true;
            j2.source.Play();
        }
        else if (!maint && !j2.source.isPlaying)
        {
            maint = true;
            Array.Find(sounds, sound => sound.name == "city-music").source.Play();
        }

        foreach (Sound s in sounds) 
        { 
            if ((s.name == currBgSound && s.source.volume != s.defaultVolume) || s.source.volume != s.offVolume) {
                float targetVolume = s.name == currBgSound ? s.defaultVolume : s.offVolume;
                float dir = s.source.volume < targetVolume ? 1 : -1;
                float newVolume = s.source.volume + Time.deltaTime * fadeSpeed * dir;
                float newDir = newVolume < targetVolume ? 1 : -1;

                if (newDir != dir) { 
                    newVolume = targetVolume;
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

    public void PlayOneShot(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.PlayOneShot(s.clip);
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }
}
