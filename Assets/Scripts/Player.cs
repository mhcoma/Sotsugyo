using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour {
	public static Player instance = null;
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
	float liquid_multiplier = 0.8f;
	bool jump_button = false;
	Vector3 move_amount = Vector3.zero;

	float player_height = 2.0f;
	float ground_distance = 0.3f;
	public LayerMask ground_mask;
	public LayerMask liquid_mask;
	public Transform ground_check_transform;
	bool is_grounded;
	bool is_liquided;
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

	public Dictionary<WeaponIndex, string> weapon_names = new Dictionary<WeaponIndex, string> {
		{WeaponIndex.none, "None"},
		{WeaponIndex.lasergun, "Laser"},
		{WeaponIndex.rocketlauncher, "Rocket"}
	};
	
	Dictionary<WeaponIndex, Sprite> weapon_hud_sprites = new Dictionary<WeaponIndex, Sprite>();

	public Dictionary<WeaponIndex, int> weapon_ammo = new Dictionary<WeaponIndex, int> {
		{WeaponIndex.none, 0},
		{WeaponIndex.lasergun, 0},
		{WeaponIndex.rocketlauncher, 0}
	};
	
	Dictionary<WeaponIndex, int> weapon_ammo_full = new Dictionary<WeaponIndex, int> {
		{WeaponIndex.none, 0},
		{WeaponIndex.lasergun, 1000},
		{WeaponIndex.rocketlauncher, 50}
	};
	Dictionary<WeaponIndex, float> weapon_interval = new Dictionary<WeaponIndex, float> {
		{WeaponIndex.none, 0},
		{WeaponIndex.lasergun, 0},
		{WeaponIndex.rocketlauncher, 0.5f}
	};
	Dictionary<WeaponIndex, float> weapon_damage = new Dictionary<WeaponIndex, float> {
		{WeaponIndex.none, 0},
		{WeaponIndex.lasergun, 10},
		{WeaponIndex.rocketlauncher, 20}
	};
	WeaponIndex weapon_index = WeaponIndex.none;
	float shoot_time = 0;

	public enum ItemIndex {
		none,
		key,
		gold
	}
	Dictionary<ItemIndex, int> item_inventory = new Dictionary<ItemIndex, int> {
		{ItemIndex.none, 0},
		{ItemIndex.key, 0},
		{ItemIndex.gold, 0}
	};

	public GameObject rocket_prefab;
	
	LayerMask raycast_mask;

	float health;
	public float max_health;

	float interact_range = 3.0f;


	AudioSource laser_asrc;

	public bool controllable = true;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
		Init();
	}

	void Init() {
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

		Sprite[] temp = Resources.LoadAll<Sprite>("Sprites/Weapons");

		foreach (WeaponIndex index in System.Enum.GetValues(typeof(WeaponIndex))) {
			weapon_hud_sprites[index] = temp[(int) index];
		}

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite")) & ~(1 << LayerMask.NameToLayer("Liquid"));

		weapon_hud_sprite_manager = canvas_transform.GetChild(0).GetComponent<WeaponHUDSprite>();
		HUD_transform = canvas_transform.GetChild(1);
		hp_tmpro = HUD_transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		weapon_tmpro = HUD_transform.GetChild(1).GetComponent<TextMeshProUGUI>();

		AudioSource[] asrcs = GetComponents<AudioSource>();
		laser_asrc = asrcs[0];

		rebirth();
	}

	void Update() {
		if (controllable) {
			is_grounded = Physics.CheckSphere(ground_check_transform.position, ground_distance, ground_mask);
			is_liquided = Physics.CheckSphere(ground_check_transform.position, ground_distance, liquid_mask);
			key_direc = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
			
			mouse_x += Input.GetAxis("Mouse X") * 10;
			cam_holder_transform.eulerAngles = new Vector3(0, mouse_x, 0);
			move_amount = Quaternion.Euler(0, mouse_x, 0) * key_direc;

			if (is_grounded || is_liquided) {
				rigid.drag = ground_drag;
			}
			else {
				rigid.drag = air_drag;
			}

			if (Input.GetButtonDown("Jump")) jump_button = true;
			if (Input.GetButtonUp("Jump")) jump_button = false;

			if (Input.GetButtonDown("Interact")) {
				interact();
			}

			slope_move_amount = Vector3.ProjectOnPlane(move_amount, slope_hit.normal);

			mouse_y += Input.GetAxis("Mouse Y") * 10;
			mouse_y = Mathf.Clamp(mouse_y, -90.0f, 90.0f);
			cam_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);

			weapon_control();
		}
	}

	void FixedUpdate() {
		float temp_multiplier;
		if (Time.timeScale > 0) {
			if (jump_button) {
				if (is_liquided) {
					rigid.AddForce(transform.up * jump_force * 0.1f, ForceMode.Impulse);
				}
				if (is_grounded) {
					rigid.velocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
					rigid.AddForce(transform.up * jump_force, ForceMode.Impulse);
					is_grounded = false;
					jumped = true;
				}
			}

			if (is_grounded) {
				temp_multiplier = is_liquided ? liquid_multiplier : 1.0f;
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
				temp_multiplier = is_liquided ? liquid_multiplier : air_multiplier;
				rigid.AddForce(move_amount * speed * movement_multiplier * temp_multiplier, ForceMode.Acceleration);
				rigid.useGravity = true;
			}
			if (is_shooting_laser) {
				if (weapon_ammo[weapon_index] > 0) {
					weapon_ammo[WeaponIndex.lasergun] -= 1;
					weapon_tmpro.SetText($"<size=64>{weapon_names[WeaponIndex.lasergun]}</size>\n{weapon_ammo[weapon_index]}");
				}
				else {
					toggle_laser(false);
					is_shooting = false;
				}
			}
		}
	}

	void weapon_control() {
		foreach (WeaponIndex index in System.Enum.GetValues(typeof(WeaponIndex))) {
			if (Input.GetButtonDown($"Weapon {(int) index}") && weapon_index != index) {
				is_shooting = false;
				toggle_laser(false);
				weapon_index = index;
				weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[index]);
				weapon_tmpro.SetText($"<size=64>{weapon_names[index]}</size>\n{weapon_ammo[index]}");
				shoot_time = 0;
			}
		}
		
		if (!weapon_hud_sprite_manager.is_changing_weapon()) {
			if (Input.GetButtonDown("Fire") && weapon_ammo[weapon_index] > 0) {
				is_shooting = true;
				switch (weapon_index) {
					case WeaponIndex.lasergun:
						toggle_laser(true);
						break;
					case WeaponIndex.rocketlauncher:
						break;
				}
			}
			if (Input.GetButtonUp("Fire")) {
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
		if (Physics.Raycast(second_ray, out second_hit, weapons_range, raycast_mask)) {
			laser.SetPosition(0, laser_transform.position);
			laser.SetPosition(1, second_hit.point);
			spark_transform.position = second_hit.point;

			Vector3 temp_distance = -((second_hit.point - cam_transform.position).normalized) * spark_light_distance;
			spark_light_transform.position = second_hit.point + temp_distance;
			Transform objectHit = second_hit.transform;
			is_hit = true;
			out_transform = second_hit.transform;
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
						shoot_time += weapon_interval[WeaponIndex.rocketlauncher];
					}
				}
				else if (shoot_time < 0) shoot_time = 0;
				break;
			case WeaponIndex.lasergun:
				if (is_shooting) {
					if (hit_transform != null) {
						if (hit_transform.CompareTag("Actor")) {
							hit_transform.gameObject.GetComponent<SpriteObject>().get_damage(weapon_damage[WeaponIndex.lasergun] * Time.deltaTime);
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
		if (weapon_ammo[weapon_index] > 0) {
			GameObject rocket_obj = Instantiate(rocket_prefab);
			Rocket rocket = rocket_obj.GetComponent<Rocket>();
			rocket.launch(rocket_launcher_transform.position, spark_transform.position, transform, weapon_damage[WeaponIndex.rocketlauncher], weapon_damage[WeaponIndex.rocketlauncher]);

			weapon_ammo[WeaponIndex.rocketlauncher] -= 1;
			weapon_tmpro.SetText($"<size=64>{weapon_names[WeaponIndex.rocketlauncher]}</size>\n{weapon_ammo[weapon_index]}");
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

	void interact() {
		RaycastHit hit;
		Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(ray, out hit, interact_range, raycast_mask)) {
			if (hit.transform != null) {
				InteractableObject interobj;

				interobj = hit.transform.GetComponent<InteractableObject>();
				if (interobj != null) {
					interobj.OnInteract.Invoke();
				}
			}
		}
		else {

		}

	}

	public bool get_damage(float damage) {
		if (damage <= 0 && health == max_health) {
			return false;
		}
		health -= damage;
		if (health > max_health) health = max_health;
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		if (health <= 0) {
			kill_player();
		}

		return true;
	}

	public bool get_ammo(WeaponIndex index, int value) {
		if (weapon_ammo[index] != weapon_ammo_full[index]) {
			weapon_ammo[index] += value;

			if (weapon_ammo[index] > weapon_ammo_full[index])
				weapon_ammo[index] = weapon_ammo_full[index];

			if (index == weapon_index) {
				weapon_tmpro.SetText($"<size=64>{weapon_names[index]}</size>\n{weapon_ammo[index]}");
			}
			return true;
		}
		return false;
	}

	public bool get_item(ItemIndex index, int value) {
		item_inventory[index] += value;
		return true;
	}

	public void set_controllable(bool toggle) {
		controllable = toggle;
		if (is_shooting_laser) {
			toggle_laser(toggle);
		}
		if (toggle) {
		}
		else {
			
		}
	}

	public void kill_player() {
		GameManager.instance.toggle_gameover(true);
	}

	public void rebirth() {
		health = max_health;
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");
		weapon_tmpro.SetText($"<size=64>{weapon_names[weapon_index]}</size>\n{weapon_ammo[weapon_index]}");
		transform.position = GameManager.instance.player_spawn_point_transform.position;
		rigid.velocity = Vector3.zero;
	}

	public bool is_alive() {
		return health >= 0;
	}
}