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

	public GameObject rocket_prefab;
	public GameObject laser_prefab;
	public GameObject healthpack_prefab;
	public GameObject key_prefab;
	public GameObject npc_prefab;
	public GameObject gas_prefab;
	public GameObject melee_prefab;
	public GameObject gunner_prefab;

	List<Transform> floor_lists = new List<Transform>();

	void Start() {
		MazeGenerator.GridNode node = GameManager.instance.get_current_node();

		// 미로 데이터에 따라 층, 계단, 지붕 생성
		for (int i = 0; i < 9; i++) {
			MazeGenerator.floor_enum f = MazeGenerator.floor_enums[i];
			int temp_index;
			int stair_under_floor_index;

			Transform first_floor = first_floors_transform.GetChild(i);
			Transform second_floor = second_floors_transform.GetChild(i);
			

			switch (f) {
				case MazeGenerator.floor_enum.s: temp_index = 0; stair_under_floor_index = 0; break;
				case MazeGenerator.floor_enum.w: temp_index = 1; stair_under_floor_index = 6; break;
				case MazeGenerator.floor_enum.e: temp_index = 2; stair_under_floor_index = 2; break;
				case MazeGenerator.floor_enum.n: temp_index = 3; stair_under_floor_index = 8; break;
				default: temp_index = -1; stair_under_floor_index = -1; break;
			}

			if (!MazeGenerator.is_exist_part(node.first_floor, (int) MazeGenerator.floor_enums[i]))
				first_floor.gameObject.SetActive(false);
			else floor_lists.Add(first_floor);

			if (!MazeGenerator.is_exist_part(node.second_floor, (int) MazeGenerator.floor_enums[i]))
				second_floor.gameObject.SetActive(false);
			else floor_lists.Add(second_floor);
			
			if (temp_index >= 0) {
				Transform stair;
				Transform stair_under_floor;
				stair = stairs_transform.GetChild(temp_index);

				stair_under_floor = first_floors_transform.GetChild(stair_under_floor_index); 
				if (!MazeGenerator.is_exist_part(node.stair, (int) MazeGenerator.floor_enums[i]))
					stair.gameObject.SetActive(false);
				else {
					var a = floor_lists.Find(x => x.name.Equals(stair_under_floor.name));
					Debug.Log(a);
				}
			}

			if (!MazeGenerator.is_exist_part(node.roof, (int) MazeGenerator.floor_enums[i])) {
				roofs_transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
			}
		}

		// 미로 방향에 따른 벽 뚫기
		MazeGenerator.direction_enum[] direction_enums = (MazeGenerator.direction_enum[]) Enum.GetValues(typeof(MazeGenerator.direction_enum));
		
		string temp_string = $"Current Pos = ({GameManager.instance.map_index_x}, {GameManager.instance.map_index_y}), Next Doors :";
		for (int i = 0; i < 4; i++) {
			bool temp = (node.dir & (int) direction_enums[i]) != 0;
			if (temp) {
				walls_transform.GetChild(i).gameObject.SetActive(false);
				temp_string += $" {direction_enums[i]}";
			}
		}
		// Debug.Log(temp_string);

		// 동적 네비게이션 메시 생성
		NavMeshSurface surface = GetComponent<NavMeshSurface>();
		surface.RemoveData();
		surface.BuildNavMesh();

		// 플레이어 시작 위치 및 방향 설정
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

		// 등장 요소 무작위 생성
		if (!node.is_cleared) {
			floor_lists = MazeGenerator.shuffle(floor_lists);


			
			int count = 0;

			bool is_enemy_already_spawned = false;

			foreach (Transform floor in floor_lists) {
				if (true) {
					GameObject.Instantiate(
						(count % 2 == UnityEngine.Random.Range(0, 2)) ? laser_prefab : rocket_prefab,
						get_random_pos_on_floor(floor, false),
						Quaternion.identity
					);
				}
				// if ((UnityEngine.Random.Range(0.0f, 1.0f) >= 0.5f) || !is_enemy_already_spawned) {
				// 	GameObject.Instantiate(
				// 		(count % 2 == 1) ? melee_prefab : gunner_prefab,
				// 		get_random_pos_on_floor(floor, true),
				// 		Quaternion.identity
				// 	);
				// 	is_enemy_already_spawned = true;
				// }
				count++;
			}
		}
	}

	void Update() {

	}

	public void next_stage_event(int temp) {
		GameManager gm = GameManager.instance;
		gm.next_dir = (MazeGenerator.direction_enum) temp;
		gm.level_clear();
	}

	Vector3 get_random_pos_on_floor(Transform floor, bool type) {
		Bounds bounds = floor.GetComponent<Renderer>().bounds;
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;

		float half_x = size.x / 2;
		float half_z = size.z / 2;
		float height = type ? 4.0f : 1.5f;

		float lerp_z = UnityEngine.Random.Range(0.0f, 1.0f);
		float lerp_x = UnityEngine.Random.Range(0.0f, 1.0f);
		
		Vector3 result = Vector3.Lerp(
			Vector3.Lerp(
				center + new Vector3(half_x, height, half_z),
				center + new Vector3(half_x, height, -half_z),
				lerp_z
			),
			Vector3.Lerp(
				center + new Vector3(-half_x, height, half_z),
				center + new Vector3(-half_x, height, -half_z),
				lerp_z
			),
			lerp_x
		);

		return result;
	}
}
