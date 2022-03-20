using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WeaponHUDSprite : MonoBehaviour {
	RectTransform rt;
	void Start() {
		rt = gameObject.GetComponent<RectTransform>();
	}

	void Update() {
		float size = Screen.height / 96.0f;
		rt.localScale = new Vector3(size, size, 0);
	}
}
