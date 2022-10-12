using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	public static InputManager instance = null;

	[System.Serializable]
	public class Button {
		public string button_name;
		public string primary_key;
		public string secondary_key;
	}

	public class Buttons {
		public Button[] buttons;
	}

	public struct KeyPair {
		public KeyCode primary_key_code;
		public KeyCode secondary_key_code;
	}

	[System.Serializable]
	public class Axis {
		public string axis_name;
		public string positive_button;
		public string negative_button;
	}

	public class Axises {
		public Axis[] axises;
	}

	public struct AxisPair {
		public string positive_button_name;
		public string negative_button_name;
	}
	public static Dictionary<string, KeyPair> key_mapping = new Dictionary<string, KeyPair>();
	public static Dictionary<string, AxisPair> axis_mapping = new Dictionary<string, AxisPair>();

	public const string button_mapping_file_path = "button.json";

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
		FileInfo button_mapping_file_info = new FileInfo(button_mapping_file_path);
		
		if (button_mapping_file_info.Exists) {
			load_button_mapping(false);
		}
		else {
			load_button_mapping(true);
			save_button_mapping();
		}

		load_default_axis_mapping();
	}

	void Update() {
		
	}

	public static bool get_button(string button_name) {
		bool primary = Input.GetKey(get_button_primary_key_code(button_name));
		bool secondary = Input.GetKey(get_button_secondary_key_code(button_name));
		return primary || secondary;
	}

	public static bool get_button_down(string button_name) {
		bool primary = Input.GetKeyDown(get_button_primary_key_code(button_name));
		bool secondary = Input.GetKeyDown(get_button_secondary_key_code(button_name));
		return primary || secondary;
	}

	public static bool get_button_up(string button_name) {
		bool primary = Input.GetKeyUp(get_button_primary_key_code(button_name));
		bool secondary = Input.GetKeyUp(get_button_secondary_key_code(button_name));
		return primary || secondary;
	}

	public static KeyCode get_anykey_down() {
		if (Input.anyKeyDown) {
			foreach (KeyCode keycode in System.Enum.GetValues(typeof(KeyCode))) {
				if (Input.GetKey(keycode)) {
					return keycode;
				}
			}
		}
		return KeyCode.None;
	}

	public static KeyCode get_button_primary_key_code(string button_name) {
		return key_mapping[button_name].primary_key_code;
	}

	public static KeyCode get_button_secondary_key_code(string button_name) {
		return key_mapping[button_name].secondary_key_code;
	}

	public static string get_button_key_names(string button_name) {
		if (get_button_secondary_key_code(button_name) == KeyCode.None) {
			return $"{get_button_primary_key_code(button_name)}";
		}
		return $"{get_button_primary_key_code(button_name)} 또는 {get_button_secondary_key_code(button_name)}";
	}

	public static float get_axis(string axis_name) {
		float result = 0.0f;

		if (get_button(axis_mapping[axis_name].positive_button_name)) result += 1.0f;
		if (get_button(axis_mapping[axis_name].negative_button_name)) result -= 1.0f;
		
		return result;
	}

	public static string get_axis_positive_button(string axis_name) {
		return axis_mapping[axis_name].positive_button_name;
	}

	public static string get_axis_negative_button(string axis_name) {
		return axis_mapping[axis_name].negative_button_name;
	}

	public static string get_axis_positive_key_names(string axis_name) {
		return get_button_key_names(get_axis_positive_button(axis_name));
	}

	public static string get_axis_negative_key_names(string axis_name) {
		return get_button_key_names(get_axis_negative_button(axis_name));
	}

	public static void change_key(string button_name, KeyCode key, bool primary = true) {
		KeyPair temp = key_mapping[button_name];
		if (primary)
			temp.primary_key_code = key;
		else 
			temp.secondary_key_code = key;
	}

	public static void load_button_mapping(bool load_default) {
		string button_mapping_file;
		if (load_default) {
			TextAsset button_mapping_file_asset = Resources.Load<TextAsset>("Settings/button");
			button_mapping_file = button_mapping_file_asset.text;
		}
		else button_mapping_file = File.ReadAllText(button_mapping_file_path);
		
		Buttons button_mapping_list = JsonUtility.FromJson<Buttons>(button_mapping_file);

		foreach (Button b in button_mapping_list.buttons) {
			KeyPair kp = new KeyPair();
			kp.primary_key_code = (KeyCode) System.Enum.Parse(typeof(KeyCode), b.primary_key);
			kp.secondary_key_code = (KeyCode) System.Enum.Parse(typeof(KeyCode), b.secondary_key);
			key_mapping[b.button_name] = kp;
		}
	}

	public static void load_default_axis_mapping() {
		TextAsset axis_mapping_file = Resources.Load<TextAsset>("Settings/axis");
		Axises axis_mapping_list = JsonUtility.FromJson<Axises>(axis_mapping_file.text);

		foreach (Axis a in axis_mapping_list.axises) {
			AxisPair ap = new AxisPair();
			ap.positive_button_name = a.positive_button;
			ap.negative_button_name = a.negative_button;
			axis_mapping[a.axis_name] = ap;
		}
	}

	public static void save_button_mapping() {
		Buttons button_mapping_list = new Buttons();
		button_mapping_list.buttons = new Button[key_mapping.Count];

		int index = 0;
		foreach (KeyValuePair<string, KeyPair> pair in key_mapping) {
			Button b = new Button();
			KeyPair kp = pair.Value;
			b.primary_key = $"{kp.primary_key_code}";
			b.secondary_key = $"{kp.secondary_key_code}";
			b.button_name = pair.Key;
			button_mapping_list.buttons[index] = b;
			index++;
		}

		string button_mapping_file = JsonUtility.ToJson(button_mapping_list);
		File.WriteAllText(button_mapping_file_path, button_mapping_file);
	}
}
