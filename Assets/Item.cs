using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	void Start() {
		
	}

	void Update() {
		
	}

	void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("Player")) {
			item_effect(other.transform);
			Destroy(gameObject);
		}
	}

	void item_effect(Transform tr) {
		Player player = tr.GetComponent<Player>();
		switch (type) {
			case item_type.health_pack:
				player.get_damage(value);
				break;
			case item_type.laser_ammo:
				player.get_ammo(Player.WeaponIndex.lasergun, value);
				break;
			case item_type.rocket_ammo:
				player.get_ammo(Player.WeaponIndex.rocketlauncher, value);
				break;
			case item_type.key:
				break;
		}
	}
}
