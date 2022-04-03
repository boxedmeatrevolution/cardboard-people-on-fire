using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : MonoBehaviour
{
	public float heat = 0.0f;
	public float cooldown_time = 4.0f;
	public float heatup_time = 4.0f;
	public float threshold_distance = 3.0f;
	public bool on_fire = false;
	public GameObject effect;

	private static List<Flammable> flammables = new List<Flammable>();
	private GameObject my_flame;
    void OnEnable()
    {
        flammables.Add(this);
		if (flammables.Count == 1) {
			on_fire = true;
		}
		if (!on_fire) {
			effect.GetComponent<ParticleSystem>().Stop();
		} else {
			effect.GetComponent<ParticleSystem>().Play();
		}
    }
    void OnDisable()
    {
        flammables.Remove(this);
		effect.GetComponent<ParticleSystem>().Stop();
    }

    // Update is called once per frame
    void Update()
    {
		Flammable source = null;
		int count = 0;
		foreach (Flammable other in flammables) {
			if (other == this || !other.on_fire) {
				continue;
			}
			float distance = Vector3.Distance(other.transform.position, transform.position);
			if (distance < threshold_distance) {
				source = other;
				count += 1;
			}
		}
		if (source != null) {
			heat += count * Time.deltaTime / heatup_time;
			if (heat > 1.0f && !on_fire) {
				on_fire = true;
				effect.GetComponent<ParticleSystem>().Play();
			}
		} else {
			heat -= Time.deltaTime / cooldown_time;
		}

		heat = Mathf.Clamp(heat, 0.0f, 1.0f);

		if (on_fire) {
			Health health = transform.parent.gameObject.GetComponent<Health>();
			if (health != null) {
				health.hp -= Time.deltaTime;
			}
		}
    }
}

