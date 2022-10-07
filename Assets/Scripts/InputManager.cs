using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	[System.Serializable]
	public class Key {
		public string button_name;
		public string primary_key;
		public string secondary_key;
	}

	public class Keys {
		public Key[] keys;
	}

	public struct key_pair {
		public KeyCode primary;
		public KeyCode secondary;
	}

	public static InputManager instance = null;
	public static Dictionary<string, key_pair> key_mapping = new Dictionary<string, key_pair>();

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
		TextAsset key_mapping_file = Resources.Load<TextAsset>("Settings/key");
		Keys key_mapping_list = JsonUtility.FromJson<Keys>(key_mapping_file.text);

		foreach (Key k in key_mapping_list.keys) {
			key_pair kp = new key_pair();
			kp.primary = (KeyCode) System.Enum.Parse(typeof(KeyCode), k.primary_key);
			kp.secondary = (KeyCode) System.Enum.Parse(typeof(KeyCode), k.secondary_key);
			key_mapping[k.button_name] = kp;
		}
	}

	void Update() {
		
	}

	public static bool get_key_down(string button_name) {
		bool primary = Input.GetKeyDown(primary_key(button_name));
		bool secondary = Input.GetKeyDown(secondary_key(button_name));
		return primary || secondary;
	}

	public static bool get_key_up(string button_name) {
		bool primary = Input.GetKeyUp(primary_key(button_name));
		bool secondary = Input.GetKeyUp(secondary_key(button_name));
		return primary || secondary;
	}

	public static KeyCode primary_key(string button_name) {
		return key_mapping[button_name].primary;
	}

	public static KeyCode secondary_key(string button_name) {
		return key_mapping[button_name].secondary;
	}

	public static string key_name(string button_name) {
		if (secondary_key(button_name) == KeyCode.None) {
			return $"{primary_key(button_name)}";
		}
		return $"{primary_key(button_name)} or {secondary_key(button_name)}";
	}
}
