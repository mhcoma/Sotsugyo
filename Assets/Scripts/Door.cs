using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
	public Vector3 destination_pos;
	Vector3 initial_pos;

	float degree = 0;
	public float speed;

	bool is_opening = false;
	bool is_moving = false;

	void Start() {
		initial_pos = transform.position;
	}
	void Update() {
		if (is_moving) {
			if (is_opening) {
				degree += Time.deltaTime * speed;
				if (degree >= 1.0f) {
					degree = 1.0f;
					is_moving = false;
				}
			}
			else {
				degree -= Time.deltaTime * speed;
				if (degree <= 0.0f) {
					degree = 0.0f;
					is_moving = false;
				}
			}
			transform.position = Vector3.Lerp(initial_pos, destination_pos, degree);
		}
	}

	public void open() {
		is_opening = true;
		is_moving = true;
	}

	public void close() {
		is_opening = false;
		is_moving = true;
	}
}
