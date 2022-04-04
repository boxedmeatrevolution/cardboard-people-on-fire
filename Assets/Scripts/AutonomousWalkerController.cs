using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousWalkerController : MonoBehaviour
{
	public float speed = 1.0f;
	public float rot_speed = 40.0f;

    private float gravity = 20.0f;
	private Vector3 velocity;
    private CharacterController character_controller;
	private Swivel swivel;
	private GameObject player;
	private float swivel_distance = 5.0f;

	private int state = 0;
	private Flammable flammable;

    // Start is called before the first frame update
    void Start()
    {
        character_controller = GetComponent<CharacterController>();
		flammable = GetComponentInChildren<Flammable>();
		swivel = GetComponent<Swivel>();
		swivel.enabled = false;
		player = GameObject.Find("Player");
    }

	void OnParticleCollision(GameObject obj) {
		flammable.Splash();
	}

    // Update is called once per frame
    void Update()
    {
        if (!character_controller.isGrounded) {
			velocity.y -= gravity * Time.deltaTime;
        } else {
			velocity.y = 0;
		}

		// Distance to player.
		float distance = Vector3.Distance(transform.position, player.transform.position);
		if (distance < swivel_distance) {
			if (!swivel.enabled) {
				swivel.enabled = true;
				velocity.x = 0.0f;
				velocity.z = 0.0f;
				state = 0;
			}
			return;
		} else if (swivel.enabled) {
			swivel.enabled = false;
		}

		// When not near the player, NPCs either stand, walk, or turn, with some probability of entering each state.
		if (state == 0) {
			velocity.x = 0.0f;
			velocity.z = 0.0f;
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 1;
			}
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				if (UnityEngine.Random.Range(0.0f, 1.0f) >= 0.5f) {
					state = 2;
				} else {
					state = 3;
				}
			}
		} else if (state == 1) {
			Vector3 dir = transform.forward;
			dir.y = 0.0f;
			dir.Normalize();
			velocity.x = speed * dir.x;
			velocity.z = speed * dir.z;
			bool collided = false;
			if ((character_controller.collisionFlags & CollisionFlags.Sides) != 0) {
				collided = true;
			}
			if ((character_controller.collisionFlags & CollisionFlags.Above) != 0) {
				collided = true;
			}
			if (collided || UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 0;
			}
			if (collided || UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				if (UnityEngine.Random.Range(0.0f, 1.0f) >= 0.5f) {
					state = 2;
				} else {
					state = 3;
				}
			}
		} else if (state == 2 || state == 3) {
			velocity.x = 0.0f;
			velocity.z = 0.0f;
			int sign = (state == 2 ? 1 : -1);
			transform.Rotate(0.0f, rot_speed * Time.deltaTime, 0.0f);
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 0;
			}
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 1;
			}
		}

        character_controller.Move(velocity * Time.deltaTime);
    }
}
