using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonAIObject : MonoBehaviour {
	Rigidbody rigid;
	float ground_drag = 10.0f;
	float air_drag = 2.0f;
	float ground_distance = 0.3f;
	public LayerMask ground_mask;
	bool is_grounded;
	void Start() {
		rigid = GetComponent<Rigidbody>();
	}

	void Update() {
		is_grounded = Physics.CheckSphere(transform.position - Vector3.up, ground_distance, ground_mask);

		if (is_grounded) {
			rigid.drag = ground_drag;
		}
		else {
			rigid.drag = air_drag;
		}
	}
}
