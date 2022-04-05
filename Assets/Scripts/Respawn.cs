using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{

    Transform[] spawners;

    // Start is called before the first frame update
    void Start()
    {
        spawners = GetComponentsInChildren<Transform>();
        Debug.Log("There are " + spawners.Length + " spawners");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform spawnPos = spawners[Random.Range(0, spawners.Length)];
        other.gameObject.transform.position = spawnPos.position;
    }

    private void OnTriggerStay(Collider other)
    {
        Transform spawnPos = spawners[Random.Range(0, spawners.Length)];
        other.gameObject.transform.position = spawnPos.position;
        
    }
}
