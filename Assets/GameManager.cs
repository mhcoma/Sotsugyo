using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;

	public Transform pause_menu_transform;
	public Transform player_transform;
	Player player;

	public Transform camera_transform;

	Transform pause_group_transform;
	Transform option_group_transform;

	TextMeshProUGUI title_tmpro;
	TextMeshProUGUI music_volume_tmpro;
	TextMeshProUGUI effect_volume_tmpro;

	public AudioMixer mixer;
	float music_volume = 100;
	float effect_volume = 100;
	

	bool menu_toggle = false;

	public enum menu_state_enum {
		none,
		pause,
		option
	}

	public menu_state_enum menu_state = menu_state_enum.none;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Init();
		Debug.Log("Loaded!!\n");
	}

	void Init() {
		player = player_transform.GetComponent<Player>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		pause_group_transform = pause_menu_transform.Find("PauseGroup");
		option_group_transform = pause_menu_transform.Find("OptionGroup");
		title_tmpro = pause_menu_transform.Find("Title").GetComponent<TextMeshProUGUI>();

		music_volume_tmpro = option_group_transform.Find("MusicVolumeText").GetComponent<TextMeshProUGUI>();
		effect_volume_tmpro = option_group_transform.Find("EffectVolumeText").GetComponent<TextMeshProUGUI>();
	}

	void Update() {
		switch (menu_state) {
			case menu_state_enum.none:
				if (Input.GetButtonDown("Cancel")) {
					toggle_pause(true);
				}
				break;
			case menu_state_enum.pause:
				if (Input.GetButtonDown("Cancel")) {
					toggle_pause(false);
				}
				break;
			case menu_state_enum.option:
				if (Input.GetButtonDown("Cancel")) {
					toggle_option(false);
				}
				break;
		}
	}

	public void toggle_pause(bool toggle) {
		menu_toggle = toggle;
		pause_menu_transform.gameObject.SetActive(toggle);
		Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = toggle;
		Time.timeScale = toggle ? 0 : 1;
		player.set_controllable(!toggle);
		menu_state = toggle ? menu_state_enum.pause : menu_state_enum.none;
		title_tmpro.text = toggle ? "PAUSE" : "";
	}

	public void toggle_option(bool toggle) {
		pause_group_transform.gameObject.SetActive(!toggle);
		option_group_transform.gameObject.SetActive(toggle);
		menu_state = toggle ? menu_state_enum.option : menu_state_enum.pause;
		title_tmpro.text = toggle ? "OPTION" : "PAUSE";
	}

	public void change_music_volume(float volume) {
		music_volume = get_gain(volume);
		if (volume == 0) {
			music_volume = -80;
		}
		mixer.SetFloat("Music", music_volume);
		music_volume_tmpro.SetText($"Music Volume - {(int) volume}%");
	}
	public void change_effect_volume(float volume) {
		effect_volume = get_gain(volume);
		if (volume == 0) {
			effect_volume = -80;
		}
		mixer.SetFloat("Effect", effect_volume);
		effect_volume_tmpro.SetText($"Effect Volume - {(int) volume}%");
	}

	float get_gain(float volume) {
		return 20 * (Mathf.Log(volume / 100) / Mathf.Log(10));
	}

	public void restart_level() {
		player.rebirth();
		SceneManager.LoadScene("Scenes/SampleScene", LoadSceneMode.Single);
		
	}
}
