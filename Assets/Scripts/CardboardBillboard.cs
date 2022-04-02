using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CardboardBillboard : MonoBehaviour
{
	public Texture2D front;
	public Texture2D back;
	public Texture2D side;

	private Mesh mesh;
	private float scale = 128.0f;
	private float tightness = 0.5f;
	private float thickness = 0.05f;
	private float deformation = 0.5f;
	private float threshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
		CreateMesh();
		GetComponent<MeshFilter>().mesh = mesh;
		if (front.width != back.width || front.height != back.height) {
			throw new Exception ("CardboardBillboard: has different front and back sizes");
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void CreateMesh()
	{
		// Randomly sample on a grid to get occupied pixels.
		float length_x = front.width / scale;
		float length_y = front.height / scale;
		int num_points_x = (int) Mathf.Ceil(length_x / tightness) + 1;
		int num_points_y = (int) Mathf.Ceil(length_y / tightness) + 1;
		Debug.Log("dimensions: " + num_points_x.ToString() + ", " + num_points_y.ToString());
		List<Vector2> points = new List<Vector2>(new Vector2[num_points_x * num_points_y]);
		// Three allowed values: 0: not occupied, 1: occupied, 2: border.
		List<int> occupied = new List<int>(new int[num_points_x * num_points_y]);
		for (int idx_y = 0; idx_y < num_points_y; ++idx_y) {
			for (int idx_x = 0; idx_x < num_points_x; ++idx_x) {
				int idx = idx_x + idx_y * num_points_x;
				float delta_x = length_x / (num_points_x - 1);
				float delta_y = length_y / (num_points_y - 1);
				float x = (idx_x + 0.5f * UnityEngine.Random.Range(-deformation, deformation)) * delta_x;
				float y = (idx_y + 0.5f * UnityEngine.Random.Range(-deformation, deformation)) * delta_y;
				bool occupied_front = false;
				bool occupied_back = false;
				if (idx_x > 0 && idx_x < num_points_x - 1 && idx_y > 0 && idx_y < num_points_y - 1) {
					int pix_x = (int) Mathf.Floor(x * scale);
					int pix_y = (int) Mathf.Floor(y * scale);
					occupied_front = (front.GetPixel(pix_x, pix_y).a > threshold);
					occupied_back = (back.GetPixel(pix_x, pix_y).a > threshold);
				}
				points[idx] = new Vector2(x, y);
				occupied[idx] = (occupied_front || occupied_back) ? 1 : 0;
				// Fill in border points.
				int left_idx = (idx_x - 1) + idx_y * num_points_x;
				int down_idx = idx_x + (idx_y - 1) * num_points_x;
				if (occupied[idx] == 1) {
					if (occupied[left_idx] == 0) {
						occupied[left_idx] = 2;
					}
					if (occupied[down_idx] == 0) {
						occupied[down_idx] = 2;
					}
				} else if (occupied[idx] == 0) {
					if (idx_x > 0 && occupied[left_idx] == 1) {
						occupied[idx] = 2;
					}
					if (idx_y > 0 && occupied[down_idx] == 1) {
						occupied[idx] = 2;
					}
				}
			}
		}
		Debug.Log("occupied: " + String.Join(", ", occupied.ToArray()));

		// Find a starting point for the border, where a point is occupied but its neighbours aren't.
		int border_start_idx = -1;
		for (int idx = 0; idx < occupied.Count; ++idx) {
			if (occupied[idx] == 2) {
				border_start_idx = idx;
				break;
			}
		}
		if (border_start_idx == -1) {
			throw new Exception("CardboardBillboard: couldn't find border start");
		}

		// Follow the border around, to make a loop.
		List<int> border_idx = new List<int>();
		border_idx.Add(border_start_idx);
		do {
			int last_last_idx = border_idx.Count > 1 ? border_idx[border_idx.Count - 2] : -1;
			int last_idx = border_idx[border_idx.Count - 1];
			int last_y = last_idx / num_points_x;
			int last_x = last_idx % num_points_x;
			for (int diff_y = -1; diff_y <= 1; ++diff_y) {
				for (int diff_x = -1; diff_x <= 1; ++diff_x) {
					int test_x = last_x + diff_x;
					int test_y = last_y + diff_y;
					if (test_x < 0 || test_x >= num_points_x || test_y < 0 || test_y >= num_points_y) {
						continue;
					}
					int neighbour_idx = test_x + test_y * num_points_x;
					if (neighbour_idx != last_idx && neighbour_idx != last_last_idx && occupied[neighbour_idx] == 2) {
						border_idx.Add(neighbour_idx);
						goto FoundNeighbour;
					}
				}
			}
			FoundNeighbour:
			;
		} while (border_idx[0] != border_idx[border_idx.Count - 1]);
		List<Vector2> border = new List<Vector2>(border_idx.Count - 1);
		for (int i = 0; i < border_idx.Count - 1; ++i) {
			border.Add(points[border_idx[i]]);
		}
		Debug.Log("border: " + String.Join(", ", border.ToArray()));
		Triangulator triangulator = new Triangulator(border.ToArray());
		int[] border_triangles = triangulator.Triangulate();

		// Make triangles for the two faces, then make triangles for the corrugated edge.
		List<Vector3> points3d = new List<Vector3>(border.Count * 2);
		List<int> triangles3d = new List<int>(border_triangles.Length * 2 + border.Count * 6);
		for (int i = 0; i < border.Count; ++i) {
			points3d.Add(new Vector3(border[i].x, border[i].y, 0.5f * thickness));
		}
		for (int i = 0; i < border.Count; ++i) {
			points3d.Add(new Vector3(border[i].x, border[i].y, -0.5f * thickness));
		}
		for (int i = 0; i < border_triangles.Length; ++i) {
			triangles3d.Add(border_triangles[i]);
		}
		for (int i = 0; i < border_triangles.Length; ++i) {
			triangles3d.Add(border.Count + border_triangles[i]);
		}
		for (int i = 0; i < border.Count; ++i) {
			triangles3d.Add(i);
			triangles3d.Add((i + 1) % border.Count);
			triangles3d.Add(i + border.Count);
			triangles3d.Add((i + 1) % border.Count + border.Count);
			triangles3d.Add(i + border.Count);
			triangles3d.Add((i + 1) % border.Count);
		}
		Debug.Log("vertices: " + String.Join(", ", points3d.ToArray()));
		Debug.Log("triangles: " + String.Join(", ", triangles3d.ToArray()));
		mesh = new Mesh();
		mesh.vertices = points3d.ToArray();
		mesh.triangles = triangles3d.ToArray();
	}
}
