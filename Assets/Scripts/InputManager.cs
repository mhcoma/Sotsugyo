using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	[System.Serializable]
	public class Button {
		public string button_name;
		public string primary_key;
		public string secondary_key;
	}

	public class Buttons {
		public Button[] buttons;
	}

	public struct key_pair {
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

	public struct axis_pair {
		public string positive_button_name;
		public string negative_button_name;
	}

	public static InputManager instance = null;
	public static Dictionary<string, key_pair> key_mapping = new Dictionary<string, key_pair>();
	public static Dictionary<string, axis_pair> axis_mapping = new Dictionary<string, axis_pair>();

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
		TextAsset key_mapping_file = Resources.Load<TextAsset>("Settings/button");
		Buttons key_mapping_list = JsonUtility.FromJson<Buttons>(key_mapping_file.text);

		foreach (Button k in key_mapping_list.buttons) {
			key_pair kp = new key_pair();
			kp.primary_key_code = (KeyCode) System.Enum.Parse(typeof(KeyCode), k.primary_key);
			kp.secondary_key_code = (KeyCode) System.Enum.Parse(typeof(KeyCode), k.secondary_key);
			key_mapping[k.button_name] = kp;
		}

		TextAsset axis_mapping_file = Resources.Load<TextAsset>("Settings/axis");
		Axises axis_mapping_list = JsonUtility.FromJson<Axises>(axis_mapping_file.text);

		foreach (Axis a in axis_mapping_list.axises) {
			axis_pair ap = new axis_pair();
			ap.positive_button_name = a.positive_button;
			ap.negative_button_name = a.negative_button;
			axis_mapping[a.axis_name] = ap;
		}

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
}
