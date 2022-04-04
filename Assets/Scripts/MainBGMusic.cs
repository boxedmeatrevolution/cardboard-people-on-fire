using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBGMusic : MonoBehaviour
{
    AudioManager am;
    void Start()
    {
        am = FindObjectOfType<AudioManager>();
        am.Play("city-music");
        am.Play("waveloop");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
