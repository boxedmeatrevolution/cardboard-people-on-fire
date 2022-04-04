using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundEmitter : Approachable
{
    public float freq = 4f;
    public float rand = 2.5f;
    public bool onApproach = true;
    public AudioClip[] clips;
    public AudioSource source;

    float elapsed = 0f;
    float nextSound;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        ScheduleNextSound();
    }

    // Update is called once per frame
    public override void Update()
    {
        //base.Update();
        elapsed += Time.deltaTime;
        if (elapsed > nextSound) {
            PlayRandomSound();
            elapsed = 0f;
            ScheduleNextSound();
        }
    }

    public override void OnApproach()
    {
        PlayRandomSound();
    }

    void PlayRandomSound()
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(clip);
    }

    void ScheduleNextSound()
    {
        nextSound = freq + Random.Range(-rand / 2f, rand / 2f);
    }
}
