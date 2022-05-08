using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class WeaponHUDSprite : MonoBehaviour {
	RectTransform rt;

	float anim_pos_y = 0;
	int anim_y_direction = 0;
	float anim_speed;

	Sprite next_sprite;

	void Start() {
		rt = GetComponent<RectTransform>();
	}

	void Update() {
		float size = Screen.height / 96.0f;
		rt.localScale = new Vector3(size, size, 0);
		anim_speed = size * 300;

		switch (anim_y_direction) {
			case 1:
				anim_pos_y += anim_speed * Time.deltaTime;
				if (anim_pos_y >= 0) {
					anim_y_direction = 0;
					anim_pos_y = 0;
				}
				break;
			case -1:
				anim_pos_y -= anim_speed * Time.deltaTime;
				if (anim_pos_y <= -(Screen.height / 2.0f)) {
					anim_y_direction = 1;
					GetComponent<CanvasRenderer>().SetTexture(next_sprite.texture);
				}
				break;
		}

		float pos_y = anim_pos_y + (Screen.height / 2.0f);

		rt.position = new Vector3(rt.position.x, pos_y, rt.position.z);
	}

	public void chnage_weapon_sprite(Sprite sprite) {
		anim_y_direction = -1;
		next_sprite = sprite;
	}

	public bool is_changing_weapon() {
		return anim_pos_y != 0;
	}
}
