using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteProjectileObject : MonoBehaviour {
	public Transform camera_transform;
	void Start() {
		camera_transform = Camera.main.transform;
	}

	void Update() {
		transform.LookAt(camera_transform.position, Vector3.up);
		transform.forward = camera_transform.forward;
	}
}
