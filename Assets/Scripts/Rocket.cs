using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {
	ParticleSystem rocket_smoke;
	Collider rocket_collider;
	SpriteRenderer rocket_sprite;
	Light rocket_light;

	public Transform shooter;
	public GameObject explosion;

	Vector3 destination_pos;
	Vector3 direction;
	public static float speed = 20.0f;
	float this_speed;
	float damage = 10.0f;
	float explosion_damage = 10.0f;

	AudioSource asrc;

	bool launched = false;

	void Start() {
		rocket_smoke = GetComponent<ParticleSystem>();
		rocket_collider = GetComponent<Collider>();
		rocket_sprite = GetComponent<SpriteRenderer>();
		rocket_light = GetComponent<Light>();
		
		asrc = GetComponent<AudioSource>();

		this_speed = speed;
	}

	void Update() {
		if (rocket_smoke.isStopped) {
			Destroy(gameObject);
		}
		else {
			if (launched) {
				launched = false;
				asrc.Play();
			}
			transform.position += direction * Time.deltaTime * this_speed;
		}
	}

	void OnTriggerEnter(Collider other) {
		
		if (other.transform.gameObject == shooter.gameObject)
			return;
		
		if (other.transform.CompareTag("Actor")) {
			other.transform.gameObject.GetComponent<SpriteObject>().get_damage(damage);
		}
		else if (other.transform.CompareTag("Player")) {
			other.transform.gameObject.GetComponent<Player>().get_damage(damage);
		}

		transform.position -= direction * 0.5f;

		GameObject explosion_object = Instantiate(explosion, transform.position, Quaternion.identity);
		Explosion explosion_explosion = explosion_object.GetComponent<Explosion>();
		explosion_explosion.set_size(2.0f, 0.1f);
		explosion_explosion.set_lifetime(1.0f, 0.75f);
		explosion_explosion.set_speed(0.0f, 7.5f);
		explosion_explosion.knockback_power = 1000.0f;
		explosion_explosion.damage = explosion_damage;


		rocket_smoke.Stop(false);
		rocket_collider.enabled = false;
		rocket_sprite.enabled = false;

		this_speed = 0.0f;
		rocket_light.enabled = false;
	}

	public void launch(Vector3 start_pos, Vector3 dest_pos, Transform shooter, float rocket_damage, float explosion_damage) {
		destination_pos = dest_pos;
		transform.position = start_pos;
		this.shooter = shooter;
		this.damage = rocket_damage;
		this.explosion_damage = explosion_damage;
		direction = (destination_pos - transform.position).normalized;
		launched = true;
	}
}