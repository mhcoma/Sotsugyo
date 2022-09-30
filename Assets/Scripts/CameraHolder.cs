using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour {
	public static CameraHolder instance = null;
	public Transform camera_position_transform;

	public Transform underwater_transform;

	
	public LayerMask liquid_mask;


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

		underwater_transform.gameObject.SetActive(Physics.CheckSphere(transform.position, 0.125f, liquid_mask));
	}

}
