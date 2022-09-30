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

	AudioSource asrc;
	public AudioClip water_splashes_aclip;
	void Start() {
		rigid = GetComponent<Rigidbody>();
		asrc = GetComponent<AudioSource>();
	}

	void Update() {
		bool temp_grounded = is_grounded;
		bool temp_liquided = is_liquided;

		is_grounded = Physics.CheckSphere(ground_check_transform.position, ground_distance, ground_mask);
		is_liquided = Physics.CheckSphere(ground_check_transform.position, ground_distance, liquid_mask);

		temp_grounded = temp_grounded != is_grounded;
		temp_liquided = temp_liquided != is_liquided;

		rigid.drag = is_grounded ? ground_drag : air_drag;
		rigid.drag += is_liquided ? liquid_drag : 0.0f;
		
		if (temp_liquided) {
			asrc.PlayOneShot(water_splashes_aclip);
		}
	}
}
