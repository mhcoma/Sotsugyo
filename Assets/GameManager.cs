using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject pause_menu;
	public GameObject player_object;
	Player player;

	bool menu_toggle = false;
	void Start() {
		player = player_object.GetComponent<Player>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update() {
		if (Input.GetButtonDown("Cancel")) {
			toggle_menu();
		}
	}

	void toggle_menu() {
		menu_toggle = !menu_toggle;
		pause_menu.gameObject.SetActive(menu_toggle);
		Cursor.lockState = menu_toggle ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = menu_toggle;
		Time.timeScale = menu_toggle ? 0 : 1;
		player.set_controllable(!menu_toggle);
	}
}
