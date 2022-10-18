using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseScene : MonoBehaviour {

	public Transform floors_transform;
	

	void Start() {
		int count = floors_transform.childCount;
		for (int i = 0; i < count; i++) {
			Transform floor = floors_transform.GetChild(i);
			NavMeshSurface surface = floor.GetComponent<NavMeshSurface>();
			if (floor.gameObject.activeSelf) {
				surface.RemoveData();
				surface.BuildNavMesh();
			}
			else {
				// surface.
				surface.RemoveData();
			}
		}
	}

	void Update() {

	}
}
