using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {
	Rigidbody rigid;
	NavMeshPath path;
	NavMeshPath path_sidestep;

	public Transform player_transform;
	Player player;

	float ground_drag = 10.0f;
	float air_drag = 2.0f;
	public float speed = 5.0f;
	float jump_force = 15.0f;
	float movement_multiplier = 10.0f;
	float air_multiplier = 0.2f;
	Vector3 move_amount = Vector3.zero;
	float player_height = 2.0f;
	float ground_distance = 0.3f;
	public LayerMask ground_mask;
	bool is_grounded;
	RaycastHit slope_hit;
	Vector3 slope_move_amount;

	Vector3 last_target_pos;
	Vector3 pos_for_rotation;
	public float finding_distance = 30;
	public float stopping_distance;
	public float rotation_speed;

	LayerMask raycast_mask;
	bool side_movement_direc = true;

	SpriteObject sobj;
	float anim_time = 0.0f;
	public float anim_rate;
	public int[] anim_frames;
	int current_anim_frame = 0;

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
		path = new NavMeshPath();
		last_target_pos = transform.position;
		pos_for_rotation = transform.position;
		sobj = GetComponent<SpriteObject>();
		asrc = GetComponent<AudioSource>();

		path = new NavMeshPath();
		path_sidestep = new NavMeshPath();

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite"));

		player = player_transform.GetComponent<Player>();

		if (type == Attack_type.melee)
			attack_effect = transform.GetChild(2).GetComponent<ParticleSystem>();
	}

	void Update() {
		is_grounded = Physics.CheckSphere(transform.position - Vector3.up, ground_distance, ground_mask);

		float distance = Vector3.Distance(player_transform.position, transform.position);

		switch (state) {
			case Act_state.wait:
				if (distance <= finding_distance) {
					state = Act_state.find;
				}
				break;
			case Act_state.find:
				find_path();
				rotate();
				if (distance > finding_distance) {
					state = Act_state.wait;
				}
				else if (distance <= stopping_distance + 0.25f) {
					state = Act_state.attack;
				}

				break;
			case Act_state.attack:
				find_path();
				find_player();
				rotate();
				if (distance > stopping_distance + 0.25f) {
					state = Act_state.find;
					attackable = false;
				}
				if (attackable) attack();
				break;
		}

		if (is_grounded) {
			rigid.drag = ground_drag;
		}
		else {
			rigid.drag = air_drag;
			move_amount = Vector3.zero;
		}

		slope_move_amount = Vector3.ProjectOnPlane(move_amount, slope_hit.normal);
	}

	void FixedUpdate() {
		if (is_grounded) {
			if (on_slope()) {
				rigid.AddForce(slope_move_amount * speed * movement_multiplier, ForceMode.Acceleration);
				rigid.AddForce(-(Physics.gravity * rigid.mass));
			}
			else {
				rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
			}
		}
		else {
			rigid.AddForce(move_amount * speed * movement_multiplier * air_multiplier, ForceMode.Acceleration);
		}
	}

	bool on_slope() {
		if (Physics.Raycast(transform.position, Vector3.down, out slope_hit, player_height / 2 + 0.5f, ground_mask)) {
			if (slope_hit.normal != Vector3.up) {
				return true;
			}
		}
		return false;
	}

	void rotate() {
		Quaternion rot;

		if (move_amount.magnitude > 0.0f) {
			rot = Quaternion.LookRotation(move_amount);
			if (anim_frames.Length > 0) {
				anim_time += Time.deltaTime;
				if (anim_time > anim_rate) {
					anim_time -= anim_rate;
					current_anim_frame = (current_anim_frame + 1) % anim_frames.Length;
					sobj.anim = anim_frames[current_anim_frame];
				}
			}
		}
		else {
			rot = Quaternion.LookRotation(pos_for_rotation - transform.position);
			if (anim_frames.Length > 0) {
				current_anim_frame = 0;
				sobj.anim = anim_frames[current_anim_frame];
			}
		}

		Quaternion trot = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotation_speed);
		transform.rotation = trot;
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	void find_path() {
		if (!is_grounded) return;

		move_amount = Vector3.zero;
		if (NavMesh.CalculatePath(transform.position, player_transform.position, NavMesh.AllAreas, path)) {
			if (path.corners.Length >= 2) {
				last_target_pos = path.corners[1];
				pos_for_rotation = path.corners[1];
				move_amount = last_target_pos - path.corners[0];
			}
			if (path.corners.Length == 2) {
				Vector2 a = new Vector2(player_transform.position.x, player_transform.position.z);
				Vector2 b = new Vector2(path.corners[1].x, path.corners[1].z);
				if (a != b) {
					pos_for_rotation = player_transform.position;
					move_amount = last_target_pos - transform.position;
					if (move_amount.magnitude <= 0.5f || Vector3.Distance(player_transform.position, transform.position) <= stopping_distance) {
						move_amount = Vector3.zero;
					}
				}
				else {
					if (move_amount.magnitude <= stopping_distance) {
						move_amount = Vector3.zero;
					}
				}
			}
		}
		else {
			Vector2 a = new Vector2(player_transform.position.x, player_transform.position.z);
			Vector2 b = new Vector2(last_target_pos.x, last_target_pos.z);
			if (a != b) {
				pos_for_rotation = player_transform.position;
				move_amount = last_target_pos - transform.position;
				if (move_amount.magnitude <= 0.5f || Vector3.Distance(player_transform.position, transform.position) <= stopping_distance) {
					move_amount = Vector3.zero;
				}
			}
			else {
				move_amount = last_target_pos - transform.position;
				if (move_amount.magnitude <= stopping_distance) {
					move_amount = Vector3.zero;
				}
			}
		}
		move_amount.y = 0;
		move_amount.Normalize();
	}

	void find_player() {
		RaycastHit hit;
		Ray ray = new Ray(transform.position, player_transform.position - transform.position);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycast_mask) && (path.corners.Length == 2)) {
			if (hit.transform != player_transform) {
				Vector2 a = new Vector2(player_transform.position.x, player_transform.position.z);
				Vector2 b = new Vector2(transform.position.x, transform.position.z);
				Vector2 c = (a - b).normalized;
				float angle = Mathf.PI * 0.5f * (side_movement_direc ? 1 : -1);
				Vector2 d = new Vector2(
					c.x * Mathf.Cos(angle) - c.y * Mathf.Sin(angle),
					c.x * Mathf.Sin(angle) + c.y * Mathf.Cos(angle)
				) + a;
				Vector3 destination_pos = new Vector3(
					d.x, transform.position.y, d.y
				);

				Vector3 temp_move_amount = Vector3.zero;
				if (NavMesh.CalculatePath(transform.position, destination_pos, NavMesh.AllAreas, path_sidestep)) {
					if (path_sidestep.corners.Length >= 2) {
						temp_move_amount = path_sidestep.corners[1] - path_sidestep.corners[0];
					}
					if (new Vector2(path_sidestep.corners[1].x, path_sidestep.corners[1].z) != new Vector2(destination_pos.x, destination_pos.z)) {
						Debug.Log($"{transform.name} : {path_sidestep.corners[1]}, {destination_pos}");
						side_movement_direc = !side_movement_direc;
					}
				}
				temp_move_amount.y = 0;
				temp_move_amount.Normalize();
				move_amount *= -1;
				move_amount += temp_move_amount;
				attackable = false;
			}
			else {
				attackable = true;
			}
		}
	}

	void attack() {
		if (sobj.is_alive) {
			attack_time += Time.deltaTime;
			if (attack_time >= attack_rate) {
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
						rocket.launch(transform.position + new Vector3(0, 0.5f, 0), player_transform.position, transform);
						break;
					case Attack_type.laser:
						break;
				}
			}
		}
	}
}
