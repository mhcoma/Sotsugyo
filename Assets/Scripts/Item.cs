using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour {
	public enum item_type {
		none,
		health_pack,
		laser_ammo,
		rocket_ammo,
		key
	};

	public item_type type;

	public int value;

	AudioSource asrc;
	public AudioClip get_clip;


	SpriteRenderer spr;
	bool alive = true;

	public UnityEvent on_get;

	void Start() {
		asrc = GetComponent<AudioSource>();
		spr = GetComponent<SpriteRenderer>();
	}

	void Update() {
		if (!alive && !asrc.isPlaying) {
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("Player") && alive) {
			bool result = item_effect(other.transform);
			if (result) {
				on_get.Invoke();
				asrc.PlayOneShot(get_clip);
				alive = false;
				spr.enabled = false;
			}
		}
	}

	bool item_effect(Transform tr) {
		bool result = true;
		Player player = tr.GetComponent<Player>();
		switch (type) {
			case item_type.health_pack:
				result = player.get_damage(value);
				break;
			case item_type.laser_ammo:
				result = player.get_ammo(Player.WeaponIndex.lasergun, value);
				break;
			case item_type.rocket_ammo:
				result = player.get_ammo(Player.WeaponIndex.rocketlauncher, value);
				break;
			case item_type.key:
				player.get_item(Player.ItemIndex.key, value);
				break;
		}
		return result;
	}
}
