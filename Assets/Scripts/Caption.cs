using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Caption : MonoBehaviour {

	// LinkedList<string> lines_buffer = new LinkedList<string>();
	// LinkedList<string> lines_display = new LinkedList<string>();
	// string line_buffer = "";

	// int type_index = 0;

	// const int MAX_LINE = 4;
	
	// float type_time = 0.0f;
	// float type_interval = 0.125f;

	// TextMeshProUGUI caption_tmpro;

	Queue<string> text_queue = new Queue<string>();
	string display_text;
	string buffer_text;

	int type_index = 0;
	float type_time = 0.0f;
	float type_interval = 0.015625f;

	TextMeshProUGUI caption_tmpro;

	void Start() {
		caption_tmpro = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
	}

	void Update() {
		if (GameManager.instance.menu_state == GameManager.menu_state_enum.none) {
			if (!string.IsNullOrEmpty(buffer_text)) {
				if (type_index < buffer_text.Length) {
					if (is_pressed_skip_button()) {
						caption_tmpro.text = buffer_text;
						type_index = buffer_text.Length;
					}
					else {
						type_time -= Time.unscaledDeltaTime;
						if (type_time < 0) {
							caption_tmpro.text += buffer_text[type_index];
							type_time += type_interval;
							type_index++;
						}
					}
				}
				else {
					if (is_pressed_skip_button()) {
						skip_text();
					}
				}
			}
			else {
				skip_text();
			}
		}
	}

	public void add_text(string str) {
		text_queue.Enqueue(str);
	}

	void skip_text() {
		caption_tmpro.text = "";
		if (text_queue.TryDequeue(out buffer_text)) {
		}
		else {
			clear();
			GameManager.instance.toggle_caption(false);
		}
		type_index = 0;
		type_time = 0.0f;
	}

	public void clear() {
		
	}

	bool is_pressed_skip_button() {
		return InputManager.get_button_down("submit") || InputManager.get_button_down("fire");
	}
}
