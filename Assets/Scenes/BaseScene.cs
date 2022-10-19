using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseScene : MonoBehaviour {

	public Transform first_floors_transform;
	public Transform second_floors_transform;
	public Transform roofs_transform;

	void Start() {
		Debug.Log(
			$"{GameManager.instance.map_index_x}, {GameManager.instance.map_index_y}"
		);

		MazeGenerator.GridNode node = GameManager.instance.get_current_node();

		MazeGenerator.floor_enum[] floor_enums = (MazeGenerator.floor_enum[]) Enum.GetValues(typeof(MazeGenerator.floor_enum));

		for (int i = 0; i < first_floors_transform.childCount; i++) {
			if ((node.first_floor & (int) floor_enums[i]) == 0) {
				first_floors_transform.GetChild(i).gameObject.SetActive(false);
			}
			if ((node.second_floor & (int) floor_enums[i]) == 0) {
				if (i % 2 == 1 || i == 4) {
					int temp;
					switch (i) {
						case 1: temp = 0; break;
						case 3: temp = 1; break;
						case 4: temp = 2; break;
						case 5: temp = 3; break;
						case 7: temp = 4; break;
						default: temp = -1; break;
					}
					second_floors_transform.GetChild(temp).gameObject.SetActive(false);
				}
			}
			if ((node.roof & (int) floor_enums[i]) == 0) {
				roofs_transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		NavMeshSurface surface = GetComponent<NavMeshSurface>();
		surface.RemoveData();
		surface.BuildNavMesh();
	}

	void Update() {

	}
}
