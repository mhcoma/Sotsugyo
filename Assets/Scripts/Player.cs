using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class Player : MonoBehaviour {
	public static Player instance = null;
	Rigidbody rigid;

	Vector3 key_direc = new Vector3(0, 0, 0);
	float mouse_x = 0;
	float mouse_y = 0;
	[System.NonSerialized]
	public float mouse_sensitivity = 10.0f;

	float ground_drag = 10.0f;
	float liquid_drag = 4.0f;
	float air_drag = 2.0f;
	float speed = 10.0f;
	float jump_force = 15.0f;
	float movement_multiplier = 10.0f;
	float air_multiplier = 0.25f;
	float liquid_jump_multiplier = 0.075f;
	float liquid_move_multiplier = 0.6f;
	bool jump_button = false;
	int jumped = 0;
	float jumped_time = 0.0f;
	float jumped_interval = 0.25f;
	Vector3 move_amount = Vector3.zero;
	public AudioClip jump_aclip;
	public AudioClip landing_aclip;

	float player_height = 2.0f;
	float ground_distance = 0.3f;
	public LayerMask ground_mask;
	public LayerMask liquid_mask;
	Transform ground_check_transform;
	bool is_grounded;
	bool is_liquided;
	RaycastHit slope_hit;
	Vector3 slope_move_amount;


	Transform camera_holder_transform;
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


	Transform canvas_transform;
	WeaponHUDSprite weapon_hud_sprite_manager;

	[System.NonSerialized]
	public Transform hud_transform;
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

	Dictionary<WeaponIndex, int> weapon_ammo = new Dictionary<WeaponIndex, int> {
		{WeaponIndex.none, 0},
		{WeaponIndex.lasergun, 0},
		{WeaponIndex.rocketlauncher, 0}
	};

	Dictionary<WeaponIndex, int> weapon_ammo_last = new Dictionary<WeaponIndex, int> {
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
		{WeaponIndex.lasergun, 50},
		{WeaponIndex.rocketlauncher, 20}
	};
	[System.NonSerialized]
	public WeaponIndex weapon_index = WeaponIndex.none;
	[System.NonSerialized]
	public WeaponIndex last_weapon_index = WeaponIndex.none;
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
	public bool is_invincible;

	PostProcessVolume post_volume;
	Vignette vignette;
	public AudioClip damaged_aclip;
	float damage_scale = 0.0f;

	float interact_range = 3.0f;
	public AudioClip interact_fail_aclip;
	public AudioClip interact_aclip;


	AudioSource laser_asrc;
	AudioSource asrc;
	public AudioClip water_splashes_aclip;
	float water_splashes_time = 0.0f;
	float water_splashes_interval = 0.5f;
	
	float goo_damage_interval = 0.5f;
	float goo_damage_time = 0.0f;
	float goo_damage = 2.5f;

	float main_demo_rotate_degree_phi = 0;
	float main_demo_rotate_degree_theta = 0;
	const float main_demo_rotate_scale_theta = 20.0f;
	const float main_demo_rotate_speed_phi = 10.0f;
	const float main_demo_rotate_speed_theta = 2.0f;

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
		camera_holder_transform = GameManager.instance.camera_holder_transform;

		rigid = GetComponent<Rigidbody>();
		camhold = camera_holder_transform.GetComponent<CameraHolder>();
		cam_transform = camera_holder_transform.GetChild(0);
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

		ground_check_transform = transform.Find("GroundChecker");

		canvas_transform = GameManager.instance.canvas_transform;

		weapon_hud_sprite_manager = canvas_transform.Find("WeaponImage").GetComponent<WeaponHUDSprite>();
		hud_transform = canvas_transform.Find("HUD");
		hp_tmpro = hud_transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		weapon_tmpro = hud_transform.GetChild(1).GetComponent<TextMeshProUGUI>();

		AudioSource[] asrcs = GetComponents<AudioSource>();
		
		laser_asrc = asrcs[0];
		asrc = asrcs[1];

		post_volume = cam.GetComponent<PostProcessVolume>();
		vignette = post_volume.profile.GetSetting<Vignette>();

		rebirth();
	}

	void Update() {
		switch (GameManager.instance.menu_state) {
			case GameManager.menu_state_enum.main_menu:
			case GameManager.menu_state_enum.main_play:
			case GameManager.menu_state_enum.main_option:
			case GameManager.menu_state_enum.main_input_option:
			case GameManager.menu_state_enum.main_input_key:
				camera_holder_transform.eulerAngles = new Vector3(0, main_demo_rotate_degree_phi, 0);
				main_demo_rotate_degree_phi += Time.unscaledDeltaTime * main_demo_rotate_speed_phi;
				float temp = Mathf.Cos(main_demo_rotate_degree_theta) * main_demo_rotate_scale_theta;
				cam_transform.localEulerAngles = new Vector3(temp, 0, 0);
				main_demo_rotate_degree_theta += Time.unscaledDeltaTime * main_demo_rotate_speed_theta;
				break;
			case GameManager.menu_state_enum.playing:
				if (InputManager.get_button_down("jump")) jump_button = true;
				if (InputManager.get_button_up("jump")) jump_button = false;
				if (controllable) {
					bool temp_grounded = is_grounded;
					bool temp_liquided = is_liquided;

					is_grounded = Physics.CheckSphere(ground_check_transform.position, ground_distance, ground_mask);
					is_liquided = Physics.CheckSphere(ground_check_transform.position, ground_distance, liquid_mask);

					temp_grounded = temp_grounded != is_grounded;
					temp_liquided = temp_liquided != is_liquided;

					rigid.drag = is_grounded ? ground_drag : air_drag;
					rigid.drag += is_liquided ? liquid_drag : 0.0f;

					if (water_splashes_time >= 0.0f) {
						water_splashes_time -= Time.deltaTime;
					}
					
					if (is_grounded && temp_grounded) {
						asrc.PlayOneShot(landing_aclip);
					}

					if (is_liquided) {
						if (goo_damage_time >= 0.0f)
							goo_damage_time -= Time.deltaTime;
						if (goo_damage_time <= 0.0f) {
							get_damage(goo_damage);
							goo_damage_time += goo_damage_interval;
						}
					}

					if (temp_liquided && water_splashes_time <= 0.0f) {
						asrc.PlayOneShot(water_splashes_aclip, 0.25f);
						water_splashes_time += water_splashes_interval;
					}

					key_direc = new Vector3 (InputManager.get_axis("horizontal"), 0, InputManager.get_axis("vertical")).normalized;
					mouse_x += Input.GetAxis("Mouse X") * mouse_sensitivity;
					camera_holder_transform.eulerAngles = new Vector3(0, mouse_x, 0);
					move_amount = Quaternion.Euler(0, mouse_x, 0) * key_direc;

					if (InputManager.get_button_down("interact")) {
						interact();
					}

					slope_move_amount = Vector3.ProjectOnPlane(move_amount, slope_hit.normal);

					mouse_y += Input.GetAxis("Mouse Y") * mouse_sensitivity;
					mouse_y = Mathf.Clamp(mouse_y, -90.0f, 90.0f);
					cam_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);

					weapon_control();

					if (damage_scale > 0.0f) {
						damage_scale -= Time.deltaTime;
						if (damage_scale < 0.0f) damage_scale = 0.0f;
						vignette.intensity.value = damage_scale;
					}
				}
				break;
		}
	}

	void FixedUpdate() {
		float temp_multiplier;
		if (Time.timeScale > 0) {
			if (jump_button) {
				if (is_liquided) {
					rigid.AddForce(transform.up * jump_force * liquid_jump_multiplier, ForceMode.Impulse);
				}
				if (is_grounded && jumped == 0) {
					rigid.velocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
					rigid.AddForce(transform.up * jump_force, ForceMode.Impulse);
					is_grounded = false;
					asrc.PlayOneShot(jump_aclip);
					
					jumped++;
				}
			}

			if (is_grounded) {
				if (jumped > 2) {
					jumped = 0;
					jumped_time = 0.0f;
				}
				else if (jumped > 0) {
					jumped_time += Time.fixedDeltaTime;
					if (jumped_time >= jumped_interval) jumped++;
				}
				if (on_slope()) {
					if (jumped != 0) {
						rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
					}
					else rigid.AddForce(slope_move_amount * speed * movement_multiplier, ForceMode.Acceleration);
					rigid.useGravity = false;
				}
				else {
					rigid.AddForce(move_amount * speed * movement_multiplier, ForceMode.Acceleration);
					rigid.useGravity = true;
				}
			}
			else {
				if (jumped == 1 || jumped == 2) jumped++; 
				temp_multiplier = is_liquided ? liquid_move_multiplier : air_multiplier;
				rigid.AddForce(move_amount * speed * movement_multiplier * temp_multiplier, ForceMode.Acceleration);
				rigid.useGravity = true;
			}
			
			if (is_shooting_laser) {
				if (weapon_ammo[weapon_index] > 0) {
					weapon_ammo[WeaponIndex.lasergun] -= 1;
					refresh_display_ammo();
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
			if (InputManager.get_button_down($"weapon {(int) index}") && weapon_index != index) {
				is_shooting = false;
				toggle_laser(false);
				weapon_index = index;
				weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[index]);
				weapon_tmpro.SetText($"<size=64>{weapon_names[index]}</size>\n{weapon_ammo[index]}");
				shoot_time = 0;
			}
		}
		
		if (!weapon_hud_sprite_manager.is_changing_weapon()) {
			if (InputManager.get_button_down("fire") && weapon_ammo[weapon_index] > 0) {
				is_shooting = true;
				switch (weapon_index) {
					case WeaponIndex.lasergun:
						toggle_laser(true);
						break;
					case WeaponIndex.rocketlauncher:
						break;
				}
			}
			if (InputManager.get_button_up("fire")) {
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
			refresh_display_ammo();
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
		bool result = false;
		RaycastHit hit;
		Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(ray, out hit, interact_range, raycast_mask)) {
			if (hit.transform != null) {
				InteractableObject interobj;

				interobj = hit.transform.GetComponent<InteractableObject>();
				if (interobj != null) {
					interobj.on_interact.Invoke();
					result = true;
				}
			}
		}

		if (result) asrc.PlayOneShot(interact_aclip);
		else asrc.PlayOneShot(interact_fail_aclip);
	}

	public bool get_damage(float damage) {
		if (damage <= 0 && health == max_health) {
			return false;
		}
		if (is_invincible && damage > 0.0f) damage = 0.0f;
		health -= damage;
		if (health > max_health) health = max_health;
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");

		if (damage > 0.0f) {
			damage_scale = Mathf.Max(Mathf.Min(Mathf.Max(damage / 20.0f, 0.5f), 1.0f), damage_scale);
			asrc.PlayOneShot(damaged_aclip, damage_scale);
		}
		if (health <= 0.0f) {
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
				refresh_display_ammo();
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
	}

	public void kill_player() {
		GameManager.instance.gameover();
	}

	public void rebirth() {
		health = max_health;
		hp_tmpro.SetText($"<size=64>HP</size>\n{(int)health}");

		foreach (WeaponIndex index in System.Enum.GetValues(typeof(WeaponIndex))) {
			weapon_ammo[index] = weapon_ammo_last[index];
		}
		weapon_index = last_weapon_index;
		weapon_hud_sprite_manager.chnage_weapon_sprite(weapon_hud_sprites[weapon_index]);
		refresh_display_ammo();

		reset_player_position();
	}

	public bool is_alive() {
		return health >= 0;
	}

	public Vector3 next_position(Vector3 launcher_pos, float projectile_speed) {

		Vector3 next_pos = transform.position;
		float distance = Vector3.Distance(next_pos, launcher_pos);
		float time = 0.0f;
		float collision_time = distance / projectile_speed;

		int count = 0;

		while ((time < collision_time) && count < 100) {
			next_pos += rigid.velocity * Time.fixedDeltaTime;
			distance = Vector3.Distance(next_pos, launcher_pos);
			time += Time.fixedDeltaTime;
			collision_time = distance / projectile_speed;
			count++;
		}

		next_pos.y = transform.position.y;
		next_pos = Vector3.Lerp(next_pos, transform.position, Random.Range(0.0f, 0.5f));

		return next_pos;
	}

	public void refresh_display_ammo() {
		weapon_tmpro.SetText($"<size=64>{weapon_names[weapon_index]}</size>\n{weapon_ammo[weapon_index]}");
	}

	public void reset_player_position() {
		Input.ResetInputAxes();

		transform.position = GameManager.instance.player_spawn_point_transform.position;
		Vector3 temp_angle = GameManager.instance.player_spawn_point_transform.localEulerAngles;
		mouse_y = temp_angle.x;
		mouse_x = temp_angle.y;
		if (camera_holder_transform != null) {
			camera_holder_transform.localEulerAngles = new Vector3(0, 0, 0);
			cam_transform.localEulerAngles = new Vector3(0, 0, 0);
		}
		
		if (rigid != null)
			rigid.velocity = Vector3.zero;
		jump_button = false;
		jumped = 0;
	}
}