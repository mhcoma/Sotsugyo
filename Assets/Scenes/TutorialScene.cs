using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : MonoBehaviour {

	void Start() {
		
	}

	void Update() {
		
	}

	public void laser_ammo_get_event() {
		
		GameManager.instance.caption_addtext(
			$"이것은 레이저의 탄환입니다.",
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

	public void health_pack_get_event(Door door) {
		GameManager.instance.caption_addtext("This is ");
		door.open();
	}
}
