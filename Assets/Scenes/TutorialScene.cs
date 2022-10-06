using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : MonoBehaviour {
	void Start() {
		
	}

	void Update() {
		
	}

	public void laser_ammo_get_event() {
		GameManager.instance.caption_addtext("안녕하세요");
	}

	public void health_pack_get_event(Door door) {
		GameManager.instance.caption_addtext("This is ");
		door.open();
	}
}
