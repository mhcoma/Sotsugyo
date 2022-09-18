using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour {
	public static CameraHolder instance = null;
	public Transform camera_position_transform;


	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}
	
	void Start() {
	}

	void Update() {
		transform.position = camera_position_transform.position;
	}

}
