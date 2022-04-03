using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swivel : MonoBehaviour
{
	public float turn_time = 0.3f;
	private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
		if (player != null) {
			Vector3 displacement = new Vector3(
				player.transform.position.x - transform.position.x,
				player.transform.position.z - transform.position.z,
				0.0f);
			displacement.Normalize();
			Vector3 direction3 = transform.TransformVector(Vector3.forward);
			Vector3 direction = new Vector2(direction3.x, direction3.z);
			direction.Normalize();
			float angle_between = -Mathf.Asin(Vector3.Cross(displacement, direction).z) * 180.0f / Mathf.PI;
			float angle_diff = angle_between * (1 - Mathf.Exp(-Time.deltaTime / turn_time));
			transform.Rotate(0.0f, -angle_diff, 0.0f);
		}
    }
}
