using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSoundCollider : MonoBehaviour
{
    public string clipName;
    public float targetVolume = 1f;
    public float overTime = 2.5f;

    AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerStay(Collider other)
    {
        audioManager.currBgSound = clipName;
    }

    private void OnTriggerExit(Collider other)
    {
        audioManager.currBgSound = "city-music";
    }
}
