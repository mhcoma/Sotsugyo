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

		public GridNode(int d = 0, bool c = false) {
			dir = d;
			is_cleared = c;	
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
	};

	public static List<List<GridNode>> grid = new List<List<GridNode>>();

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
}
