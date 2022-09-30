using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonAIObject : MonoBehaviour {
	Rigidbody rigid;
	float ground_drag = 10.0f;
	float liquid_drag = 4.0f;
	float air_drag = 2.0f;
	float ground_distance = 0.3f;
	public Transform ground_check_transform;
	public LayerMask ground_mask;
	public LayerMask liquid_mask;
	bool is_grounded;
	bool is_liquided;
	void Start() {
		rigid = GetComponent<Rigidbody>();
	}

	void Update() {
		is_grounded = Physics.CheckSphere(ground_check_transform.position, ground_distance, ground_mask);
		is_liquided = Physics.CheckSphere(ground_check_transform.position, ground_distance, liquid_mask);
		rigid.drag = is_grounded ? ground_drag : air_drag;
		rigid.drag += is_liquided ? liquid_drag : 0.0f;
	}
}
