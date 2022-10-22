using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerLocation : MonoBehaviour {
	public UnityEvent on_trigger;

	void Start() {
		
	}

	void Update() {
		
	}

	void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("Player")) {
			on_trigger.Invoke();
		}
	}
}
