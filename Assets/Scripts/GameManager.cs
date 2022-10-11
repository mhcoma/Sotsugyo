using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;

	public Transform canvas_transform;
	Transform pause_menu_transform;
	public Transform player_transform;
	Player player;

	public Transform camera_holder_transform;

	[System.NonSerialized]
	public Transform camera_transform;

	Transform player_spawn_point_transform;
	public Vector3 get_player_spawn_point {
		get { return player_spawn_point_transform.position; }
	}


	Transform pause_group_transform;
	Transform option_group_transform;
	Transform gameover_screen_transform;

	Transform caption_transform;

	Transform crosshair_transform;

	TextMeshProUGUI title_tmpro;
	TextMeshProUGUI music_volume_tmpro;
	TextMeshProUGUI effect_volume_tmpro;

	public AudioMixer mixer;
	float music_volume = 100;
	float effect_volume = 100;


	public struct ScreenRes {
		public int width;
		public int height;
		public ScreenRes(int w, int h) {
			width = w;
			height = h;
		}
	}

	List<ScreenRes> screen_res_list = new List<ScreenRes> {
		new ScreenRes(1280, 720),
		new ScreenRes(1920, 1080),
		new ScreenRes(2560, 1440),
		new ScreenRes(3840, 2160)
	};
	Slider screen_res_slider;
	Slider fullscreen_slider;
	TextMeshProUGUI screen_res_tmpro;
	TextMeshProUGUI fullscreen_tmpro;
	int screen_res_index = 0;
	int temp_screen_res_index = 0;
	FullScreenMode fullscreen_mode = FullScreenMode.ExclusiveFullScreen;
	FullScreenMode temp_fullscreen_mode = FullScreenMode.ExclusiveFullScreen;

	bool menu_toggle = false;
	bool caption_toggle = false;

	Caption caption;

	public enum menu_state_enum {
		none,
		pause,
		option,
		gameover
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
		ScreenRes screen_res = new ScreenRes(Screen.width, Screen.height);
		int temp_index = 0;

		for (int i = 0; i < screen_res_list.Count; i++) {
			ScreenRes temp_res = screen_res_list[i];
			if (temp_res.width == screen_res.width) {
				temp_index = i;
				if (temp_res.height == screen_res.height) {
					screen_res_index = i;
					break;
				}
			}
		}
		if (temp_index != screen_res_index) screen_res_index = temp_index;

		screen_res_slider.maxValue = screen_res_list.Count - 1;
		screen_res_slider.value = (int) screen_res_index;
		change_screen_res(screen_res_slider.value);

		fullscreen_slider.maxValue = System.Enum.GetNames(typeof(FullScreenMode)).Length - 2;
		fullscreen_slider.value = (float) Screen.fullScreenMode;
		if (fullscreen_slider.value >= (float) FullScreenMode.MaximizedWindow) fullscreen_slider.value += 1.0f;
		change_fullscreen(fullscreen_slider.value);
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Init();
		Debug.Log("init");
	}

	void Init() {
		player = player_transform.GetComponent<Player>();

		camera_transform = camera_holder_transform.Find("PlayerCamera");

		player_spawn_point_transform = transform.Find("PlayerSpawnPoint");

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		pause_menu_transform = canvas_transform.Find("PauseMenu");

		pause_group_transform = pause_menu_transform.Find("PauseGroup");
		option_group_transform = pause_menu_transform.Find("OptionGroup");
		gameover_screen_transform = pause_menu_transform.Find("GameOverGroup");
		title_tmpro = pause_menu_transform.Find("Title").GetComponent<TextMeshProUGUI>();

		music_volume_tmpro = option_group_transform.Find("MusicVolumeText").GetComponent<TextMeshProUGUI>();
		effect_volume_tmpro = option_group_transform.Find("EffectVolumeText").GetComponent<TextMeshProUGUI>();

		caption_transform = canvas_transform.Find("Caption");
		caption = caption_transform.GetComponent<Caption>();

		crosshair_transform = canvas_transform.Find("Crosshair");



		screen_res_slider = option_group_transform.Find("ScreenResSlider").GetComponent<Slider>();
		screen_res_tmpro = option_group_transform.Find("ScreenResText").GetComponent<TextMeshProUGUI>();

		fullscreen_slider = option_group_transform.Find("FullscreenSlider").GetComponent<Slider>();
		fullscreen_tmpro = option_group_transform.Find("FullscreenText").GetComponent<TextMeshProUGUI>();
	}

	void Update() {
		switch (menu_state) {
			case menu_state_enum.none:
				if (InputManager.get_button_down("cancel")) {
					toggle_pause(true);
				}
				break;
			case menu_state_enum.pause:
				if (InputManager.get_button_down("cancel")) {
					toggle_pause(false);
				}
				break;
			case menu_state_enum.option:
				if (InputManager.get_button_down("cancel")) {
					toggle_option(false);
				}
				break;
			case menu_state_enum.gameover:
				if (InputManager.get_button_down("cancel")) {
					
				}
				break;
		}
	}

	public void toggle_pause(bool toggle) {
		bool temp_tot = toggle || caption_toggle;
		menu_toggle = toggle;
		pause_menu_transform.gameObject.SetActive(toggle);
		Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = toggle;
		Time.timeScale = temp_tot ? 0 : 1;
		player.set_controllable(!temp_tot);
		menu_state = toggle ? menu_state_enum.pause : menu_state_enum.none;
		title_tmpro.text = toggle ? "PAUSE" : "";
	}

	public void toggle_option(bool toggle) {
		pause_group_transform.gameObject.SetActive(!toggle);
		option_group_transform.gameObject.SetActive(toggle);
		menu_state = toggle ? menu_state_enum.option : menu_state_enum.pause;
		title_tmpro.text = toggle ? "OPTION" : "PAUSE";

		if (!toggle) {
			screen_res_slider.value = screen_res_index;
			fullscreen_slider.value = (float) fullscreen_mode;
			change_screen_res(screen_res_slider.value);
			change_fullscreen(fullscreen_slider.value);
		}
	}
	
	public void toggle_gameover(bool toggle) {
		bool temp_tot = toggle || caption_toggle;
		menu_toggle = toggle;
		pause_menu_transform.gameObject.SetActive(toggle);
		Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = toggle;
		Time.timeScale = temp_tot ? 0 : 1;
		player.set_controllable(!temp_tot);

		gameover_screen_transform.gameObject.SetActive(toggle);
		pause_group_transform.gameObject.SetActive(!toggle);
		option_group_transform.gameObject.SetActive(false);
		menu_state = toggle ? menu_state_enum.gameover : menu_state_enum.none;
		title_tmpro.text = toggle ? "GAME OVER" : "";
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

	public void change_screen_res(float index) {
		screen_res_tmpro.text = get_screen_res_text((int) index);
		temp_screen_res_index = (int) index;
	}

	public void change_fullscreen(float index) {
		foreach (FullScreenMode mode in System.Enum.GetValues(typeof(FullScreenMode))) {
			if ((FullScreenMode) index == FullScreenMode.MaximizedWindow) index += 1;
			if ((int) index == (int) mode) {
				temp_fullscreen_mode = mode;
				break;
			}
		}
		fullscreen_tmpro.text = System.Enum.GetName(typeof(FullScreenMode), temp_fullscreen_mode);
	}

	public void apply_screen_res() {
		screen_res_index = temp_screen_res_index;
		fullscreen_mode = temp_fullscreen_mode;
		Screen.SetResolution(screen_res_list[screen_res_index].width, screen_res_list[screen_res_index].height, fullscreen_mode);
	}

	string get_screen_res_text(int index) {
		return $"{screen_res_list[index].width}×{screen_res_list[index].height}";
	}

	float get_gain(float volume) {
		return 20 * (Mathf.Log(volume / 100) / Mathf.Log(10));
	}

	public void restart_level() {
		move_level(SceneManager.GetActiveScene().name);
	}

	public void move_level(string level_name) {
		player.rebirth();
		toggle_gameover(false);
		caption.clear();
		SceneManager.LoadScene(level_name, LoadSceneMode.Single);
	}

	public void quit() {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public void toggle_caption(bool toggle) {
		bool temp_tot = toggle || menu_toggle;
		caption_toggle = toggle;
		Time.timeScale = temp_tot ? 0 : 1;
		player.set_controllable(!temp_tot);
		caption_transform.gameObject.SetActive(toggle);
	}

	public void caption_addtext(params string[] strs) {
		toggle_caption(true);

		caption.add_text(string.Join("\n", strs));
	}

	public bool controlable() {
		return player.controllable;
	}
}