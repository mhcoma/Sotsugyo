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
	public float knockback_power = 0.0f;
	public float knockback_radius = 10.0f;

	float damage_range = 2.0f;
	SphereCollider colid;
	List<int> damaged_objects = new List<int>();

	AudioSource asrc;

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
		
		asrc = GetComponent<AudioSource>();
	}

	void Update() {
		if (!(sprite_particle.isPlaying || spark_particle.isPlaying)) {
			Destroy(gameObject);
		}
		damage_time += Time.deltaTime;
	}

	void OnTriggerEnter(Collider other) {

		if (damage_time < 0.0625f) {
			if (other.transform.CompareTag("Actor") || other.transform.CompareTag("Player")) {
				int index = other.GetInstanceID();
				if (damaged_objects.FindIndex(x => x == index) == -1) {
					damaged_objects.Add(index);

					Vector3 temp1 = other.transform.position;
					temp1.y = 0;

					Vector3 temp2 = transform.position;
					temp2.y = 0;

					float dist = Mathf.Clamp(0.5f - (Vector3.Distance(temp1, temp2) / (damage_range * Mathf.Sqrt(2) * 2)), 0.0f, 1.0f);
					
					Rigidbody other_rigid = other.gameObject.GetComponent<Rigidbody>();
					SpriteObject other_spriteobj = other.transform.gameObject.GetComponent<SpriteObject>();
					EnemyAI other_ai = other.transform.gameObject.GetComponent<EnemyAI>();

					float final_damage = dist * damage;
					if (other.transform.CompareTag("Actor")) {
						other_spriteobj.get_damage(final_damage);
						if (other_spriteobj.is_ai_object) {
							other_ai.toggle_rigid(false);
							other.transform.position += new Vector3(0, 0.375f, 0);
						}
						other_rigid.AddExplosionForce(knockback_power, transform.position, knockback_radius, 1.0f);
					}
					else {
						Player player = other.transform.gameObject.GetComponent<Player>();
						player.get_damage(final_damage);
						
						if (player.is_alive())
							other_rigid.AddExplosionForce(knockback_power, transform.position, knockback_radius, 1.0f);
					}
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
