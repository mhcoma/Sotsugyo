using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour {
	// Start is called before the first frame update
	Rigidbody rigid;

	Vector3 key_direc = new Vector3(0, 0, 0);
	float mouse_x = 0;
	float mouse_y = 0;
	bool jumped = false;

	float ground_drag = 10.0f;
	float air_drag = 2.0f;
	float speed = 10.0f;
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


	public Transform cam_holder_transform;
	CameraHolder camhold;
	Transform cam_transform;
	Camera cam;
	
	Transform laser_transform;
	Transform spark_transform;
	Transform rocket_launcher_transform;
	Transform spark_light_transform;
	LineRenderer laser;
	float spark_light_distance = 0.25f;

	float weapons_range = 100.0f;
	Sprite[] weapon_hud_sprites;


	public Transform canvas_transform;
	WeaponHUDSprite weapon_hud_sprite_manager;
	Transform HUD_transform;
	TextMeshProUGUI hp_tmpro;
	TextMeshProUGUI weapon_tmpro;
	
	ParticleSystem spark;

	bool is_shooting = false;
	bool is_shooting_laser = false;

	public enum WeaponIndex {
		none,
		lasergun,
		rocketlauncher
	}
	static string[] weapon_names = {
		"None",
		"Laser",
		"Rocket"
	};
	public int[] weapon_ammo;
	int[] weapon_ammo_full = {0, 1000, 50};
	float[] weapon_interval = {0, 0, 0.5f};
	float[] weapon_damage = {0, 10, 20};
	WeaponIndex weapon_index = WeaponIndex.none;
	
	float shoot_time = 0;

	public GameObject rocket_prefab;
	
	LayerMask raycast_mask;

	public float health = 100;
	float max_health = 100;


	AudioSource laser_asrc;

	bool controlable = true;

	void Start() {
		rigid = GetComponent<Rigidbody>();
		camhold = cam_holder_transform.GetComponent<CameraHolder>();
		cam_transform = cam_holder_transform.GetChild(0);
		cam = cam_transform.GetComponent<Camera>();
		laser_transform = cam_transform.GetChild(1);
		laser = laser_transform.GetComponent<LineRenderer>();
		spark_transform = cam_transform.GetChild(2);
		spark = spark_transform.GetComponent<ParticleSystem>();
		rocket_launcher_transform = cam_transform.GetChild(3);
		spark_light_transform = cam_transform.GetChild(4);

		weapon_hud_sprites = Resources.LoadAll<Sprite>("Sprites/Weapons");

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite"));

		weapon_hud_sprite_manager = canvas_transform.GetChild(0).GetComponent<WeaponHUDSprite>();
		HUD_transform = canvas_transform.GetChild(1);
		hp_tmpro = HUD_transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		weapon_tmpro = HUD_transform.GetChild(1).GetComponent<TextMeshProUGUI>();

		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");

		// Debug.Log(HUD_transform.GetChild(0));

		AudioSource[] asrcs = GetComponents<AudioSource>();
		laser_asrc = asrcs[0];
	}

	void Update() {
		if (Time.timeScale > 0) {
			is_grounded = Physics.CheckSphere(transform.position - Vector3.up, ground_distance, ground_mask);

			key_direc = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
			
			mouse_x += Input.GetAxis("Mouse X") * 10;
			cam_holder_transform.eulerAngles = new Vector3(0, mouse_x, 0);
			move_amount = Quaternion.Euler(0, mouse_x, 0) * key_direc;

			if (is_grounded) {
				rigid.drag = ground_drag;
			}
			else {
				rigid.drag = air_drag;
			}

			if (Input.GetKeyDown(KeyCode.Space) && is_grounded) {
				is_grounded = false;
				rigid.AddForce(transform.up * jump_force, ForceMode.Impulse);
				jumped = true;
			}

			slope_move_amount = Vector3.ProjectOnPlane(move_amount, slope_hit.normal);

			mouse_y += Input.GetAxis("Mouse Y") * 10;
			mouse_y = Mathf.Clamp(mouse_y, -90.0f, 90.0f);
			cam_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);

			weapon_control();
		}
	}

	void FixedUpdate() {
		// rigid.MovePosition(transform.position + move_amount * Time.fixedDeltaTime * speed);
		
		if (Time.timeScale > 0) {
			if (is_grounded) {
				if (on_slope()) {
					if (jumped) {
						rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
						jumped = false;
					}
					else rigid.AddForce(slope_move_amount * speed * movement_multiplier, ForceMode.Acceleration);
					rigid.useGravity = false;
				}
				else {
					rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
					jumped = false;
					rigid.useGravity = true;
				}
			}
			else {
				rigid.AddForce(move_amount * speed * movement_multiplier * air_multiplier, ForceMode.Acceleration);
				rigid.useGravity = true;
			}
			if (is_shooting_laser) {
				if (weapon_ammo[(int)weapon_index] > 0) {
					weapon_ammo[(int) WeaponIndex.lasergun] -= 1;
					weapon_tmpro.SetText($"<size=64>{weapon_names[(int) WeaponIndex.lasergun]}</size>\n{weapon_ammo[(int) weapon_index]}");
				}
				else {
					toggle_laser(false);
					is_shooting = false;
				}
			}
		}
	}

	void weapon_control() {
		if (Input.GetKeyDown(KeyCode.Alpha1) && weapon_index != WeaponIndex.lasergun) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = WeaponIndex.lasergun;
			weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[(int)WeaponIndex.lasergun]);
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");
			shoot_time = 0;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2) && weapon_index != WeaponIndex.rocketlauncher) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = WeaponIndex.rocketlauncher;
			weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[(int)WeaponIndex.rocketlauncher]);
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");
			shoot_time = 0;
		}

		if (!weapon_hud_sprite_manager.is_changing_weapon()) {
			if (Input.GetMouseButtonDown(0) && weapon_ammo[(int)weapon_index] > 0) {
				is_shooting = true;
				switch (weapon_index) {
					case WeaponIndex.lasergun:
						toggle_laser(true);
						break;
					case WeaponIndex.rocketlauncher:
						break;
				}
			}
			if (Input.GetMouseButtonUp(0)) {
				is_shooting = false;
				switch (weapon_index) {
					case WeaponIndex.lasergun:
						toggle_laser(false);
						break;
				}
			}
		}

		RaycastHit first_hit;
		Ray first_ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		Transform hit_transform;
		if (Physics.Raycast(first_ray, out first_hit, Mathf.Infinity, raycast_mask)) {
			hit_transform = cast_second_ray(
				laser_transform.position,
				-(laser_transform.position - first_hit.point).normalized
			);
		}
		else {
			spark_transform.localPosition = new Vector3(0, 0, weapons_range);
			spark_light_transform.localPosition = new Vector3(0, 0, weapons_range - spark_light_distance);
			hit_transform = cast_second_ray(
				laser_transform.position,
				-(laser_transform.position - spark_transform.position).normalized
			);
		}

		shoot(hit_transform);
	}

	Transform cast_second_ray(Vector3 origin, Vector3 direction) {
		RaycastHit second_hit;
		Transform out_transform = null;
		bool is_hit = false;
		Ray second_ray = new Ray(origin, direction);
		if (Physics.Raycast(second_ray, out second_hit, Mathf.Infinity, raycast_mask)) {
			if (second_hit.distance < weapons_range) {
				laser.SetPosition(0, laser_transform.position);
				laser.SetPosition(1, second_hit.point);
				spark_transform.position = second_hit.point;

				Vector3 temp_distance = -((second_hit.point - cam_transform.position).normalized) * spark_light_distance;
				spark_light_transform.position = second_hit.point + temp_distance;
				Transform objectHit = second_hit.transform;
				is_hit = true;
				out_transform = second_hit.transform;
			}
		}

		if (!is_hit) {
			spark_transform.localPosition = new Vector3(0, 0, weapons_range);
			spark_light_transform.localPosition = new Vector3(0, 0, weapons_range - spark_light_distance);
			Vector3[] temp_laser_positions = {laser_transform.position, spark_transform.position};
			laser.SetPositions(temp_laser_positions);
		}
		return out_transform;
	}

	void shoot(Transform hit_transform) {
		switch (weapon_index) {
			case WeaponIndex.rocketlauncher:
				shoot_time -= Time.deltaTime;
				if (is_shooting) {
					if (shoot_time < 0) {
						launch_rocket();
						shoot_time += weapon_interval[(int)WeaponIndex.rocketlauncher];
					}
				}
				else if (shoot_time < 0) shoot_time = 0;
				break;
			case WeaponIndex.lasergun:
				if (is_shooting) {
					if (hit_transform != null) {
						if (hit_transform.CompareTag("Actor")) {
							hit_transform.gameObject.GetComponent<SpriteObject>().get_damage(weapon_damage[(int) WeaponIndex.lasergun] * Time.deltaTime);
						}
					}
				}
				break;
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

	void launch_rocket() {
		if (weapon_ammo[(int)weapon_index] > 0) {
			GameObject rocket_obj = Instantiate(rocket_prefab);
			Rocket rocket = rocket_obj.GetComponent<Rocket>();
			rocket.launch(rocket_launcher_transform.position, spark_transform.position, transform, weapon_damage[(int) WeaponIndex.rocketlauncher], weapon_damage[(int) WeaponIndex.rocketlauncher]);

			weapon_ammo[(int)WeaponIndex.rocketlauncher] -= 1;
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) WeaponIndex.rocketlauncher]}</size>\n{weapon_ammo[(int) weapon_index]}");
		}
	}

	void toggle_laser(bool state) {
		laser_transform.gameObject.SetActive(state);
		spark_light_transform.gameObject.SetActive(state);
		is_shooting_laser = state;
		if (state) {
			spark.Play();
			laser_asrc.Play();
		}
		else {
			spark.Stop(false, ParticleSystemStopBehavior.StopEmitting);
			laser_asrc.Pause();
		}
	}

	public bool get_damage(float damage) {
		if (damage <= 0 && health == max_health) {
			return false;
		}
		health -= damage;
		if (health > max_health) health = max_health;
		// Debug.Log($"Get Damaged! : {health}");
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		if (health <= 0) {
			kill_player();
		}

		return true;
	}

	public bool get_ammo(WeaponIndex index, int value) {
		if (weapon_ammo[(int) index] != weapon_ammo_full[(int) index]) {
			weapon_ammo[(int) index] += value;

			if (weapon_ammo[(int) index] > weapon_ammo_full[(int) index])
				weapon_ammo[(int) index] = weapon_ammo_full[(int) index];

			if (index == weapon_index) {
				weapon_tmpro.SetText($"<size=64>{weapon_names[(int) index]}</size>\n{weapon_ammo[(int) index]}");
			}
			return true;
		}
		return false;
	}

	public void set_controllable(bool toggle) {
		controlable = toggle;
		if (is_shooting_laser) {
			toggle_laser(toggle);
		}
		if (toggle) {
		}
		else {
			
		}
	}

	public void kill_player() {
		// SceneManager.LoadScene("Scenes/Test", LoadSceneMode.Single);
	}
}