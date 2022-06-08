using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteObject : MonoBehaviour {

	public Transform camera_transform;
	private Transform sprite_box_transform;
	private Transform sprite_transform;
	private Transform shadow_mesh_transform;
	private SpriteRenderer sprite_renderer;
	private Rigidbody rigid;
	private CapsuleCollider colid;

	public string[] directories;
	public int sprite_side_count;
	public List<Sprite[]> sprites = new List<Sprite[]>();
	public int anim = 0;

	public float health = 100;
	public bool is_alive = true;
	float dead_anim = 0;
	float dead_anim_speed = 1f;

	public GameObject explosion;

	public bool is_gas_explosion = false;

	public bool is_ai_object;

	EnemyAITest ai;
	NonAIObject nonai;

	
	AudioSource asrc;
	public AudioClip dead_clip;


	void Start() {
		rigid = GetComponent<Rigidbody>();
		colid = GetComponent<CapsuleCollider>();

		sprite_box_transform = transform.GetChild(0);
		sprite_transform = sprite_box_transform.GetChild(0);
		sprite_renderer = sprite_transform.GetComponent<SpriteRenderer>();

		shadow_mesh_transform = transform.GetChild(1);

		foreach (string directory in directories) {
			sprites.Add(Resources.LoadAll<Sprite>(directory + "/diffuse"));
		}
		
		is_ai_object = TryGetComponent<EnemyAITest>(out ai);

		asrc = GetComponent<AudioSource>();
	}

	void Update() {
		if (Time.timeScale > 0) {
			Vector3 angle = transform.eulerAngles;

			Vector3 camera_plane_position = camera_transform.position;
			camera_plane_position.y = 0;
			Vector3 plane_position = transform.position;
			plane_position.y = 0;

			float direction_angle_y = Quaternion.LookRotation((camera_plane_position - plane_position).normalized).eulerAngles.y;
			int angle_index = ((sprite_side_count * 2) - Mathf.RoundToInt((direction_angle_y - angle.y) / 45)) % sprite_side_count;
			sprite_box_transform.rotation = Quaternion.Euler(0, direction_angle_y + 180.0f, 0);
			sprite_renderer.sprite = sprites[anim][angle_index];

			if (!is_alive) {
				if (dead_anim <= 0) {
					GameObject explosion_object = Instantiate(explosion, transform.position, Quaternion.identity);
					Explosion explosion_explosion = explosion_object.GetComponent<Explosion>();
					asrc.PlayOneShot(dead_clip);
					if (!is_gas_explosion) {
						explosion_explosion.set_size(2.0f, 0.1f);
						explosion_explosion.set_lifetime(1.0f, 0.25f);
						explosion_explosion.knockback_power = 100;
					}
					else {
						explosion_explosion.set_size(2.0f, 0.1f);
						explosion_explosion.set_lifetime(1.0f, 0.75f);
						explosion_explosion.set_speed(0.0f, 7.5f);
						explosion_explosion.damage = 50.0f;
						explosion_explosion.knockback_power = 2000f;
					}
				}

				dead_anim += Time.deltaTime * dead_anim_speed;
				sprite_renderer.material.SetFloat("_NoisePower", dead_anim);

				if (dead_anim >= 1) {
					Destroy(gameObject);
				}
			}
		}
	}

	public void get_damage(float damage) {
		health -= damage;
		if (health <= 0) {
			is_alive = false;
			if (is_ai_object) ai.die();
			rigid.isKinematic = true;
			colid.enabled = false;
			shadow_mesh_transform.gameObject.SetActive(false);
		}
	}
}
