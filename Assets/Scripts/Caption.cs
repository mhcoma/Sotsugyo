using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Caption : MonoBehaviour {
	Queue<string> text_queue = new Queue<string>();
	string buffer_text;

	int type_index = 0;
	float type_time = 0.0f;
	float type_interval = 0.015625f;

	TextMeshProUGUI caption_tmpro;
	TextMeshProUGUI skip_tmpro;

	public UnityEvent on_end;

	void Start() {
		caption_tmpro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
		skip_tmpro = transform.Find("Skip").GetComponent<TextMeshProUGUI>();

		reset_caption_info();
	}

	void Update() {
		if (GameManager.instance.menu_state == GameManager.menu_state_enum.playing) {
			if (!string.IsNullOrEmpty(buffer_text)) {
				if (type_index < buffer_text.Length) {
					if (is_pressed_skip_button()) {
						caption_tmpro.text = buffer_text;
						type_index = buffer_text.Length;
					}
					else {
						type_time -= Time.unscaledDeltaTime;
						if (type_time < 0) {
							char c;
							c = buffer_text[type_index];
							caption_tmpro.text += c;
							if (c == '<') {
								do {
									type_index++;
									c = buffer_text[type_index];
									caption_tmpro.text += c;
								} while (c != '>');
							}
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
		if (caption_tmpro != null)
			caption_tmpro.text = "";
		if (!text_queue.TryDequeue(out buffer_text)) {
			GameManager.instance.toggle_caption(false);
			on_end.Invoke();
			on_end.RemoveAllListeners();
		}
		type_index = 0;
		type_time = 0.0f;
	}

	public void clear() {
		text_queue.Clear();
		skip_text();
	}

	bool is_pressed_skip_button() {
		return InputManager.get_button_down("submit") || InputManager.get_button_down("fire");
	}

	public void reset_caption_info() {
		if (skip_tmpro != null)
		skip_tmpro.text = $"[{InputManager.get_button_key_names("submit")} 또는 {InputManager.get_button_key_names("fire")}] 키를 눌러 넘기기...";
	}
}
