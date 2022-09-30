using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {
	Rigidbody rigid;

	Transform player_transform;
	public float finding_distance;
	public float stopping_distance;
	Player player;

	float ground_drag = 10.0f;
	float liquid_multiplier = 4.0f;
	float air_drag = 2.0f;
	
	float height = 2.0f;
	public Transform ground_check_transform;
	public LayerMask ground_mask;
	public LayerMask liquid_mask;
	bool is_grounded;
	bool is_liquided;
	float ground_distance = 0.3f;
	bool toggle;
	RaycastHit slope_hit;
	LayerMask raycast_mask;
	public float speed;

	NavMeshAgent agent;

	SpriteObject sobj;
	float anim_time = 0.0f;
	public float anim_rate;
	public int[] anim_frames;
	int current_anim_frame;

	bool is_alive = true;
	

	public enum Act_state {
		wait,
		find,
		attack,
		dead
	}
	public Act_state state = Act_state.wait;

	public enum Attack_type {
		melee,
		rocket,
		laser,
	}
	public Attack_type type;
	bool attackable = false;
	public float attack_rate;
	float attack_time = 0.0f;

	ParticleSystem attack_effect;
	public GameObject rocket_prefab;

	AudioSource asrc;
	public AudioClip attack_clip;

	void Start() {
		rigid = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();
		sobj = GetComponent<SpriteObject>();
		asrc = GetComponent<AudioSource>();

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite"));

		player_transform = GameManager.instance.player_transform;

		player = player_transform.GetComponent<Player>();

		if (type == Attack_type.melee)
			attack_effect = transform.GetChild(2).GetComponent<ParticleSystem>();
	}

	void Update() {
		if (Time.timeScale > 0) {
			if (is_alive) {
				bool temp_grounded = is_grounded;
				bool temp_liquided = is_liquided;
				is_grounded = Physics.CheckSphere(ground_check_transform.position, ground_distance, ground_mask);
				is_liquided = Physics.CheckSphere(ground_check_transform.position, ground_distance, liquid_mask);
				
				temp_grounded = temp_grounded != is_grounded;
				temp_liquided = temp_liquided != is_liquided;
				if (temp_grounded || temp_liquided) {
					rigid.drag = is_grounded ? ground_drag : air_drag;
					rigid.drag *= is_liquided ? liquid_multiplier : 0.0f;
				}

				if ((temp_grounded || !agent.enabled || agent.isStopped) && is_stopped() && !agent.isOnOffMeshLink) {
					toggle_rigid(is_grounded);
				}
			
				float distance = Vector3.Distance(player_transform.position, transform.position);

				switch (state) {
					case Act_state.wait:
						if (distance <= finding_distance) {
							state = Act_state.find;
						}
						break;
					case Act_state.find:
						if (agent.enabled) {
							agent.SetDestination(player_transform.position);
							if (agent.velocity.magnitude != 0) animate();
							else stop_animate();
						}
						if (distance > finding_distance) {
							state = Act_state.wait;
							stop_animate();
						}
						else if (distance <= stopping_distance + 0.25f) {
							state = Act_state.attack;
							agent.updateRotation = false;
						}
						break;
					case Act_state.attack:
						if (agent.enabled) {
							agent.SetDestination(player_transform.position);
							if (agent.velocity.magnitude != 0) animate();
							else stop_animate();
						}
						attackable = on_sight();
						rotate();
						if (attackable) agent.stoppingDistance = stopping_distance;
						else agent.stoppingDistance = 0;

						if (distance > stopping_distance + 0.25f) {
							state = Act_state.find;
							agent.updateRotation = true;
							attackable = false;
						}
						if (attackable) attack();
						break;
				}
			}
		}
	}

	void FixedUpdate() {
		if (Time.timeScale > 0) {
			if (is_grounded && on_slope() && (!agent.enabled || agent.velocity.magnitude == 0.0f)) {
				rigid.useGravity = false;
			}
			else {
				rigid.useGravity = true;
			}
		}
	}

	bool is_stopped() {
		return rigid.velocity.magnitude <= 0.0f;
	}

	bool on_slope() {
		if (Physics.Raycast(transform.position, Vector3.down, out slope_hit, height / 2 + 0.5f, ground_mask)) {
			if (slope_hit.normal != Vector3.up) {
				return true;
			}
		}
		return false;
	}

	public void toggle_rigid(bool toggle) {
		rigid.isKinematic = toggle;
		agent.enabled = toggle;
		rigid.drag = toggle ? ground_drag : air_drag;
	}

	void rotate() {

		Vector3 dir = (player_transform.position - transform.position).normalized;
		dir.y = 0;

		Quaternion rot = Quaternion.LookRotation(dir);

		if (dir.magnitude != 0.0f) {
			animate();
		}
		else {
			stop_animate();
		}

		Quaternion trot = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * agent.angularSpeed);
		transform.rotation = trot;
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	bool on_sight() {
		RaycastHit hit;
		Ray ray = new Ray(transform.position, player_transform.position - transform.position);

		if (!Physics.Raycast(ray, out hit, Mathf.Infinity, raycast_mask)) return false;
		if (hit.transform != player_transform) return false;
		return true;
	}

	void attack() {
		if (!sobj.is_alive) return;

		attack_time += Time.deltaTime;
		if (attack_time < attack_rate) return;

		attack_time -= attack_rate;
		switch (type) {
			case Attack_type.melee:
				attack_effect.Play();
				asrc.PlayOneShot(attack_clip);
				player.get_damage(10.0f);
				break;
			case Attack_type.rocket:
				GameObject rocket_obj = Instantiate(rocket_prefab);
				Rocket rocket = rocket_obj.GetComponent<Rocket>();
				rocket.launch(
					transform.position + new Vector3(0, 0.5f, 0),
					player_transform.position,
					transform,
					10, 10
				);
				break;
			case Attack_type.laser:
				break;
		}
	}

	void animate() {
		if (anim_frames.Length > 0) {
			anim_time -= Time.deltaTime;
			if (anim_time < 0) {
				anim_time += anim_rate;
				current_anim_frame = (current_anim_frame + 1) % anim_frames.Length;
				sobj.anim = anim_frames[current_anim_frame];
			}
		}
	}

	void stop_animate() {
		if (anim_frames.Length > 0) {
			current_anim_frame = 0;
			sobj.anim = anim_frames[current_anim_frame];
		}
	}

	public void die() {
		toggle_rigid(false);
		is_alive = false;
	}
}
