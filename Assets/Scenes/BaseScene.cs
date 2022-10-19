using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseScene : MonoBehaviour {

	public Transform first_floors_transform;
	public Transform second_floors_transform;
	public Transform roofs_transform;
	public Transform stairs_transform;
	public Transform walls_transform;

	void Start() {
		MazeGenerator.GridNode node = GameManager.instance.get_current_node();

		for (int i = 0; i < 9; i++) {
			MazeGenerator.floor_enum f = MazeGenerator.floor_enums[i];
			int temp_index;
			if (!MazeGenerator.is_exist_part(node.first_floor, (int) MazeGenerator.floor_enums[i])) {
				first_floors_transform.GetChild(i).gameObject.SetActive(false);
			}
			if (!MazeGenerator.is_exist_part(node.second_floor, (int) MazeGenerator.floor_enums[i])) {
				second_floors_transform.GetChild(i).gameObject.SetActive(false);
			}
			if (!MazeGenerator.is_exist_part(node.stair, (int) MazeGenerator.floor_enums[i])) {
				switch (f) {
					case MazeGenerator.floor_enum.s: temp_index = 0; break;
					case MazeGenerator.floor_enum.w: temp_index = 1; break;
					case MazeGenerator.floor_enum.e: temp_index = 2; break;
					case MazeGenerator.floor_enum.n: temp_index = 3; break;
					default: temp_index = -1; break;
				}
				if (temp_index >= 0) {
					stairs_transform.GetChild(temp_index).gameObject.SetActive(false);
				}
			}
			if (!MazeGenerator.is_exist_part(node.roof, (int) MazeGenerator.floor_enums[i])) {
				roofs_transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		MazeGenerator.direction_enum[] direction_enums = (MazeGenerator.direction_enum[]) Enum.GetValues(typeof(MazeGenerator.direction_enum));

		for (int i = 0; i < 4; i++) {
			if ((node.dir & (int) direction_enums[i]) == 0) {
				walls_transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		NavMeshSurface surface = GetComponent<NavMeshSurface>();
		surface.RemoveData();
		surface.BuildNavMesh();
	}

	void Update() {

	}
}
