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
	public Transform doors_transform;

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
				roofs_transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
			}
		}

		MazeGenerator.direction_enum[] direction_enums = (MazeGenerator.direction_enum[]) Enum.GetValues(typeof(MazeGenerator.direction_enum));

		
		string temp_string = $"Current Pos = ({GameManager.instance.map_index_x}, {GameManager.instance.map_index_y}), Next Doors :";
		for (int i = 0; i < 4; i++) {
			bool temp = (node.dir & (int) direction_enums[i]) != 0;
			if (temp) {
				walls_transform.GetChild(i).gameObject.SetActive(false);
				temp_string += $" {direction_enums[i]}";
			}
		}
		Debug.Log(temp_string);

		NavMeshSurface surface = GetComponent<NavMeshSurface>();
		surface.RemoveData();
		surface.BuildNavMesh();


		Vector3 temp_door_pos;
		int temp_door_index = 0;

		float temp_angle = 90.0f;;
		switch (GameManager.instance.start_dir) {
			case MazeGenerator.direction_enum.east:
				temp_door_index = 2;
				temp_angle += 90.0f;
				break;
			case MazeGenerator.direction_enum.south:
				temp_door_index = 1;
				temp_angle += 180.0f;
				break;
			case MazeGenerator.direction_enum.west:
				temp_door_index = 3;
				temp_angle += 270.0f;
				break;
		}
		temp_door_pos = doors_transform.GetChild(temp_door_index).GetComponent<Renderer>().bounds.center;
		GameManager.instance.player_spawn_point_transform.position = Vector3.Lerp(GameManager.instance.player_spawn_point_transform.position, temp_door_pos, 0.5f);

		GameManager.instance.player_spawn_point_transform.localEulerAngles = new Vector3(0, temp_angle, 0);

		GameManager.instance.player_transform.GetComponent<Player>().reset_player_position();
	}

	void Update() {

	}

	public void next_stage_event(int temp) {
		GameManager gm = GameManager.instance;
		gm.next_dir = (MazeGenerator.direction_enum) temp;
		gm.level_clear();
	}
}
