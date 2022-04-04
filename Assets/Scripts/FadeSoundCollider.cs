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

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>())
        {
           audioManager.FadeVolume(clipName, targetVolume, overTime);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>())
        {
            audioManager.FadeVolume(clipName, 0f, overTime);
        }
    }
}
