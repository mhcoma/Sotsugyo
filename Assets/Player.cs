using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour {
	// Start is called before the first frame update
	Rigidbody rigid;

	Vector3 key_direc = new Vector3(0, 0, 0);
	float mouse_x = 0;
	float mouse_y = 0;

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

	enum Weapon_index {
		none,
		lasergun,
		rocketlauncher
	}
	static string[] weapon_names = {
		"None",
		"Laser",
		"Rocket"
	};
	int[] weapon_ammo = {0, 1000, 250};
	Weapon_index weapon_index = Weapon_index.none;

	public GameObject rocket_prefab;
	
	LayerMask raycast_mask;

	public float health = 100;


	AudioSource laser_asrc;

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

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		weapon_hud_sprites = Resources.LoadAll<Sprite>("Sprites/Weapons");

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite"));

		weapon_hud_sprite_manager = canvas_transform.GetChild(0).GetComponent<WeaponHUDSprite>();
		HUD_transform = canvas_transform.GetChild(1);
		hp_tmpro = HUD_transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		weapon_tmpro = HUD_transform.GetChild(1).GetComponent<TextMeshProUGUI>();

		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");

		Debug.Log(HUD_transform.GetChild(0));

		AudioSource[] asrcs = GetComponents<AudioSource>();
		laser_asrc = asrcs[0];
	}

	void Update() {
		// is_grounded = Physics.Raycast(transform.position, Vector3.down, player_height / 2 + 0.1f);
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
		}

		slope_move_amount = Vector3.ProjectOnPlane(move_amount, slope_hit.normal);

		mouse_y += Input.GetAxis("Mouse Y") * 10;
		mouse_y = Mathf.Clamp(mouse_y, -90.0f, 90.0f);
		cam_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);

		weapon_control();
	}

	void FixedUpdate() {
		// rigid.MovePosition(transform.position + move_amount * Time.fixedDeltaTime * speed);
		
		if (is_grounded) {
			if (on_slope()) {
				rigid.AddForce(slope_move_amount * speed * movement_multiplier, ForceMode.Acceleration);
				rigid.useGravity = false;
			}
			else {
				rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
				rigid.useGravity = true;
			}
		}
		else {
			rigid.AddForce(move_amount * speed * movement_multiplier * air_multiplier, ForceMode.Acceleration);
			rigid.useGravity = true;
		}
		
		if (is_shooting_laser) {
			if (weapon_ammo[(int)weapon_index] > 0) {
				weapon_ammo[(int) Weapon_index.lasergun] -= 1;
				weapon_tmpro.SetText($"<size=64>{weapon_names[(int) Weapon_index.lasergun]}</size>\n{weapon_ammo[(int) weapon_index]}");
			}
			else {
				toggle_laser(false);
				is_shooting = false;
			}
		}
	}

	void weapon_control() {
		if (Input.GetKeyDown(KeyCode.Alpha1) && weapon_index != Weapon_index.lasergun) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = Weapon_index.lasergun;
			weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[(int)Weapon_index.lasergun]);
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2) && weapon_index != Weapon_index.rocketlauncher) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = Weapon_index.rocketlauncher;
			weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[(int)Weapon_index.rocketlauncher]);
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) weapon_index]}</size>\n{weapon_ammo[(int) weapon_index]}");
		}

		if (!weapon_hud_sprite_manager.is_changing_weapon()) {
			if (Input.GetMouseButtonDown(0) && weapon_ammo[(int)weapon_index] > 0) {
				switch (weapon_index) {
					case Weapon_index.lasergun:
						toggle_laser(true);
						is_shooting = true;
						break;
					case Weapon_index.rocketlauncher:
						is_shooting = true;
						break;
				}
			}
			if (Input.GetMouseButtonUp(0)) {
				switch (weapon_index) {
					case Weapon_index.lasergun:
						toggle_laser(false);
						is_shooting = false;
						break;
				}
			}
		}

		RaycastHit first_hit;
		bool is_hit = false;
		Ray first_ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(first_ray, out first_hit, Mathf.Infinity, raycast_mask)) {
			is_hit = cast_second_ray(laser_transform.position, -(laser_transform.position - first_hit.point).normalized);
		}
		else {
			spark_transform.localPosition = new Vector3(0, 0, weapons_range);
			spark_light_transform.localPosition = new Vector3(0, 0, weapons_range - spark_light_distance);
			is_hit = cast_second_ray(laser_transform.position, -(laser_transform.position - spark_transform.position).normalized);
		}
		if (!is_hit) {
			spark_transform.localPosition = new Vector3(0, 0, weapons_range);
			spark_light_transform.localPosition = new Vector3(0, 0, weapons_range - spark_light_distance);
			Vector3[] temp_laser_positions = {laser_transform.position, spark_transform.position};
			laser.SetPositions(temp_laser_positions);

			if (is_shooting) {
				switch (weapon_index) {
					case Weapon_index.rocketlauncher:
						launch_rocket();
						break;
				}
			}
		}
	}

	bool cast_second_ray(Vector3 origin, Vector3 direction) {
		RaycastHit second_hit;
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

				if (is_shooting) {
					switch (weapon_index) {
						case Weapon_index.lasergun:
							if (second_hit.transform.CompareTag("Actor")) {
								second_hit.transform.gameObject.GetComponent<SpriteObject>().get_damage(0.1f);
							}
							break;
						case Weapon_index.rocketlauncher:
							launch_rocket();
							break;
					}
				}
			}
		}
		return is_hit;
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
			rocket.launch(rocket_launcher_transform.position, spark_transform.position, transform, 0, 0);
			is_shooting = false;

			weapon_ammo[(int)Weapon_index.rocketlauncher] -= 1;
			weapon_tmpro.SetText($"<size=64>{weapon_names[(int) Weapon_index.rocketlauncher]}</size>\n{weapon_ammo[(int) weapon_index]}");
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

	public void get_damage(float damage) {
		health -= damage;
		// Debug.Log($"Get Damaged! : {health}");
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		if (health <= 0) {
			// Debug.Log("Player Dead");
		}
	}
}