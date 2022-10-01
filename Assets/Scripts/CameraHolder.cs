using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;

public class CameraHolder : MonoBehaviour {
	public static CameraHolder instance = null;
	public Transform camera_position_transform;

	public Transform underwater_transform;

	AudioSource underwater_asrc;
	AudioSource effect_asrc;
	public AudioClip water_splashes_aclip;
	
	public AudioMixer mixer;
	
	public LayerMask liquid_mask;

	bool underwater = false;

	float water_splashes_time = 0.0f;
	float water_splashes_interval = 0.5f;


	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}
	
	void Start() {
		AudioSource[] asrcs = GetComponents<AudioSource>();
		underwater_asrc = asrcs[0];
		effect_asrc = asrcs[1];
	}

	void Update() {
		transform.position = camera_position_transform.position;
		bool temp = underwater;
		underwater = Physics.CheckSphere(transform.position, 0.125f, liquid_mask);
		underwater_transform.gameObject.SetActive(underwater);
		
		temp = temp != underwater;

		if (water_splashes_time >= 0.0f) {
			water_splashes_time -= Time.deltaTime;
		}
		
		if (temp) {
			if (underwater) {
				underwater_asrc.Play();
				if (water_splashes_time <= 0.0f) {
					effect_asrc.PlayOneShot(water_splashes_aclip, 0.125f);
					water_splashes_time += water_splashes_interval;
				}
				mixer.SetFloat("UnderwaterLowpass", 0.0f);
			}
			else {
				underwater_asrc.Stop();
				if (water_splashes_time <= 0.0f) {
					effect_asrc.PlayOneShot(water_splashes_aclip, 0.125f);
					water_splashes_time += water_splashes_interval;
				}
				mixer.SetFloat("UnderwaterLowpass", -80.0f);
			}
		}
	}
}
