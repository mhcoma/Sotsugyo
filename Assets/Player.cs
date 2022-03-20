using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	// Start is called before the first frame update

	Vector3 key_direc = new Vector3(0, 0, 0);
	float mouse_x = 0;
	float mouse_y = 0;

	float speed = 10;

	Transform cam_transform;
	Camera cam;
	
	Transform laser_object;
	Transform test_obj;
	LineRenderer laser;
	ParticleSystem spark;
	void Start() {
		cam_transform = transform.GetChild(0);
		cam = cam_transform.GetComponent<Camera>();
		laser_object = cam_transform.GetChild(1);
		laser = laser_object.GetComponent<LineRenderer>();
		test_obj = cam_transform.GetChild(2);
		spark = test_obj.GetComponent<ParticleSystem>();
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.W)) key_direc += new Vector3(0, 0, 1);
		else if (Input.GetKeyUp(KeyCode.W)) key_direc -= new Vector3(0, 0, 1);
		if (Input.GetKeyDown(KeyCode.S)) key_direc -= new Vector3(0, 0, 1);
		else if (Input.GetKeyUp(KeyCode.S)) key_direc += new Vector3(0, 0, 1);
		if (Input.GetKeyDown(KeyCode.D)) key_direc += new Vector3(1, 0, 0);
		else if (Input.GetKeyUp(KeyCode.D)) key_direc -= new Vector3(1, 0, 0);
		if (Input.GetKeyDown(KeyCode.A)) key_direc -= new Vector3(1, 0, 0);
		else if (Input.GetKeyUp(KeyCode.A)) key_direc += new Vector3(1, 0, 0);
		
		mouse_x += Input.GetAxis("Mouse X") * 10;
		transform.eulerAngles = new Vector3(0, mouse_x, 0);
		Vector3 move_direc = Quaternion.Euler(0, mouse_x, 0) * key_direc.normalized;
		
		transform.position += move_direc * Time.deltaTime * speed;

		mouse_y += Input.GetAxis("Mouse Y") * 10;
		mouse_y = Mathf.Clamp(mouse_y, -55.0f, 55.0f);
		cam_transform.localEulerAngles = new Vector3(-mouse_y, 0, 0);
		
		if (Input.GetMouseButtonDown(0)) {
			laser_object.gameObject.SetActive(true);
			spark.Play();
		}
		if (Input.GetMouseButtonUp(0)) {
			laser_object.gameObject.SetActive(false);
			spark.Stop(false, ParticleSystemStopBehavior.StopEmitting);
		}
		
		

		RaycastHit hit;
		bool is_hit = false;
		Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		if (Physics.Raycast(ray, out hit)) {
			if (hit.distance < 100) {
				laser.SetPosition(0, laser_object.position);
				laser.SetPosition(1, hit.point);
				test_obj.position = hit.point;
				Transform objectHit = hit.transform;
				is_hit = true;
			}
		}
		if (!is_hit) {
			test_obj.localPosition = new Vector3(0, 0, 100);
			laser.SetPosition(0, laser_object.position);
			laser.SetPosition(1, test_obj.position);
		}
	}
}