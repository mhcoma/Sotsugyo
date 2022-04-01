using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour {
	public Transform camera_position_transform;

	void Start() {
	}

	void Update() {
		transform.position = camera_position_transform.position;
	}

}
