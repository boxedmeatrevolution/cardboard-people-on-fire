using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialPerson : Approachable
{

    AudioSource audioSource;
    public AudioClip voice1;
    public AudioClip voice2;
    public Flammable flame;
    public TMPro.TextMeshPro text;

    bool has_been_enflamed = false;
    bool has_been_extinguished = false;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = voice1;
    }

    public override void Update()
    {
        base.Update();

        if (has_been_enflamed && !has_been_extinguished && !flame.on_fire)
        {
            has_been_extinguished = true;
            audioSource.clip = voice2;
            audioSource.Play();
            text.text = "Suffering is inevitable.";

            Invoke(nameof(EndTutorial), 2);
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        flame.Splash();
    }

    public override void OnApproach()
    {
        audioSource.clip = voice1;
        audioSource.Play();
        Invoke(nameof(Flameohotmin), 1);
    }

    void Flameohotmin() {
        if (flame) 
        { 
            flame.Enflame();
            has_been_enflamed = true;
        }

        if (text)
        {
            text.text = "Use the Left Mouse Button to end all suffering.";
        }

    }

    void EndTutorial()
    {
        SceneManager.LoadScene("Island2"); 
    }
}
