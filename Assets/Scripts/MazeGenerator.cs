using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	enum node_type {
		none,
		n, s, w, e,
		ns, nw, ne, sw, se, we,
		nsw, nse, nwe, swe,
		nswe
	};

	struct node {
		public node_type type;
		public int count;
	}
	static Vector2Int max_size = new Vector2Int(5, 5);
	node[,] maze = new node[max_size.x, max_size.y];

	void Start() {
		
	}

	void Update() {
		
	}

	Vector2Int get_random_neighbor_node(Vector2Int p) {
		Vector2Int result = new Vector2Int();

		Vector2Int up = get_valid_node(new Vector2Int(p.x, p.y + 1));
		Vector2Int down = get_valid_node(new Vector2Int(p.x, p.y - 1));
		Vector2Int left = get_valid_node(new Vector2Int(p.x - 1, p.y));
		Vector2Int right = get_valid_node(new Vector2Int(p.x + 1, p.y));

		List<Vector2Int> temp_list = new List<Vector2Int>();

		// if (is_valid_node(up) && maze[p.x, p.y]) {
			
		// }

		Random.Range(0, 4);

		return result;
	}

	Vector2Int get_valid_node(Vector2Int p) {
		if (
			p.x >= 0 && p.x < max_size.x &&
			p.y >= 0 && p.y < max_size.y
		) {
			return p;
		}
		return new Vector2Int(-1, -1);
	}

	bool is_valid_node(Vector2Int p) {
		return (p.x != -1);
	}

	bool is_visited(Vector2Int p) {
		// maze'[p.x, p.y]
		return true;
	}
}
