using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousWalkerController : MonoBehaviour
{
	public float speed = 1.0f;
	public float rot_speed = 40.0f;
	// 0: person. 1: cat. 2: dog. 3: seagull.
	public int type = 0;

    private float gravity = 20.0f;
	private Vector3 velocity_plane;
	private Vector3 real_velocity_plane;
	private float velocity_y;
    private CharacterController character_controller;
	private Swivel swivel;
	private GameObject player;
	private float swivel_distance;

	private int state = 0;
	private Flammable flammable;

	private float time_to_start_react = 1.0f;
	private float time_to_stop_react = 2.0f;
	private float react_timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        character_controller = GetComponent<CharacterController>();
		flammable = GetComponentInChildren<Flammable>();
		swivel = GetComponent<Swivel>();
		swivel.enabled = false;
		player = GameObject.Find("Player");
		if (type == 0) {
			speed = 1.2f;
			time_to_start_react = 3.0f;
			time_to_stop_react = 1.0f;
			swivel_distance = 5.0f;
		} else if (type == 1) {
			speed = 1.0f;
			time_to_start_react = 2.2f;
			time_to_stop_react = 1.2f;
			swivel_distance = 4.0f;
		} else if (type == 2) {
			speed = 1.8f;
			time_to_start_react = 1.0f;
			time_to_stop_react = 0.6f;
			swivel_distance = 8.0f;
		} else if (type == 3) {
			speed = 0.5f;
			time_to_start_react = 1.3f;
			time_to_stop_react = 2.0f;
			swivel_distance = 3.0f;
		}
    }

	void OnParticleCollision(GameObject obj) {
		flammable.Splash();
	}

    // Update is called once per frame
    void Update()
    {
        if (!character_controller.isGrounded) {
			velocity_y -= gravity * Time.deltaTime;
        } else {
			velocity_y = 0;
		}

		if (state == -1) {
			react_timer += Time.deltaTime;
			if (react_timer > time_to_start_react) {
				swivel.enabled = false;
				state = 4;
				react_timer = 0.0f;
			}
		}

		// Distance to player.
		float distance = Vector3.Distance(transform.position, player.transform.position);
		if (distance < swivel_distance) {
			if (state != -1 && state != 4) {
				swivel.enabled = true;
				react_timer = 0.0f;
				velocity_plane.x = 0.0f;
				velocity_plane.z = 0.0f;
				state = -1;
			}
		} else if (state == -1) {
			swivel.enabled = false;
			state = 0;
		}

		// When not near the player, NPCs either stand, walk, or turn, with some probability of entering each state.
		if (state == 0) {
			velocity_plane.x = 0.0f;
			velocity_plane.z = 0.0f;
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
			velocity_plane.x = speed * dir.x;
			velocity_plane.z = speed * dir.z;
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
			velocity_plane.x = 0.0f;
			velocity_plane.z = 0.0f;
			int sign = (state == 2 ? 1 : -1);
			transform.Rotate(0.0f, rot_speed * Time.deltaTime, 0.0f);
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 0;
			}
			if (UnityEngine.Random.Range(0.0f, 1.0f) >= Mathf.Exp(-Time.deltaTime / 3.0f)) {
				state = 1;
			}
		} else if (state == 4) {
			react_timer += Time.deltaTime;
			if (react_timer > time_to_stop_react) {
				state = 2;
			}
			if (type == 0) {
				velocity_plane.x = 0.0f;
				velocity_plane.z = 0.0f;
			} else if (type == 1) {
				Vector3 displacement = new Vector3(
					player.transform.position.x - transform.position.x,
					player.transform.position.z - transform.position.z,
					0.0f);
				displacement.Normalize();
				Vector3 direction3 = transform.TransformVector(Vector3.forward);
				Vector3 direction = new Vector2(direction3.x, direction3.z);
				direction.Normalize();
				float angle_between = -Mathf.Asin(Vector3.Cross(displacement, direction).z) * 180.0f / Mathf.PI;
				float angle_diff = 20.0f * Mathf.Sign(angle_between) * (1 - Mathf.Exp(-Time.deltaTime / (0.4f * swivel.turn_time)));
				transform.Rotate(0.0f, angle_diff, 0.0f);
				velocity_plane = transform.position - player.transform.position;
				velocity_plane.y = 0.0f;
				velocity_plane.Normalize();
				velocity_plane *= 3 * speed;
			} else if (type == 2) {
				velocity_plane = transform.forward;
				velocity_plane.Normalize();
				velocity_plane *= 2 * speed;
			} else if (type == 3) {
				Vector3 displacement = new Vector3(
					player.transform.position.x - transform.position.x,
					player.transform.position.z - transform.position.z,
					0.0f);
				displacement.Normalize();
				Vector3 direction3 = transform.TransformVector(Vector3.forward);
				Vector3 direction = new Vector2(direction3.x, direction3.z);
				direction.Normalize();
				float angle_between = -Mathf.Asin(Vector3.Cross(displacement, direction).z) * 180.0f / Mathf.PI;
				float angle_diff = 20.0f * Mathf.Sign(angle_between) * (1 - Mathf.Exp(-Time.deltaTime / (0.3f * swivel.turn_time)));
				transform.Rotate(0.0f, angle_diff, 0.0f);
				velocity_plane = transform.position - player.transform.position;
				if (react_timer < 0.3f || (react_timer > 0.6f && react_timer < 0.9f) || (react_timer > 1.2f && react_timer < 1.5f)) {
					velocity_y = 3.1f;
				}
				velocity_plane.Normalize();
				velocity_plane *= 5 * speed;
			}
		}

		real_velocity_plane = velocity_plane + Mathf.Exp(-Time.deltaTime / 0.3f) * (real_velocity_plane - velocity_plane);
        character_controller.Move(new Vector3(real_velocity_plane.x, velocity_y, real_velocity_plane.z) * Time.deltaTime);
    }
}
