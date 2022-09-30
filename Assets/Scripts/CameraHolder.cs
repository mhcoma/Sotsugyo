using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;

public class CameraHolder : MonoBehaviour {
	public static CameraHolder instance = null;
	public Transform camera_position_transform;

	public Transform underwater_transform;

	AudioSource underwater_asrc;
	
	public AudioMixer mixer;
	
	public LayerMask liquid_mask;

	bool underwater = false;


	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}
	
	void Start() {
		underwater_asrc = GetComponent<AudioSource>();
	}

	void Update() {
		transform.position = camera_position_transform.position;
		bool temp = underwater;
		underwater = Physics.CheckSphere(transform.position, 0.125f, liquid_mask);
		underwater_transform.gameObject.SetActive(underwater);
		
		temp = temp != underwater;
		
		if (temp) {
			if (underwater) {
				underwater_asrc.Play();
				mixer.SetFloat("UnderwaterLowpass", 0.0f);
			}
			else {
				underwater_asrc.Stop();
				mixer.SetFloat("UnderwaterLowpass", -80.0f);
			}
		}
	}
}
