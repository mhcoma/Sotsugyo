using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public LayerMask groundMask;
	bool is_grounded;
	RaycastHit slope_hit;
	Vector3 slope_move_amount;


	public Transform cam_holder_transform;
	CameraHolder camhold;
	Transform camhold_transform;
	Camera cam;
	
	Transform laser_object;
	Transform test_obj;
	Transform rocket_launcher;
	LineRenderer laser;

	float weapons_range = 100.0f;
	Sprite[] weapon_hud_sprites;
	
	ParticleSystem spark;

	bool is_shooting = false;

	enum Weapon_index {
		lasergun,
		rocketlauncher
	}
	Weapon_index weapon_index = Weapon_index.lasergun;

	public GameObject rocket_prefab;
	
	LayerMask raycast_mask;

	void Start() {
		rigid = GetComponent<Rigidbody>();
		camhold = cam_holder_transform.GetComponent<CameraHolder>();
		camhold_transform = cam_holder_transform.GetChild(0);
		cam = camhold_transform.GetComponent<Camera>();
		laser_object = camhold_transform.GetChild(1);
		laser = laser_object.GetComponent<LineRenderer>();
		test_obj = camhold_transform.GetChild(2);
		spark = test_obj.GetComponent<ParticleSystem>();
		rocket_launcher = camhold_transform.GetChild(3);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		weapon_hud_sprites = Resources.LoadAll<Sprite>("Sprites/Weapons");

		raycast_mask = ~(1 << LayerMask.NameToLayer("ProjectileSprite"));
	}

	void Update() {
		// is_grounded = Physics.Raycast(transform.position, Vector3.down, player_height / 2 + 0.1f);
		is_grounded = Physics.CheckSphere(transform.position - Vector3.up, ground_distance, groundMask);

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
		camhold_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);

		weapon_control();
	}

	void FixedUpdate() {
		// rigid.MovePosition(transform.position + move_amount * Time.fixedDeltaTime * speed);
		
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

	void weapon_control() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = Weapon_index.lasergun;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			is_shooting = false;
			toggle_laser(false);
			weapon_index = Weapon_index.rocketlauncher;
		}


		if (Input.GetMouseButtonDown(0)) {
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

		RaycastHit first_hit;
		bool is_hit = false;
		Ray first_ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(first_ray, out first_hit, Mathf.Infinity, raycast_mask)) {
			is_hit = cast_second_ray(laser_object.position, -(laser_object.position - first_hit.point).normalized);
		}
		else {
			test_obj.localPosition = new Vector3(0, 0, weapons_range);
			is_hit = cast_second_ray(laser_object.position, -(laser_object.position - test_obj.position).normalized);
		}
		if (!is_hit) {
			test_obj.localPosition = new Vector3(0, 0, weapons_range);
			laser.SetPosition(0, laser_object.position);
			laser.SetPosition(1, test_obj.position);

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
				laser.SetPosition(0, laser_object.position);
				laser.SetPosition(1, second_hit.point);
				test_obj.position = second_hit.point;
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
		if (Physics.Raycast(transform.position, Vector3.down, out slope_hit, player_height / 2 + 0.5f, groundMask)) {
			if (slope_hit.normal != Vector3.up) {
				return true;
			}
		}
		return false;
	}

	void launch_rocket() {
		GameObject rocket_obj = Instantiate(rocket_prefab);
		Rocket rocket = rocket_obj.GetComponent<Rocket>();
		rocket.launch(rocket_launcher.position, test_obj.position, transform);
		is_shooting = false;
	}

	void toggle_laser(bool state) {
		if (state) {
			laser_object.gameObject.SetActive(true);
			spark.Play();
		}
		else {
			laser_object.gameObject.SetActive(false);
			spark.Stop(false, ParticleSystemStopBehavior.StopEmitting);
		}
	}
}