using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : MonoBehaviour {
	void Start() {
		
	}

	void Update() {
		
	}

	public void door_open_event() {
		GameManager.instance.caption_addtext("Test");
		GameManager.instance.caption_addtext("Test");
		GameManager.instance.caption_addtext("Test");
	}
}
