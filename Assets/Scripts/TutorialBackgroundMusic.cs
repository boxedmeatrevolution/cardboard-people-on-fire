using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBackgroundMusic : MonoBehaviour
{
    void Start() 
    {
        FindObjectOfType<AudioManager>().Play("waveloop");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
