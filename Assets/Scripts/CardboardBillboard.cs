using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using mattatz.Triangulation2DSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CardboardBillboard : MonoBehaviour
{
	public Texture2D mask;
	public Texture2D front;
	public Texture2D back;
	public Texture2D side;
	public GameObject stick;

	public Material cardboard;
	public Material paint;

	private Mesh mesh;
	private Texture2D texture;
	public float scale = 600.0f;
	public float density = 6.0f;
	public float margin = 0.1f;
	public float push_factor = 1.0f;
	public float smoothing = 0.10f;
	private int smoothing_iterations = 2;
	private float thickness = 0.10f;
	private float deformation = 0.0f;
	private float alpha_threshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
		CreateMesh();
		/*
		mesh = new Mesh();
		mesh.vertices = new Vector3[] {
			new Vector3(0.0f, 0.0f, 0.0f),
			new Vector3(5.0f, 0.0f, 0.0f),
			new Vector3(5.0f, 5.0f, 0.0f),
			new Vector3(0.0f, 5.0f, 0.0f)
		};
		mesh.triangles = new int[] {
			0, 1, 2,
			0, 2, 3
		};
		mesh.SetUVs(0, new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(1.0f, 1.0f),
			new Vector2(0.0f, 1.0f)
		});
		*/
		GetComponent<MeshFilter>().mesh = mesh;
		Material paint_mod = new Material(paint);
		paint_mod.mainTexture = texture;
		GetComponent<MeshRenderer>().materials = new Material[] {
			cardboard,
			paint_mod
		};
		if (stick != null) {
			GameObject obj = Instantiate(stick);
			obj.transform.SetParent(GetComponent<Transform>(), false);
		}
    }

	void CreateMesh()
	{
		if (front.width != back.width || front.height != back.height) {
			throw new Exception ("CardboardBillboard: has different front and back sizes");
		}
		if (mask.width != front.width || mask.height != front.height) {
			throw new Exception ("CardboardBillboard: has different mask size " + mask.width.ToString() + ", " + mask.height.ToString() + ", " + front.width.ToString() + ", " + front.height.ToString());
		}
		// Randomly sample on a grid to get occupied pixels.
		float length_x = front.width / scale;
		float length_y = front.height / scale;
		int num_points_x = (int) Mathf.Ceil(length_x * density) + 1;
		int num_points_y = (int) Mathf.Ceil(length_y * density) + 1;
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
					occupied_front = (mask.GetPixel(pix_x, pix_y).a > alpha_threshold);
					occupied_back = (mask.GetPixel(pix_x, pix_y).a > alpha_threshold);
				}
				points[idx] = new Vector2(x, y);
				occupied[idx] = (occupied_front || occupied_back) ? 1 : 0;
				// Fill in border points.
				int left_idx = (idx_x - 1) + idx_y * num_points_x;
				int down_idx = idx_x + (idx_y - 1) * num_points_x;
				//int down_left_idx = (idx_x - 1) + (idx_y - 1) * num_points_x;
				//int down_right_idx = (idx_x + 1) + (idx_y - 1) * num_points_x;
				if (occupied[idx] == 1) {
					if (occupied[left_idx] == 0) {
						occupied[left_idx] = 2;
					}
					if (occupied[down_idx] == 0) {
						occupied[down_idx] = 2;
					}
					/*
					if (occupied[down_left_idx] == 0) {
						occupied[down_left_idx] = 2;
					}
					if (occupied[down_right_idx] == 0) {
						occupied[down_right_idx] = 2;
					}
					*/
				} else if (occupied[idx] == 0) {
					if (idx_x > 0 && occupied[left_idx] == 1) {
						occupied[idx] = 2;
					}
					if (idx_y > 0 && occupied[down_idx] == 1) {
						occupied[idx] = 2;
					}
					/*
					if (idx_x > 0 && idx_y > 0 && occupied[down_left_idx] == 1) {
						occupied[idx] = 2;
					}
					if (idx_x < num_points_x - 1 && idx_y > 0 && occupied[down_right_idx] == 1) {
						occupied[idx] = 2;
					}
					*/
				}
			}
		}

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
		float total_rotation = 0.0f;
		float first_rotation = 0.0f;
		float last_rotation = 0.0f / 0.0f;
		do {
			int last_last_idx = border_idx.Count > 1 ? border_idx[border_idx.Count - 2] : -1;
			int last_idx = border_idx[border_idx.Count - 1];
			occupied[last_idx] = 3;
			int last_y = last_idx / num_points_x;
			int last_x = last_idx % num_points_x;
			bool found_neighbour = false;
			Tuple<int, int>[] diffs = new Tuple<int, int>[] {
				new Tuple<int, int>(-1, 0),
				new Tuple<int, int>(+1, 0),
				new Tuple<int, int>( 0,-1),
				new Tuple<int, int>( 0,+1),
				new Tuple<int, int>(-1,-1),
				new Tuple<int, int>(+1,+1),
				new Tuple<int, int>(+1,-1),
				new Tuple<int, int>(-1,+1)
			};
			foreach (Tuple<int, int> diff in diffs) {
				int diff_x = diff.Item1;
				int diff_y = diff.Item2;
				int test_x = last_x + diff_x;
				int test_y = last_y + diff_y;
				if (test_x < 0 || test_x >= num_points_x || test_y < 0 || test_y >= num_points_y) {
					continue;
				}
				int neighbour_idx = test_x + test_y * num_points_x;
				if (occupied[neighbour_idx] == 2) {
					border_idx.Add(neighbour_idx);
					float rotation = Mathf.Atan2(diff_y, diff_x);
					if (!float.IsNaN(last_rotation)) {
						total_rotation += Mathf.Repeat(rotation - last_rotation + Mathf.PI, 2.0f * Mathf.PI) - Mathf.PI;
					} else {
						first_rotation = rotation;
					}
					last_rotation = rotation;
					found_neighbour = true;
					goto FoundNeighbour;
				}
			}
			FoundNeighbour:
			if (!found_neighbour) {
				int a_idx = border_idx[border_idx.Count - 1];
				int a_y = a_idx / num_points_x;
				int a_x = a_idx % num_points_x;
				int b_idx = border_idx[0];
				int b_y = b_idx / num_points_x;
				int b_x = b_idx % num_points_x;
				if (Mathf.Abs(a_x - b_x) <= 1 && Mathf.Abs(a_y - b_y) <= 1) {
					float rotation = Mathf.Atan2(b_y - a_y, a_x - b_x);
					if (!float.IsNaN(last_rotation)) {
						total_rotation += Mathf.Repeat(rotation - last_rotation + Mathf.PI, 2.0f * Mathf.PI) - Mathf.PI;
					} else {
						first_rotation = rotation;
					}
					last_rotation = rotation;
					break;
				}
				border_idx.RemoveAt(border_idx.Count - 1);
				if (border_idx.Count == 0) {
					throw new Exception("CardboardBillboard: ran out of idxs");
				}
				//throw new Exception("CardboardBillboard: couldn't find neighbour");
			}
		} while (true);
		total_rotation += Mathf.Repeat(first_rotation - last_rotation + Mathf.PI, 2.0f * Mathf.PI) - Mathf.PI;
		List<Vector2> border = new List<Vector2>(border_idx.Count);
		for (int i = 0; i < border_idx.Count; ++i) {
			border.Add(points[border_idx[i]]);
		}
		if (total_rotation < 0.0f) {
			border.Reverse();
		}

		List<Vector2> border_smoothed = new List<Vector2>();
		
		// Smooth the border.
		for (int j = 0; j < smoothing_iterations; ++j) {
			border_smoothed = new List<Vector2>(border.Count);
			for (int i = 0; i < border.Count; ++i) {
				Vector2 point = border[i];
				Vector2 point_next = border[(i + 1) % border.Count];
				Vector2 point_prev = border[i == 0 ? border.Count - 1 : i - 1];
				Vector2 point_avg = 0.5f * (point_next + point_prev);
				border_smoothed.Add(smoothing * point_avg + (1.0f - smoothing) * point);
			}
			border = border_smoothed;
		}

		// Push edges out.
		border_smoothed = new List<Vector2>(border.Count);
		for (int i = 0; i < border.Count; ++i) {
			Vector2 point = border[i];
			Vector2 point_next = border[(i + 1) % border.Count];
			Vector2 point_prev = border[i == 0 ? border.Count - 1 : i - 1];
			Vector2 norm = -Vector2.Perpendicular(point_next - point_prev);
			norm.Normalize();
			Vector2 point_push = point + push_factor * margin * length_y * norm;
			border_smoothed.Add(point_push);
		}
		border = border_smoothed;
		
		// Smooth the border.
		for (int j = 0; j < smoothing_iterations; ++j) {
			border_smoothed = new List<Vector2>(border.Count);
			for (int i = 0; i < border.Count; ++i) {
				Vector2 point = border[i];
				Vector2 point_next = border[(i + 1) % border.Count];
				Vector2 point_prev = border[i == 0 ? border.Count - 1 : i - 1];
				Vector2 point_avg = 0.5f * (point_next + point_prev);
				border_smoothed.Add(smoothing * point_avg + (1.0f - smoothing) * point);
			}
			border = border_smoothed;
		}

		// Scale down.
		Vector2 center_of_mass = new Vector2(0.0f, 0.0f);
		for (int i = 0; i < border.Count; ++i) {
			center_of_mass += border[i] / border.Count;
		}
		border_smoothed = new List<Vector2>(border.Count);
		for (int i = 0; i < border.Count; ++i) {
			border_smoothed.Add(center_of_mass + (border[i] - center_of_mass) / (1.0f + push_factor * margin));
		}
		border = border_smoothed;

		// Clamp edges.
		for (int i = 0; i < border.Count; ++i) {
			border[i] = new Vector2(
				Mathf.Clamp(border[i].x, -margin * length_x, (1.0f + margin) * length_x),
				Mathf.Clamp(border[i].y, -margin * length_y, (1.0f + margin) * length_y));
		}

		//Polygon2D polygon = Polygon2D.Contour(border.ToArray());
    	//Triangulation2D triangulator = new Triangulation2D(polygon, 22.5f);
		//Mesh interior_mesh = triangulator.Build();
		//List<Vector3> interior = new List<Vector3>(interior_mesh.vertices);
		//List<int> triangles = new List<int>(interior_mesh.triangles);
		Triangulator triangulator = new Triangulator(border.ToArray());
		List<Vector2> interior = border;
		List<int> triangles = new List<int>(triangulator.Triangulate());

		// Make triangles for the two faces, then make triangles for the corrugated edge.
		int padding_vert = (int) (front.height * margin);
		int padding_horz = (int) (front.width * margin);
		float v_bottom = (float) (front.height + 2 * padding_vert) / (front.height + 2 * padding_vert + side.height);
		List<Vector3> points3d = new List<Vector3>(interior.Count * 2 + border.Count * 2);
		List<int> triangles3d = new List<int>(triangles.Count * 2 + border.Count * 6);
		List<Vector2> texuv3d = new List<Vector2>(interior.Count * 6 + border.Count * 4);
		for (int i = 0; i < interior.Count; ++i) {
			points3d.Add(new Vector3(interior[i].x - 0.5f * length_x, interior[i].y, 0.5f * thickness));
			texuv3d.Add(new Vector2(
				Mathf.Lerp(0.0f, 0.5f, (interior[i].x + margin * length_x) / ((1.0f + 2 * margin) * length_x)),
				Mathf.Lerp(0.0f, v_bottom, (interior[i].y + margin * length_y) / ((1.0f + 2 * margin) * length_y))));
		}
		for (int i = 0; i < interior.Count; ++i) {
			points3d.Add(new Vector3(interior[i].x - 0.5f * length_x, interior[i].y, -0.5f * thickness));
			texuv3d.Add(new Vector2(
				Mathf.Lerp(0.5f, 1.0f, (interior[i].x + margin * length_x) / ((1.0f + 2 * margin) * length_x)),
				Mathf.Lerp(0.0f, v_bottom, (interior[i].y + margin * length_y) / ((1.0f + 2 * margin) * length_y))));
		}
		for (int i = 0; i < triangles.Count; ++i) {
			triangles3d.Add(triangles[i]);
		}
		for (int i = 0; i < triangles.Count; ++i) {
			triangles3d.Add(triangles[i] + interior.Count);
			if (i % 3 == 1) {
				int temp = triangles3d[i];
				triangles3d[i] = triangles3d[i - 1];
				triangles3d[i - 1] = temp;
			}
		}
		for (int i = 0; i < border.Count; ++i) {
			points3d.Add(new Vector3(border[i].x - 0.5f * length_x, border[i].y, 0.5f * thickness));
			points3d.Add(new Vector3(border[i].x - 0.5f * length_x, border[i].y, -0.5f * thickness));
			triangles3d.Add(2 * interior.Count + 2 * i);
			triangles3d.Add(2 * interior.Count + 2 * i + 1);
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 2) : 0));
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 2) : 0));
			triangles3d.Add(2 * interior.Count + 2 * i + 1);
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 3) : 1));
			float border_pos = 1.0f * (float) i / (border.Count - 1);
			texuv3d.Add(new Vector2(border_pos, v_bottom));
			texuv3d.Add(new Vector2(border_pos, v_bottom + 0.5f * (1.0f - v_bottom)));
		}

		// Assign the mesh.
		mesh = new Mesh();
		mesh.SetVertices(points3d.ToArray());
		mesh.SetTriangles(triangles3d.ToArray(), 0);
		mesh.SetUVs(0, texuv3d.ToArray());
		mesh.RecalculateNormals();

		// Make the texture.
		texture = new Texture2D(front.width + back.width + 4 * padding_horz, front.height + side.height + 2 * padding_vert, TextureFormat.RGBA32, true);
		Color[] pixels = texture.GetPixels();
		for (int i = 0; i < pixels.Length; ++i) {
			pixels[i] = Color.clear;
		}
		texture.SetPixels(pixels);
		if (front.format != texture.format) {
			throw new Exception("CardboardBillboard: front has different texture format");
		}
		if (back.format != texture.format) {
			throw new Exception("CardboardBillboard: back has different texture format");
		}
		if (side.format != texture.format) {
			throw new Exception("CardboardBillboard: side has different texture format");
		}
		Graphics.CopyTexture(front, 0, 0, 0, 0, front.width, front.height, texture, 0, 0, padding_horz, padding_vert);
		Graphics.CopyTexture(back, 0, 0, 0, 0, back.width, back.height, texture, 0, 0, front.width + 3 * padding_horz, padding_vert);
		int side_coord = 0;
		while (side_coord < front.width + back.width + 4 * padding_horz) {
			int pix_remaining = front.width + back.width + 4 * padding_horz - side_coord;
			int pix_available = side.width;
			Graphics.CopyTexture(side, 0, 0, 0, 0, Mathf.Min(pix_remaining, pix_available), side.height, texture, 0, 0, side_coord, front.height + 2 * padding_vert);
			side_coord += Mathf.Min(pix_remaining, pix_available);
		}
		texture.Apply(true, true);
	}
}

