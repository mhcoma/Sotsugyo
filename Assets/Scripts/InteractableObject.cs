using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour {
	public UnityEvent on_interact;

	void Start() {
	}

	void Update() {
	}

	public void event_test() {
		GameManager.instance.caption_addtext("Test");
		GameManager.instance.caption_addtext("Test");
		GameManager.instance.caption_addtext("Test");
	}
}