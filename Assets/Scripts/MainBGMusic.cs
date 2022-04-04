using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBGMusic : MonoBehaviour
{
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("city-music");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
