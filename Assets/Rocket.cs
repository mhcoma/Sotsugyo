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
	float speed = 20.0f;
	float damage = 10.0f;

	AudioSource asrc;
	public AudioClip rocket_boom_clip;

	bool launched = false;

	void Start() {
		rocket_smoke = GetComponent<ParticleSystem>();
		rocket_collider = GetComponent<Collider>();
		rocket_sprite = GetComponent<SpriteRenderer>();
		rocket_light = GetComponent<Light>();
		
		asrc = GetComponent<AudioSource>();
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
			transform.position += direction * Time.deltaTime * speed;
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

		rocket_smoke.Stop(false);
		rocket_collider.enabled = false;
		rocket_sprite.enabled = false;

		speed = 0.0f;
		rocket_light.enabled = false;
		asrc.PlayOneShot(rocket_boom_clip);
	}

	public void launch(Vector3 start_pos, Vector3 dest_pos, Transform shooter) {
		destination_pos = dest_pos;
		transform.position = start_pos;
		this.shooter = shooter;
		direction = (destination_pos - transform.position).normalized;
		launched = true;
	}
}