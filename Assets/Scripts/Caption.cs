using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Caption : MonoBehaviour {

	LinkedList<string> lines_buffer = new LinkedList<string>();
	LinkedList<string> lines_display = new LinkedList<string>();
	string line_buffer = "";

	int type_index = 0;

	const int MAX_LINE = 4;
	
	float type_time = 0.0f;
	float type_interval = 0.125f;

	TextMeshProUGUI caption_tmpro;

	void Start() {
		caption_tmpro = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
		lines_display.AddLast("");
	}

	void Update() {
		if (!string.IsNullOrEmpty(line_buffer)) {
			type_time -= Time.unscaledDeltaTime;
			if (type_time < 0) {
				lines_display.Last.Value += line_buffer[type_index];
				type_time += type_interval;
				type_index++;
				if (type_index >= line_buffer.Length) {
					if (lines_buffer.Count > 0) {
						line_buffer = new string(lines_buffer.Last.Value);
						lines_buffer.RemoveFirst();
					}
					else line_buffer = "";
					
					if (lines_display.Count > MAX_LINE) lines_display.RemoveFirst();
					lines_display.AddLast("");

					type_index = 0;
					type_time = 0.0f;
				}
				caption_tmpro.text = string.Join("\n", lines_display);
			}
		}
	}

	public void add_text(string str) {
		lines_buffer.AddFirst(str);
		if (string.IsNullOrEmpty(line_buffer)) {
			line_buffer = new string(lines_buffer.Last.Value);
			lines_buffer.RemoveFirst();
		}
	}
}
