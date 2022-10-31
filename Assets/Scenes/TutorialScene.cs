using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : MonoBehaviour {

	bool is_worked_start_event;

	void Start() {
		
	}

	void Update() {
		
	}

	public void start_event() {
		if (!is_worked_start_event) {
			
			GameManager.instance.caption_addtext(
				"환영합니다.",
				"튜토리얼을 시작하겠습니다."
			);
			GameManager.instance.caption_addtext(
				"우선 이동 조작법입니다.",
				$"[{InputManager.get_axis_positive_key_names("vertical")}] 키를 눌러 앞으로 이동할 수 있습니다.",
				$"[{InputManager.get_axis_negative_key_names("vertical")}] 키를 눌러 뒤로 이동할 수 있습니다."
			);
			GameManager.instance.caption_addtext(
				$"[{InputManager.get_axis_positive_key_names("horizontal")}] 키를 눌러 오른쪽으로 이동할 수 있습니다.",
				$"[{InputManager.get_axis_negative_key_names("horizontal")}] 키를 눌러 왼쪽으로 이동할 수 있습니다."
			);
			GameManager.instance.caption_addtext(
				"이동하여 언덕 위로 올라가 초록색의 물체에 다가가세요.",
				"아이템을 획득할 수 있습니다."
			);

			is_worked_start_event = true;
		}
	}

	public void laser_ammo_get_event() {
		
		GameManager.instance.caption_addtext(
			"이것은 레이저의 탄환입니다.",
			$"[{InputManager.get_button_key_names("weapon 1")}] 키를 눌러 레이저를 선택할 수 있습니다.",
			$"[{InputManager.get_button_key_names("fire")}] 키를 눌러 발사할 수 있습니다."
		);
		GameManager.instance.caption_addtext(
			$"함정에 빠지면 지속적으로 체력이 감소합니다",
			$"[{InputManager.get_button_key_names("jump")}] 키를 눌러 점프하여 건너가세요."
		);
		GameManager.instance.caption_addtext(
			$"레이저로 앞의 적을 죽이세요."
		);
	}

	public void rocket_ammo_get_event(GameObject gas) {
		GameManager.instance.caption_addtext(
			"이것은 로켓 런처의 탄환입니다.",
			$"[{InputManager.get_button_key_names("weapon 2")}] 키를 눌러 로켓 런처를 선택할 수 있습니다."
		);
		GameManager.instance.caption_addtext(
			"가스통에 피해를 입히면 폭발하여 주변을 밀쳐내고 피해를 입힙니다.",
			"점프를 하고 가스통이나 로켓을 터뜨리면,",
			"그 반동으로 높게 점프할 수 있습니다."
		);
		GameManager.instance.caption_addtext(
			"점프 후 가스통을 터뜨려 벽을 올라가세요.",
			"실패해도 로켓 런처를 이용해 올라갈 수 있습니다."
		);
		gas.SetActive(true);
	}

	public void health_pack_get_event(Door door) {
		GameManager.instance.caption_addtext(
			"이것은 체력 아이템입니다.",
			"획득하면 체력을 50 회복합니다."
		);
		GameManager.instance.caption_addtext(
			"잠시 후 벽이 열리면 로켓 발사형 적이 등장합니다.",
			"적은 플레이어의 위치를 예측하여 로켓을 발사하므로,",
			"잘 피하면서 적을 사살하세요."
		);
		door.open();
	}

	public void gunner_death_event(GameObject npc) {
		GameManager.instance.caption_addtext(
			"NPC가 생성될 것입니다.",
			$"[{InputManager.get_button_key_names("interact")}] 키를 눌러 상호작용할 수 있습니다."
		);
		npc.SetActive(true);
	}

	public void interact_event() {
		GameManager.instance.caption_addtext(
			"튜토리얼을 완료했습니다.",
			"축하합니다."
		);
		GameManager.instance.caption_addevent(
			delegate {
				GameManager.instance.level_clear();
			}
		);
	}
}