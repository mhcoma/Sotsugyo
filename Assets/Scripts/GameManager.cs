using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
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

	[System.NonSerialized]
	public Transform player_spawn_point_transform;

	Transform caption_transform;


	Transform hud_transform;
	Transform crosshair_transform;

	
	bool caption_toggle = false;
	Caption caption;

	TextMeshProUGUI option_back_button_tmpro;
	TextMeshProUGUI input_option_back_button_tmpro;


	Slider music_volume_slider;
	Slider effect_volume_slider;
	TextMeshProUGUI title_tmpro;
	TextMeshProUGUI music_volume_tmpro;
	TextMeshProUGUI effect_volume_tmpro;

	public AudioMixer mixer;
	float music_volume = 100;
	float effect_volume = 100;

	float temp_music_volume = 100;
	float temp_effect_volume = 100;

	[System.Serializable]
	public class SoundVolume {
		public int effect_volume;
		public int music_volume;
	}

	
	[NonSerialized]
	public const string sound_volume_file_path = "music.json";

	public AudioClip[] play_bgms;
	int play_bgms_index = 0;
	public AudioClip tutorial_bgm;
	public AudioClip main_bgm;
	public AudioClip gameover_bgm;
	AudioSource asrc;
	bool is_gameover_bgm_played = false;

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

	
	Slider mouse_sensitivity_slider;
	TextMeshProUGUI mouse_sensitivity_tmpro;
	float temp_mouse_sensitivity = 0;
	float mouse_sensitivity_scale = 2.0f;
	
	[NonSerialized]
	public const string input_settings_file_path = "input.json";

	public class InputSettings {
		public float mouse_sensitivity;
	}

	List<string> button_names = new List<string> {
		"fire",
		"interact",
		"jump",
		"weapon 1",
		"weapon 2",
		"up",
		"down",
		"left",
		"right"
	};

	Dictionary<string, TextMeshProUGUI> key_button_texts = new Dictionary<string, TextMeshProUGUI>();


	Dictionary<string, KeyCode> temp_keys = new Dictionary<string, KeyCode>();
	string current_input_button_name = "";
	Transform input_panel;
	bool is_reset_buttons = false;

	bool menu_toggle = false;

	public enum menu_state_enum {
		none,
		main_menu,
		main_play,
		main_option,
		main_input_option,
		main_input_key,
		playing,
		pause,
		option,
		input_option,
		input_key,
		gameover
	}
	Transform main_menu_group_transform;
	Transform play_group_transform;
	Transform pause_group_transform;
	Transform option_group_transform;
	Transform input_option_group_transform;
	Transform gameover_group_transform;
	Transform next_level_button_transform;
	bool is_main_menu = true;
	string gameover_text = "GAME OVER";

	public enum gameover_state_enum {
		none,
		gameover,
		clear
	}

	gameover_state_enum gameover_state = gameover_state_enum.none;

	
	[NonSerialized]
	public menu_state_enum menu_state = menu_state_enum.none;

	public int map_size_x;
	public int map_size_y;
	
	[NonSerialized]
	public int map_index_x = 0;
	
	[NonSerialized]
	public int map_index_y = 0;
	string next_level_name = "";
	bool is_cleared_stage = false;
	bool is_maze_stage = false;
	
	[NonSerialized]
	public int cleared_stage_count = 0;

	[NonSerialized]
	public MazeGenerator.direction_enum start_dir;
	
	[NonSerialized]
	public MazeGenerator.direction_enum next_dir;


	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);
	}

	void Start() {
		menu_state = menu_state_enum.main_menu;
		toggle_playing(false);
		swap_clear_gameover(false);

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

		FileInfo music_volume_file_info = new FileInfo(sound_volume_file_path);

		if (music_volume_file_info.Exists) {
			load_sound_volume();
		}

		FileInfo input_settings_file_info = new FileInfo(input_settings_file_path);
		if (input_settings_file_info.Exists) {
			load_input_settings();
		}

		apply_option();
		apply_mouse_sensitivity();

		initialize_input_button_texts();
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Init();
	}

	void Init() {
		asrc = GetComponent<AudioSource>();

		player = player_transform.GetComponent<Player>();

		camera_transform = camera_holder_transform.Find("PlayerCamera");

		caption_transform = canvas_transform.Find("Caption");
		caption = caption_transform.GetComponent<Caption>();

		hud_transform = canvas_transform.Find("HUD");
		crosshair_transform = hud_transform.Find("Crosshair");

		pause_menu_transform = canvas_transform.Find("PauseMenu");

		main_menu_group_transform = pause_menu_transform.Find("MainMenuGroup");
		play_group_transform = pause_menu_transform.Find("PlayGroup");
		pause_group_transform = pause_menu_transform.Find("PauseGroup");
		option_group_transform = pause_menu_transform.Find("OptionGroup");
		input_option_group_transform = pause_menu_transform.Find("InputOptionGroup");
		gameover_group_transform = pause_menu_transform.Find("GameOverGroup");

		title_tmpro = pause_menu_transform.Find("Title").GetComponent<TextMeshProUGUI>();

		music_volume_slider = option_group_transform.Find("MusicVolumeSlider").GetComponent<Slider>();
		music_volume_tmpro = option_group_transform.Find("MusicVolumeText").GetComponent<TextMeshProUGUI>();

		effect_volume_slider = option_group_transform.Find("EffectVolumeSlider").GetComponent<Slider>();
		effect_volume_tmpro = option_group_transform.Find("EffectVolumeText").GetComponent<TextMeshProUGUI>();

		screen_res_slider = option_group_transform.Find("ScreenResSlider").GetComponent<Slider>();
		screen_res_tmpro = option_group_transform.Find("ScreenResText").GetComponent<TextMeshProUGUI>();

		fullscreen_slider = option_group_transform.Find("FullscreenSlider").GetComponent<Slider>();
		fullscreen_tmpro = option_group_transform.Find("FullscreenText").GetComponent<TextMeshProUGUI>();

		option_back_button_tmpro = option_group_transform.Find("Back").GetChild(0).GetComponent<TextMeshProUGUI>();

		input_option_back_button_tmpro = input_option_group_transform.Find("Back").GetChild(0).GetComponent<TextMeshProUGUI>();
		input_panel = input_option_group_transform.Find("InputPanel");

		mouse_sensitivity_slider = input_option_group_transform.Find("MouseSensivitySlider").GetComponent<Slider>();
		mouse_sensitivity_tmpro = input_option_group_transform.Find("MouseSensivityText").GetComponent<TextMeshProUGUI>();

		next_level_button_transform = gameover_group_transform.Find("NextLevelButton");

		key_button_texts.Clear();

		foreach (string str in button_names) {
			Char[] temp_arr = str.ToCharArray();
			temp_arr[0] = Char.ToUpper(temp_arr[0]);
			string temp_str = $"{temp_arr.ArrayToString().Replace(" ", "")}Button";
			
			Transform temp_button_transform = input_option_group_transform.Find(temp_str);
			Button temp_button = temp_button_transform.GetComponent<Button>();
			TextMeshProUGUI temp_tmpro = temp_button_transform.GetChild(0).GetComponent<TextMeshProUGUI>();

			temp_button.onClick.AddListener(
				delegate {
					toggle_input_key(str);
				}
			);

			key_button_texts.Add(str, temp_tmpro);
		}

		if (is_cleared_stage) {
			GameObject[] actors = GameObject.FindGameObjectsWithTag("Actor");
			foreach (GameObject obj in actors) {
				obj.SetActive(false);
			}

			GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
			foreach (GameObject obj in items) {
				obj.SetActive(false);
			}
		}

		player_spawn_point_transform = GameObject.Find("PlayerSpawnPoint").transform;
		player.reset_player_position();

		if (is_maze_stage) {
			
		}
	}

	int count = 0;

	void Update() {
		count++;
		switch (menu_state) {
			case menu_state_enum.main_menu:
				if (InputManager.get_button_down("cancel")) {
					quit();
				}
				break;
			case menu_state_enum.main_play:
				if (InputManager.get_button_down("cancel")) {
					swap_play_menu_main_menu(false);
				}
				break;
			case menu_state_enum.playing:
				if (InputManager.get_button_down("cancel")) {
					pause();
				}
				if (InputManager.get_button_down("debug_hud")) {
					toggle_hud();
				}
				if (InputManager.get_button_down("debug_invincivle")) {
					player.toggle_invincivle();
				}
				if (InputManager.get_button_down("debug_kill")) {
					kill_enemies();
				}
				break;
			case menu_state_enum.pause:
				if (InputManager.get_button_down("cancel")) {
					resume();
				}
				break;
			case menu_state_enum.main_option:
			case menu_state_enum.option:
				if (InputManager.get_button_down("cancel")) {
					toggle_option(false);
				}
				if (InputManager.get_button_down("submit")) {
					apply_option();
				}
				break;
			case menu_state_enum.main_input_option:
			case menu_state_enum.input_option:
				if (InputManager.get_button_down("cancel")) {
					toggle_input_option(false);
				}
				if (InputManager.get_button_down("submit")) {
					apply_input_option();
				}
				break;
			case menu_state_enum.main_input_key:
			case menu_state_enum.input_key:
				KeyCode keycode = InputManager.get_anykey_down();
				if (keycode != KeyCode.None) {
					get_input_key(current_input_button_name, keycode);
				}
				break;
			case menu_state_enum.gameover:
				if (InputManager.get_button_down("cancel")) {
					gameover_to_main_menu();
				}
				if (InputManager.get_button_down("submit")) {
					gameover_to_restart();
				}
				break;
		}

		switch (menu_state) {
			case menu_state_enum.main_menu:
			case menu_state_enum.main_play:
			case menu_state_enum.main_option:
			case menu_state_enum.main_input_option:
			case menu_state_enum.main_input_key:
				if (!asrc.isPlaying) {
					asrc.clip = main_bgm;
					asrc.Play();
				}
				break;
			case menu_state_enum.playing:
			case menu_state_enum.pause:
			case menu_state_enum.option:
			case menu_state_enum.input_option:
			case menu_state_enum.input_key:
				if (!asrc.isPlaying) {
					if (is_maze_stage) {
						play_bgms_index = (play_bgms_index + 1) % play_bgms.Length;
						asrc.clip = play_bgms[play_bgms_index];
					}
					else {
						asrc.clip = tutorial_bgm;
					}
					asrc.Play();
				}
				break;
			case menu_state_enum.gameover:
				if (!asrc.isPlaying && !is_gameover_bgm_played && gameover_state != gameover_state_enum.none) {
					if (gameover_state == gameover_state_enum.gameover)
						asrc.clip = gameover_bgm;
					else
						asrc.clip = main_bgm;
					asrc.Play();
					is_gameover_bgm_played = true;
				}
				break;
			default:
				break;
		}
	}

	public void start_tutorial() {
		start_level("Scenes/TutorialScene");
		map_index_x = -1;
		map_index_y = -1;
		next_level_name = "";
		player.last_weapon_index = Player.WeaponIndex.none;

		asrc.Stop();
	}

	public void start_maze() {
		MazeGenerator.generate_grid(map_size_x, map_size_y);
		map_index_x = map_size_x - 1;
		map_index_y = map_size_y - 1;

		start_level("Scenes/BaseScene");
		next_level_name = "Scenes/BaseScene";
		is_maze_stage = true;
		start_dir = MazeGenerator.direction_enum.south;
		player.last_weapon_index = Player.WeaponIndex.none;

		cleared_stage_count = 0;
		
		asrc.Stop();
	}

	public string maze_direction(int x, int y) {
		string result = "";
		int c = MazeGenerator.grid[y][x].dir;

		foreach(MazeGenerator.direction_enum dir in Enum.GetValues(typeof(MazeGenerator.direction_enum))) {
			if ((c & (int) dir) != 0) {
				result += $"{dir.ToString()[0]}";
			}
		}

		return result;
	}

	public void start_level(string level_name) {
		toggle_play_menu(false);
		toggle_playing(true);
		move_level(level_name);
		is_main_menu = false;
	}

	public void restart_level() {
		toggle_playing(true);
		move_level(SceneManager.GetActiveScene().name);
	}

	public void start_next_level() {
		toggle_playing(true);
		player.last_weapon_index = player.weapon_index;
		if (is_maze_stage) {
			if (!get_current_node().is_cleared) {
				get_current_node().set_cleared(true);
				cleared_stage_count++;
			}
			switch (next_dir) {
				case MazeGenerator.direction_enum.north:
					map_index_y -= 1;
					start_dir = MazeGenerator.direction_enum.south;
					break;
				case MazeGenerator.direction_enum.south:
					map_index_y += 1;
					start_dir = MazeGenerator.direction_enum.north;
					break;
				case MazeGenerator.direction_enum.east:
					map_index_x += 1;
					start_dir = MazeGenerator.direction_enum.west;
					break;
				case MazeGenerator.direction_enum.west:
					map_index_x -= 1;
					start_dir = MazeGenerator.direction_enum.east;
					break;
			}
		}
		move_level(next_level_name);
	}

	public void move_level(string level_name) {
		caption.clear();
		SceneManager.LoadScene(level_name, LoadSceneMode.Single);
		player.rebirth();
	}

	public void toggle_playing(bool toggle) {
		bool temp_tot = !toggle || caption_toggle;

		menu_toggle = !toggle;
		
		Cursor.lockState = !toggle ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = !toggle;
		Time.timeScale = temp_tot ? 0 : 1;
		player.set_controllable(!temp_tot);

		pause_menu_transform.gameObject.SetActive(!toggle);
		hud_transform.gameObject.SetActive(toggle);

		if (caption_toggle) {
			caption_transform.gameObject.SetActive(toggle);
		}

		if (toggle) {
			menu_state = menu_state_enum.playing;
		}
	}

	public void toggle_main_menu(bool toggle) {
		if (toggle) {
			move_level("Scenes/MainScene");
			menu_state = menu_state_enum.main_menu;
			title_tmpro.text = "";
			is_main_menu = true;
		}
		main_menu_group_transform.gameObject.SetActive(toggle);
	}

	public void toggle_pause(bool toggle) {
		pause_group_transform.gameObject.SetActive(toggle);
		if (toggle) {
			menu_state = menu_state_enum.pause;
			title_tmpro.text = "PAUSE";
		}
	}

	public void toggle_play_menu(bool toggle) {
		if (toggle) {
			menu_state = menu_state_enum.main_play;
			title_tmpro.text = "PLAY";
		}
		play_group_transform.gameObject.SetActive(toggle);
	}
	
	public void toggle_gameover(bool toggle) {
		if (gameover_state != gameover_state_enum.none) asrc.Stop();
		gameover_group_transform.gameObject.SetActive(toggle);
		if (toggle) {
			menu_state = menu_state_enum.gameover;
			title_tmpro.text = gameover_text;
		}
		else {
			swap_clear_gameover(false);
			is_gameover_bgm_played = false;
		}
	}

	public void swap_clear_gameover(bool toggle) {
		bool is_exist_next_level = !next_level_name.Equals("");
		next_level_button_transform.gameObject.SetActive(toggle && is_exist_next_level);
		gameover_text = toggle ? "CLEAR" : "GAME OVER";
		if (toggle) {
			if (is_last_stage())
				gameover_state = gameover_state_enum.clear;
			else
				gameover_state = gameover_state_enum.none;
		}
		else {
			gameover_state = gameover_state_enum.gameover;
		}
	}

	public void toggle_option(bool toggle) {
		option_group_transform.gameObject.SetActive(toggle);
		if (is_main_menu) toggle_main_menu(!toggle);
		else toggle_pause(!toggle);

		if (toggle) {
			menu_state = is_main_menu ? menu_state_enum.main_option : menu_state_enum.option;
			title_tmpro.text = "OPTION";
		}
		else {
			cancel_option();
		}
	}

	public void toggle_input_option(bool toggle) {
		option_group_transform.gameObject.SetActive(!toggle);
		input_option_group_transform.gameObject.SetActive(toggle);
		if (is_main_menu) {
			menu_state = toggle ? menu_state_enum.main_input_option : menu_state_enum.main_option;
		}
		else {
			menu_state = toggle ? menu_state_enum.input_option : menu_state_enum.option;
		}
		title_tmpro.text = toggle ? "INPUT OPTION" : "OPTION";

		if (!toggle) {
			cancel_input_option();
		}
	}

	public void resume() {
		menu_state = menu_state_enum.playing;
		toggle_pause(false);
		toggle_playing(true);
	}

	public void pause() {
		menu_state = menu_state_enum.pause;
		toggle_pause(true);
		toggle_playing(false);
	}

	public void playing_to_main_menu() {
		menu_state = menu_state_enum.main_menu;
		asrc.Stop();
	}

	public void gameover() {
		toggle_playing(false);
		toggle_gameover(true);
	}

	public void level_clear() {
		swap_clear_gameover(true);
		gameover();
	}

	public void swap_play_menu_main_menu(bool swap) {
		toggle_play_menu(swap);
		toggle_main_menu(!swap);
	}

	public void pause_to_restart() {
		toggle_pause(false);
		restart_level();
	}

	public void pause_to_main_menu() {
		toggle_pause(false);
		toggle_main_menu(true);
		asrc.Stop();
	}

	public void gameover_to_next_level() {
		toggle_gameover(false);
		start_next_level();
	}

	public void gameover_to_restart() {
		toggle_gameover(false);
		restart_level();
	}

	public void gameover_to_main_menu() {
		toggle_gameover(false);
		toggle_main_menu(true);
		asrc.Stop();
	}

	public void change_music_volume(float volume) {
		temp_music_volume = volume;
		mixer.SetFloat("Music", get_gain(volume));
		music_volume_tmpro.SetText($"Music Volume - {(int) volume}%");
		change_option_back_button_text(true);
		
	}
	public void change_effect_volume(float volume) {
		temp_effect_volume = volume;
		mixer.SetFloat("Effect", get_gain(volume));
		effect_volume_tmpro.SetText($"Effect Volume - {(int) volume}%");
		change_option_back_button_text(true);
	}

	public void load_sound_volume() {
		string sound_volume_file = File.ReadAllText(sound_volume_file_path);
		SoundVolume sound_volume = JsonUtility.FromJson<SoundVolume>(sound_volume_file);

		temp_music_volume = sound_volume.music_volume;
		temp_effect_volume = sound_volume.effect_volume;
	}
	
	public void save_sound_volume() {
		SoundVolume sound_volume = new SoundVolume();
		sound_volume.music_volume = (int) music_volume;
		sound_volume.effect_volume = (int) effect_volume;

		string sound_volume_file = JsonUtility.ToJson(sound_volume);
		File.WriteAllText(sound_volume_file_path, sound_volume_file);
	}

	public void change_screen_res(float index) {
		screen_res_tmpro.text = get_screen_res_text((int) index);
		temp_screen_res_index = (int) index;
		change_option_back_button_text(true);
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
		change_option_back_button_text(true);
	}

	public void apply_option() {
		screen_res_index = temp_screen_res_index;
		fullscreen_mode = temp_fullscreen_mode;

		effect_volume = temp_effect_volume;
		music_volume = temp_music_volume;
		effect_volume_slider.value = effect_volume;
		music_volume_slider.value = music_volume;

		Screen.SetResolution(screen_res_list[screen_res_index].width, screen_res_list[screen_res_index].height, fullscreen_mode);

		save_sound_volume();

		change_option_back_button_text(false);
	}

	public void cancel_option() {
		screen_res_slider.value = screen_res_index;
		fullscreen_slider.value = (float) fullscreen_mode;
		change_screen_res(screen_res_slider.value);
		change_fullscreen(fullscreen_slider.value);
		
		effect_volume_slider.value = effect_volume;
		music_volume_slider.value = music_volume;
		change_music_volume(music_volume);
		change_effect_volume(effect_volume);

		change_option_back_button_text(false);
	}

	void change_option_back_button_text(bool toggle) {
		if (toggle) option_back_button_tmpro.text = "Cancel";
		else option_back_button_tmpro.text = "Back";
	}

	void change_input_option_back_button_text(bool toggle) {
		if (toggle) input_option_back_button_tmpro.text = "Cancel";
		else input_option_back_button_tmpro.text = "Back";
	}

	string get_screen_res_text(int index) {
		return $"{screen_res_list[index].width}×{screen_res_list[index].height}";
	}

	float get_gain(float volume) {
		if (volume == 0.0f) return -80.0f;
		return 20 * (Mathf.Log(volume / 100) / Mathf.Log(10));
	}

	public void toggle_input_key(string button_name) {
		current_input_button_name = button_name;
		key_button_texts[button_name].text = "";
		menu_state = is_main_menu ? menu_state_enum.main_input_key : menu_state_enum.input_key;
		input_panel.gameObject.SetActive(true);
	}

	public void get_input_key(string button_name, KeyCode keycode) {
		foreach (string temp_button_name in button_names) {
			if (key_button_texts[temp_button_name].text.Equals($"{keycode}")) {
				key_button_texts[temp_button_name].text = "";
				set_temp_key(temp_button_name, KeyCode.None);
			}
		}

		key_button_texts[button_name].text = $"{keycode}";
		set_temp_key(button_name, keycode);
		change_input_option_back_button_text(true);
		menu_state = is_main_menu ? menu_state_enum.main_input_option : menu_state_enum.input_option;
		input_panel.gameObject.SetActive(false);
	}

	public void set_temp_key(string button_name, KeyCode keycode) {
		KeyCode temp_keycode;
		if (temp_keys.TryGetValue(button_name, out temp_keycode)) {
			temp_keys[button_name] = keycode;
		}
		else temp_keys.Add(button_name, keycode);
	}

	public void load_input_settings() {
		string input_settings_file = File.ReadAllText(input_settings_file_path);
		InputSettings input_settings = JsonUtility.FromJson<InputSettings>(input_settings_file);
		temp_mouse_sensitivity = input_settings.mouse_sensitivity;
	}

	public void save_input_settings() {
		InputSettings input_settings = new InputSettings();
		input_settings.mouse_sensitivity = player.mouse_sensitivity;

		string input_settings_file = JsonUtility.ToJson(input_settings);
		File.WriteAllText(input_settings_file_path, input_settings_file);
	}

	public void change_mouse_sensitivity(float scale) {
		temp_mouse_sensitivity = scale / mouse_sensitivity_scale;
		mouse_sensitivity_tmpro.SetText($"Mouse Sensivity - {temp_mouse_sensitivity:0.0}");
		change_input_option_back_button_text(true);
	}

	public void apply_mouse_sensitivity() {
		player.mouse_sensitivity = temp_mouse_sensitivity;
		mouse_sensitivity_slider.value = temp_mouse_sensitivity * mouse_sensitivity_scale;
		save_input_settings();
	}

	public void apply_input_option() {
		foreach (KeyValuePair<string, KeyCode> pair in temp_keys) {
			string button_name = pair.Key;
			InputManager.KeyPair kp = InputManager.key_mapping[button_name];
			kp.primary_key_code = pair.Value;
			InputManager.key_mapping[button_name] = kp;
		}
		temp_keys.Clear();
		caption.reset_caption_info();

		InputManager.save_button_mapping();
		
		apply_mouse_sensitivity();

		change_input_option_back_button_text(false);
	}

	public void cancel_input_option() {
		if (is_reset_buttons) {
			InputManager.load_button_mapping(false);
			is_reset_buttons = false;
		}
		initialize_input_button_texts();

		mouse_sensitivity_slider.value = player.mouse_sensitivity * mouse_sensitivity_scale;
		change_mouse_sensitivity(mouse_sensitivity_slider.value);

		change_input_option_back_button_text(false);
	}

	public void reset_input_option() {
		InputManager.load_button_mapping(true);
		initialize_input_button_texts();
		change_input_option_back_button_text(true);
		is_reset_buttons = true;
	}

	public void initialize_input_button_texts() {
		foreach (string button_name in button_names) {
			if (InputManager.get_button_primary_key_code(button_name) == KeyCode.None)
				key_button_texts[button_name].text = "";
			else
				key_button_texts[button_name].text = $"{InputManager.get_button_primary_key_code(button_name)}";
		}
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

	public void caption_addevent(UnityAction call) {
		caption.on_end.AddListener(call);
	}

	public MazeGenerator.GridNode get_current_node() {
		return get_node(map_index_x, map_index_y);
	}

	public MazeGenerator.GridNode get_node(int x, int y) {
		return MazeGenerator.grid[y][x];
	}

	public void toggle_hud() {
		bool result = true;
		if (hud_transform.gameObject.activeSelf) {
			result = false;
		}
		hud_transform.gameObject.SetActive(result);
	}

	public void kill_enemies() {
		GameObject[] actors = GameObject.FindGameObjectsWithTag("Actor");

		foreach (GameObject actor_gameobj in actors) {
			EnemyAITest ai;

			if (actor_gameobj.TryGetComponent<EnemyAITest>(out ai)) {
				actor_gameobj.GetComponent<SpriteObject>().get_damage(float.MaxValue);
			}
		}
	}

	public bool is_last_stage() {
		return cleared_stage_count == ((map_size_x * map_size_y) - 1);
	}

	public void set_last_stage() {
		
		next_level_name = "";
	}
}