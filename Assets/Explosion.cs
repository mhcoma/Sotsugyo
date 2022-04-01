using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	ParticleSystem sprite_particle;
	ParticleSystem spark_particle;

	ParticleSystem.MainModule sprite_main;
	ParticleSystem.MainModule spark_main;

	float[] initial_values = {5.0f, 0.1f, 2.0f, 1.0f, 0.0f, 15.0f};
	bool is_setted_values = false;

	float damage_time = 0.0f;
	public float damage = 50.0f;

	float damage_range = 1.0f;
	SphereCollider colid;
	List<int> damaged_objects = new List<int>();

	void Start() {
		sprite_particle = transform.GetChild(0).GetComponent<ParticleSystem>();
		spark_particle = transform.GetChild(1).GetComponent<ParticleSystem>();

		sprite_main = sprite_particle.main;
		spark_main = spark_particle.main;
		
		if (is_setted_values) {
			sprite_main.startSizeMultiplier = initial_values[0];
			spark_main.startSizeMultiplier = initial_values[1];
			sprite_main.startLifetimeMultiplier = initial_values[2];
			spark_main.startLifetimeMultiplier = initial_values[3];
			sprite_main.startSpeedMultiplier = initial_values[4];
			spark_main.startSpeedMultiplier = initial_values[5];
		}

		colid = GetComponent<SphereCollider>();
	}

	void Update() {
		if (!(sprite_particle.isPlaying || spark_particle.isPlaying)) {
			Destroy(gameObject);
		}
		damage_time += Time.deltaTime;
	}

	void OnTriggerEnter(Collider other) {
		if (damage_time < 1.0f) {
			if (other.transform.CompareTag("Actor")) {
				int index = other.GetInstanceID();
				if (damaged_objects.FindIndex(x => x == index) == -1) {
					damaged_objects.Add(index);
					float dist = (2.0f - (Vector3.Distance(other.transform.position, transform.position) / damage_range));
					float final_damage = dist * damage;
					other.transform.gameObject.GetComponent<SpriteObject>().get_damage(final_damage);
					Debug.Log(other.transform.gameObject.GetComponent<SpriteObject>().health);
				}
			}
		}
	}

	public void set_size(float sprite_multiplier, float spark_multiplier) {
		initial_values[0] = sprite_multiplier;
		initial_values[1] = spark_multiplier;
		if (sprite_particle != null) {
			sprite_main.startSizeMultiplier = initial_values[0];
			spark_main.startSizeMultiplier = initial_values[1];
		}
		else {
			is_setted_values = true;
		}
	}

	public void set_lifetime(float sprite_multiplier, float spark_multiplier) {
		initial_values[2] = sprite_multiplier;
		initial_values[3] = spark_multiplier;
		if (sprite_particle != null) {
			sprite_main.startLifetimeMultiplier = initial_values[2];
			spark_main.startLifetimeMultiplier = initial_values[3];
		}
		else {
			is_setted_values = true;
		}
	}

	public void set_speed(float sprite_multiplier, float spark_multiplier) {
		initial_values[4] = sprite_multiplier;
		initial_values[5] = spark_multiplier;
		if (sprite_particle != null) {
			sprite_main.startSpeedMultiplier = initial_values[4];
			spark_main.startSpeedMultiplier = initial_values[5];
		}
		else {
			is_setted_values = true;
		}
	}

	public void set_damage_range(float range) {
		damage_range = range;
		colid.radius = range;
	}
}
