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
	public bool auto_on = true;
	public GameObject effect;

	private static List<Flammable> flammables = new List<Flammable>();
    void OnEnable()
    {
        flammables.Add(this);
		if (flammables.Count == 1 && auto_on) {
			Enflame();
		}
		else
		{ 
			Extinguish();
		}    
    }

    void OnDisable()
    {
        flammables.Remove(this);
		Extinguish();
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
				Enflame();
			}
		} else if (!on_fire) {
			heat -= Time.deltaTime / cooldown_time;
		}

		heat = Mathf.Clamp(heat, 0.0f, 1.0f);

		if (on_fire && heat == 0) {
			Extinguish();
        }

		if (on_fire) {
			Health health = transform.parent.gameObject.GetComponent<Health>();
			if (health != null) {
				health.hp -= Time.deltaTime;
			}
		}
    }

	public void Splash() {
		heat -= 0.04f;
		if (heat < 0.0f) {
			heat = 0.0f;
		}
		if (on_fire) {
			Extinguish();
		}
    }

	public void Enflame() {
		on_fire = true;
		heat = 1f;
		effect.GetComponent<ParticleSystem>().Play();
		effect.GetComponent<Light>().enabled = true;
	}

	public void Extinguish() {
		on_fire = false;
		heat = 0f;
		effect.GetComponent<ParticleSystem>().Stop();
		effect.GetComponent<Light>().enabled = false;
	}
}

