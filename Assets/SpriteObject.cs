using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteObject : MonoBehaviour {

	public Transform camera_transform;
	private Transform sprite_box_transform;
	private Transform sprite_transform;
	private SpriteRenderer sprite_renderer;

	public string[] directories;
	public int sprite_side_count;
	public List<Sprite[]> sprites = new List<Sprite[]>();
	public int anim = 0;

	// Start is called before the first frame update
	void Start() {
		sprite_box_transform = transform.GetChild(0);
		sprite_transform = sprite_box_transform.GetChild(0);
		sprite_renderer = sprite_transform.GetComponent<SpriteRenderer>();

		foreach (string directory in directories) {
			sprites.Add(Resources.LoadAll<Sprite>(directory + "/diffuse"));
		}
	}

	// Update is called once per frame
	void Update() {
		Vector3 angle = transform.eulerAngles;

		Vector3 camera_plane_position = camera_transform.position;
		camera_plane_position.y = 0;
		Vector3 plane_position = transform.position;
		plane_position.y = 0;

		float direction_angle_y = Quaternion.LookRotation((camera_plane_position - plane_position).normalized).eulerAngles.y;
		int angle_index = ((sprite_side_count * 2) - Mathf.RoundToInt((direction_angle_y - angle.y) / 45)) % sprite_side_count;
		sprite_box_transform.rotation = Quaternion.Euler(0, direction_angle_y + 180.0f, 0);
		sprite_renderer.sprite = sprites[anim][angle_index];
	}
}
