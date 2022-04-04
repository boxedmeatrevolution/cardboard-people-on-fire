using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Approachable : MonoBehaviour
{   
    GameObject player;
    public bool isApproached = false;
    public float approachDistance = 10f;
    public float deproachDistance = 15f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (player) 
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (!isApproached && distance < approachDistance) {
                isApproached = true;
                OnApproach();
            }
            else if (isApproached && distance > deproachDistance) {
                isApproached = false;
            }

        }
        else if (player && !isApproached)
        {
            isApproached = false;
        }
    }

    public virtual void OnApproach()
    {
        Debug.Log("Approached");
    }
}
