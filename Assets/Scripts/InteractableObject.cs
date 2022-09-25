using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour {
	public UnityEvent OnInteract;

	void Start() {
	}

	void Update() {
	}

	public void inter() {
		GameManager.instance.caption_addtext("Hello");
		GameManager.instance.caption_addtext("Hello");
		GameManager.instance.caption_addtext("Hello");
	}
}
