using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	public enum direction_enum {
		north = 1,
		south = 2,
		east = 4,
		west = 8
	};

	public enum floor_enum {
		sw = 1,
		s = 2,
		se = 4,
		w = 8,
		c = 16,
		e = 32,
		nw = 64,
		n = 128,
		ne = 256
	};

	public static MazeGenerator.floor_enum[] floor_enums = (MazeGenerator.floor_enum[]) Enum.GetValues(typeof(MazeGenerator.floor_enum));

	static Dictionary<direction_enum, int> dx = new Dictionary<direction_enum, int> {
		{direction_enum.east, 1},
		{direction_enum.west, -1},
		{direction_enum.north, 0},
		{direction_enum.south, 0}
	};

	static Dictionary<direction_enum, int> dy = new Dictionary<direction_enum, int> {
		{direction_enum.east, 0},
		{direction_enum.west, 0},
		{direction_enum.north, -1},
		{direction_enum.south, 1}
	};

	static Dictionary<direction_enum, int> opposite = new Dictionary<direction_enum, int> {
		{direction_enum.east, (int) direction_enum.west},
		{direction_enum.west, (int) direction_enum.east},
		{direction_enum.north, (int) direction_enum.south},
		{direction_enum.south, (int) direction_enum.north}
	};

	public static int grid_width;
	public static int grid_height;
	
	public class GridNode {
		public int dir;
		public bool is_cleared;

		public int first_floor;
		public int second_floor;
		public int roof;
		public int stair;

		public GridNode(int d = 0, bool c = false) {
			dir = d;
			is_cleared = c;
			first_floor = 0;
			second_floor = 0;
			roof = 0;
			stair = 0;
		}

		public void set_dir(int d) {
			dir = d;
		}

		public void add_dir(int d) {
			dir |= d;
		}

		public void set_cleared(bool c) {
			is_cleared = c;
		}

		public void add_floor(int i, int f) {
			if (i == 0)
				first_floor |= f;
			else if (i == 1)
				second_floor |= f;
			else if (i == 2)
				roof |= f;
			else
				stair |= f;
		}
	};

	public static List<List<GridNode>> grid = new List<List<GridNode>>();


	const float first_floor_ratio = 0.625f;
	const float second_floor_ratio = 0.75f;
	const float roof_ratio = 0.375f;
	const float stair_ratio = 0.375f;


	void Start() {
		
	}

	void Update() {
		
	}

	static public void generate_grid(int width = 3, int height = 3) {
		grid_width = width;
		grid_height = height;
		grid.Clear();
		for (int y = 0; y < height; y++) {
			List<GridNode> temp_list = new List<GridNode>();
			for (int x = 0; x < width; x++) {
				GridNode temp_node = new GridNode();
				temp_list.Add(temp_node);
			}
			grid.Add(temp_list);
		}

		int rx = UnityEngine.Random.Range(0, width);
		int ry = UnityEngine.Random.Range(0, height);

		carve(rx, ry);

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				randomize_node(x, y);
			}
		}
	}

	static public List<T> shuffle<T>(List<T> list) {
		for (int i = 0; i < list.Count; i++) {
			int r = UnityEngine.Random.Range(i, list.Count);

			T temp = list[i];
			list[i] = list[r];
			list[r] = temp;
		}
		return list;
	}

	static public void carve(int cx, int cy) {
		List<direction_enum> dirs = new List<direction_enum>((direction_enum[]) Enum.GetValues(typeof(direction_enum)));
		dirs = shuffle<direction_enum>(dirs);
		
		foreach (direction_enum dir in dirs) {
			int nx = cx + dx[dir];
			int ny = cy + dy[dir];
			if (
				(ny >= 0 && ny < grid_height) &&
				(nx >= 0 && nx < grid_width) &&
				(grid[ny][nx].dir == 0)
			) {
				grid[cy][cx].add_dir((int) dir);
				if (grid[cy][cx].dir == 0) GameManager.instance.quit();
				grid[ny][nx].add_dir(opposite[dir]);
				carve(nx, ny);
			}
		}
	}

	static public void randomize_node(int x, int y) {
		GridNode node = grid[y][x];

		if (is_exist_part(node.dir, (int) direction_enum.north)) node.add_floor(0, (int) floor_enum.n);
		if (is_exist_part(node.dir, (int) direction_enum.south)) node.add_floor(0, (int) floor_enum.s);
		if (is_exist_part(node.dir, (int) direction_enum.west)) node.add_floor(0, (int) floor_enum.w);
		if (is_exist_part(node.dir, (int) direction_enum.east)) node.add_floor(0, (int) floor_enum.e);

		bool is_able_generate_center_of_second = false;
		float r;
		foreach (floor_enum f in Enum.GetValues(typeof(floor_enum))) {
			r = UnityEngine.Random.Range(0.0f, 1.0f);
			if (r <= first_floor_ratio) {
				node.add_floor(0, (int) f);
			}

			if (is_exist_part(node.first_floor, (int) f)) {
				r = UnityEngine.Random.Range(0.0f, 1.0f);
				int rotated = rotate_first_to_second(f);
				if (r <= second_floor_ratio && rotated >= 0) {
					node.add_floor(1, rotated);
					is_able_generate_center_of_second = true;
				}
			}

			r = UnityEngine.Random.Range(0.0f, 1.0f);
			if (r <= roof_ratio) node.add_floor(2, (int) f);
		}

		if (is_able_generate_center_of_second) {
			r = UnityEngine.Random.Range(0.0f, 1.0f);
			if (r <= second_floor_ratio) {
				node.add_floor(1, (int) floor_enum.c);
			}
		}

		List<floor_enum> stair_floor = new List<floor_enum> {
			floor_enum.s,
			floor_enum.w,
			floor_enum.e,
			floor_enum.n
		};
		stair_floor = shuffle<floor_enum>(stair_floor);
		bool is_able_generate_no_more_stairs = false;

		foreach (floor_enum f in stair_floor) {
			if (is_exist_part(node.second_floor, (int) f)) {
				if (is_able_generate_no_more_stairs) {
					r = UnityEngine.Random.Range(0.0f, 1.0f);
					if (r <= stair_ratio) {
						node.add_floor(3, (int) f);
					}
					else if (r <= second_floor_ratio) {
						node.add_floor(1, rotate_stair_to_second(f));
					}
				}
				else {
					is_able_generate_no_more_stairs = true;
					node.add_floor(3, (int) f);
				}
			}
		}
		grid[y][x] = node;
	}

	static int rotate_first_to_second(floor_enum f) {
		floor_enum result;
		switch (f) {
			case floor_enum.n: result = floor_enum.w; break;
			case floor_enum.s: result = floor_enum.e; break;
			case floor_enum.w: result = floor_enum.s; break;
			case floor_enum.e: result = floor_enum.n; break;
			default: return -1;
		}

		return (int) result;
	}

	static int rotate_stair_to_second(floor_enum f) {
		floor_enum result;
		switch (f) {
			case floor_enum.n: result = floor_enum.ne; break;
			case floor_enum.s: result = floor_enum.sw; break;
			case floor_enum.w: result = floor_enum.nw; break;
			case floor_enum.e: result = floor_enum.se; break;
			default: return -1;
		}
		return (int) result;
	}

	public static bool is_exist_part(int p, int f) {
		return (p & (int) f) != 0;
	}
}
