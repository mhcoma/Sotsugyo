using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseScene : MonoBehaviour {
	GameManager gm;

	public Transform first_floors_transform;
	public Transform second_floors_transform;
	public Transform roofs_transform;
	public Transform stairs_transform;
	public Transform walls_transform;
	public Transform doors_transform;

	public Transform second_to_first_navmeshlinks_transform;

	public GameObject rocket_prefab;
	public GameObject laser_prefab;
	public GameObject healthpack_prefab;
	public GameObject key_prefab;
	public GameObject npc_prefab;
	public GameObject gas_prefab;
	public GameObject melee_prefab;
	public GameObject gunner_prefab;

	List<Transform> floor_transform_lists = new List<Transform>();

	MazeGenerator.direction_enum[] direction_enums;
	string next_doors = "";
	string next_cleared_doors = "";

	string[] door_colors = new string[] {
		"ff7f9f",
		"7f9fff",
		"ff9f7f",
		"9f7fff"
	};

	int enemy_count = 0;
	SpriteObject npc_sprite_obj;

	List<List<(int, int)>> navmeshlinks_lists = new List<List<(int, int)>> {
		new List<(int, int)> { (6, 3), (5, 2), (4, 1) },
		new List<(int, int)> { (9, 6), (8, 5), (7, 4) },
		new List<(int, int)> { (2, 1), (5, 4), (8, 7) },
		new List<(int, int)> { (3, 2), (6, 5), (9, 8) }
	};
	
	bool is_already_worked_start_event;

	Player player;

	void Start() {
		gm = GameManager.instance;

		MazeGenerator.GridNode node = GameManager.instance.get_current_node();

		// 미로 데이터에 따라 층, 지붕 생성
		for (int i = 0; i < 9; i++) {
			Transform first_floor = first_floors_transform.GetChild(i);
			Transform second_floor = second_floors_transform.GetChild(i);

			if (!MazeGenerator.is_exist_part(node.first_floor, (int) MazeGenerator.floor_enums[i]))
				first_floor.gameObject.SetActive(false);
			else floor_transform_lists.Add(first_floor);

			if (!MazeGenerator.is_exist_part(node.second_floor, (int) MazeGenerator.floor_enums[i]))
			// if (false)
				second_floor.gameObject.SetActive(false);
			else floor_transform_lists.Add(second_floor);

			if (!MazeGenerator.is_exist_part(node.roof, (int) MazeGenerator.floor_enums[i])) {
				roofs_transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
			}
		}

		// 미로 데이터에 따라 계단 생성, 계단이 위치한 층은 엔티티 생성하지 않음
		for (int i = 0; i < 9; i++) {
			MazeGenerator.floor_enum f = MazeGenerator.floor_enums[i];
			int temp_index;
			int stair_under_floor_index;

			switch (f) {
				case MazeGenerator.floor_enum.s: temp_index = 0; stair_under_floor_index = 0; break;
				case MazeGenerator.floor_enum.w: temp_index = 1; stair_under_floor_index = 6; break;
				case MazeGenerator.floor_enum.e: temp_index = 2; stair_under_floor_index = 2; break;
				case MazeGenerator.floor_enum.n: temp_index = 3; stair_under_floor_index = 8; break;
				default: temp_index = -1; stair_under_floor_index = -1; break;
			}
			
			if (temp_index >= 0) {
				Transform stair;
				Transform stair_under_floor;
				stair = stairs_transform.GetChild(temp_index);

				stair_under_floor = first_floors_transform.GetChild(stair_under_floor_index); 
				if (!MazeGenerator.is_exist_part(node.stair, (int) MazeGenerator.floor_enums[i]))
					stair.gameObject.SetActive(false);
				else {
					var a = floor_transform_lists.Find(x => x.name.Equals(stair_under_floor.name));
					floor_transform_lists.Remove(a);
				}
			}
		}

		// 미로 방향에 따른 벽 뚫기
		direction_enums = (MazeGenerator.direction_enum[]) Enum.GetValues(typeof(MazeGenerator.direction_enum));
		
		List<string> temp_doors = new List<string>();
		List<string> temp_cleared_doors = new List<string>();
		for (int i = 0; i < 4; i++) {
			bool temp = (node.dir & (int) direction_enums[i]) != 0;
			if (temp) {
				walls_transform.GetChild(i).gameObject.SetActive(false);

				int temp_index_x = gm.map_index_x;
				int temp_index_y = gm.map_index_y;

				switch (direction_enums[i]) {
					case MazeGenerator.direction_enum.north:
						temp_index_y -= 1;
						break;
					case MazeGenerator.direction_enum.south:
						temp_index_y += 1;
						break;
					case MazeGenerator.direction_enum.east:
						temp_index_x += 1;
						break;
					case MazeGenerator.direction_enum.west:
						temp_index_x -= 1;
						break;
				}

				MazeGenerator.GridNode temp_node = gm.get_node(temp_index_x, temp_index_y);
				if (temp_node.is_cleared) temp_cleared_doors.Add(get_door_name(i));
				else temp_doors.Add(get_door_name(i));
			}
		}

		next_doors = string.Join(", ", temp_doors);
		next_cleared_doors = string.Join(", ", temp_cleared_doors);

		// 층 생성에 따라 네비게이션 메시 링크 생성
		for (int i = 0; i < navmeshlinks_lists.Count; i++) {
			List<(int, int)> navmeshlinks_list = navmeshlinks_lists[i];

			Transform navmeshlinks_transform = second_to_first_navmeshlinks_transform.GetChild(i);

			NavMeshLink[] navmeshlinks = navmeshlinks_transform.GetComponents<NavMeshLink>();
			for (int j = 0; j < navmeshlinks_list.Count; j++) {
				NavMeshLink navmeshlink = navmeshlinks[j];
				(int, int) second_to_first = navmeshlinks_list[j];
				bool second_floor_active = second_floors_transform.GetChild(second_to_first.Item1 - 1).gameObject.activeSelf;
				bool first_floor_active = first_floors_transform.GetChild(second_to_first.Item2 - 1).gameObject.activeSelf;
				bool second_floor_on_first_floor_active = second_floors_transform.GetChild(second_to_first.Item2 - 1).gameObject.activeSelf;
				bool result = (second_floor_active && first_floor_active) && (!second_floor_on_first_floor_active);

				if (!result) navmeshlink.enabled = false;
			}

			for (int j = 0; j < (navmeshlinks_list.Count); j++) {
				NavMeshLink navmeshlink = navmeshlinks[j + 3];
				(int, int) first_from_second = navmeshlinks_list[j];
				bool second_floor_active = second_floors_transform.GetChild(first_from_second.Item2 - 1).gameObject.activeSelf;
				bool first_floor_active = first_floors_transform.GetChild(first_from_second.Item1 - 1).gameObject.activeSelf;
				bool second_floor_on_first_floor_active = second_floors_transform.GetChild(first_from_second.Item1 - 1).gameObject.activeSelf;
				bool result = (second_floor_active && first_floor_active) && (!second_floor_on_first_floor_active);

				if (!result) navmeshlink.enabled = false;
			}
		}

		// 동적 네비게이션 메시 생성
		NavMeshSurface surface = GetComponent<NavMeshSurface>();
		surface.RemoveData();
		surface.BuildNavMesh();

		// 플레이어 시작 위치 및 방향 설정, 초기 탄약 제공
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

		player = GameManager.instance.player_transform.GetComponent<Player>();
		temp_door_pos = doors_transform.GetChild(temp_door_index).GetComponent<Renderer>().bounds.center;
		GameManager.instance.player_spawn_point_transform.position = Vector3.Lerp(GameManager.instance.player_spawn_point_transform.position, temp_door_pos, 0.5f);

		GameManager.instance.player_spawn_point_transform.localEulerAngles = new Vector3(0, temp_angle, 0);

		player.reset_player_position();
		player.get_ammo(Player.WeaponIndex.lasergun, 500);
		player.get_ammo(Player.WeaponIndex.rocketlauncher, 25);

		// 등장 엔티티 무작위 생성
		if (node.is_cleared) {
			for (int i = 0; i < 4; i++) {
				doors_transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		else {
			int count;

			float difficulty = calculate_difficulty();

			// 무기 탄약 생성
			floor_transform_lists = MazeGenerator.shuffle(floor_transform_lists);
			count = 0;
			foreach (Transform floor_transform in floor_transform_lists) {
				if (count <= 3) {
					GameObject obj = GameObject.Instantiate(
						(count % 2 == UnityEngine.Random.Range(0, 2)) ? laser_prefab : rocket_prefab,
						get_random_pos_on_floor(floor_transform, false, true),
						Quaternion.identity
					);
				}
				count++;
			}

			// 적 생성
			floor_transform_lists = MazeGenerator.shuffle(floor_transform_lists);
			count = 0;
			foreach (Transform floor_transform in floor_transform_lists) {
				if (count <= UnityEngine.Random.Range(
					Mathf.Lerp(1, 2, difficulty),
					Mathf.Lerp(1, 4, difficulty)
				)) {
					GameObject obj = GameObject.Instantiate(
						(UnityEngine.Random.Range(0.0f, 1.0f) <= 0.5f) ? melee_prefab : gunner_prefab,
						get_random_pos_on_floor(floor_transform, true, true),
						Quaternion.identity
					);
					SpriteObject spriteobj = obj.GetComponent<SpriteObject>();
					spriteobj.health *= Mathf.Lerp(0.25f, 1.0f, difficulty);
					spriteobj.on_dead.AddListener(
						enemy_kill_event
					);
					EnemyAITest ai = obj.GetComponent<EnemyAITest>();
					ai.primary_attack_damage *= Mathf.Lerp(0.25f, 1.0f, difficulty);
					ai.secondary_attack_damage *= Mathf.Lerp(0.75f, 1.0f, difficulty);

					enemy_count++;
				}
				count++;
			}

			// 체력 팩 생성
			floor_transform_lists = MazeGenerator.shuffle(floor_transform_lists);
			count = 0;
			foreach (Transform floor_transform in floor_transform_lists) {
				if (count <= UnityEngine.Random.Range(
					Mathf.Lerp(5, 1, difficulty),
					Mathf.Lerp(10, 4, difficulty)
				)) {
					GameObject obj = GameObject.Instantiate(
						healthpack_prefab,
						get_random_pos_on_floor(floor_transform, false, true),
						Quaternion.identity
					);
				}
				count++;
			}

			// 가스통 생성
			floor_transform_lists = MazeGenerator.shuffle(floor_transform_lists);
			count = 0;
			foreach (Transform floor_transform in floor_transform_lists) {
				if (count <= 3) {
					for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++) {
						GameObject obj = GameObject.Instantiate(
							gas_prefab,
							get_random_pos_on_floor(floor_transform, true, true),
							Quaternion.identity
						);
					}
				}
				count++;
			}

			Transform random_floor_transform = floor_transform_lists[UnityEngine.Random.Range(0, floor_transform_lists.Count)];
			GameObject key_obj = GameObject.Instantiate(
				key_prefab,
				get_random_pos_on_floor(random_floor_transform, false, false),
				Quaternion.identity
			);

		}
	}

	void Update() {

	}

	public void start_event() {
		if (gm.cleared_stage_count == 0 && !is_already_worked_start_event) {
			string door_directions = "";

			for (int i = 0; i < 4; i++) {
				door_directions += get_door_name(i);
				if (i < 3) door_directions += ", ";
			}

			gm.caption_addtext(
				"게임의 맵은 여러 개의 방으로 구성된 미로 형식입니다.",
				$"미로는 총 {gm.map_size_x} × {gm.map_size_y} 크기이며,",
				$"사방에 {door_directions}의 방향 별 게이트가 있습니다."
			);
			gm.caption_addtext(
				"미로는 무작위로 생성되고 각 방의 형태도 무작위로 생성됩니다.",
				"미로에 있는 모든 <color=#ff6f6f>적</color>을 파괴하고 <color=#ffff00>열쇠</color>를 획득하세요."
			);
			gm.caption_addtext(
				"모든 적을 파괴하면 <color=#7fff3f>NPC</color>가 생성되고,",
				"<color=#ffff00>열쇠</color>가 있으면 상호작용을 통해 문을 열 수 있습니다.",
				"문을 통해 다음 방으로 넘어가면 체력과 탄알이 초기화됩니다."
			);
			is_already_worked_start_event = true;
		}
	}

	// 다음 스테이지 이동 이벤트
	public void next_stage_event(int temp) {
		gm.next_dir = (MazeGenerator.direction_enum) temp;
		gm.level_clear();
	}

	// NPC 상호작용 이벤트
	public void npc_interact_event() {
		if (player.has_key()) {
			if (gm.is_last_stage()) {
				gm.caption_addtext(
					"<color=#7fff3f>[Admin]</color>",
					"미로를 클리어했습니다!"
				);
				gm.caption_addevent(
					delegate {
						gm.set_last_stage();
						gm.level_clear();
					}
				);
			}
			else {
				string next_doors_text = next_doors.Equals("") ? "" : $"{next_doors} 방향으로 건너갈 수 있습니다.";
				string next_cleared_doors_text = next_cleared_doors.Equals("") ? "" : $"{next_cleared_doors} 방향은 클리어했습니다.";
				gm.caption_addtext(
					"<color=#7fff3f>[Admin]</color>",
					"다음 스테이지로 넘어가세요.",
					next_cleared_doors_text,
					next_doors_text
				);
				for (int i = 0; i < 4; i++) {
					doors_transform.GetChild(i).GetComponent<Door>().open();
				}
				npc_sprite_obj.kill(true);
				player.clear_item(Player.ItemIndex.key);
			}
		}
		else {
			gm.caption_addtext(
				"<color=#7fff3f>[Admin]</color>",
				"<color=#ffff00>열쇠</color>가 없으니 열어줄 수 없습니다."
			);
		}
	}

	// 적 사살 이벤트
	public void enemy_kill_event() {
		enemy_count--;

		if (enemy_count == 0) {
			Transform floor_transform = floor_transform_lists[UnityEngine.Random.Range(0, floor_transform_lists.Count)];
			GameObject obj = GameObject.Instantiate(
				npc_prefab,
				get_random_pos_on_floor(floor_transform, true, false),
				Quaternion.identity
			);
			npc_sprite_obj = obj.GetComponent<SpriteObject>();
			InteractableObject inter_obj = obj.GetComponent<InteractableObject>();
			inter_obj.on_interact.AddListener(npc_interact_event);
		}
	}

	// 오브젝트 생성할 무작위 위치 선택
	Vector3 get_random_pos_on_floor(Transform floor, bool type, bool randomize) {
		Bounds bounds = floor.GetComponent<Renderer>().bounds;
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;

		float half_x = size.x / 2;
		float half_z = size.z / 2;
		float height = type ? 4.0f : 1.5f;

		float lerp_z = randomize ? UnityEngine.Random.Range(0.0f, 1.0f) : 0.5f;
		float lerp_x = randomize ? UnityEngine.Random.Range(0.0f, 1.0f) : 0.5f;
		
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

	// 난이도 결정
	public float calculate_difficulty() {
		GameManager game_manager = GameManager.instance;
		float result = (game_manager.cleared_stage_count + 1.0f) / (game_manager.map_size_x * game_manager.map_size_y);
		return result;
	}

	string get_door_name(int i) {
		char[] door_name_cstr = direction_enums[i].ToString().ToCharArray();
		door_name_cstr[0] = Char.ToUpper(door_name_cstr[0]);
		string door_name = new string(door_name_cstr);
		return $"<color=#{door_colors[i]}>{door_name}</color>";
	}
}
