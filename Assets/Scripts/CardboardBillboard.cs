using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using mattatz.Triangulation2DSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CardboardBillboard : MonoBehaviour
{
	public Texture2D front;
	public Texture2D back;
	public Texture2D side;

	public Material cardboard;
	public Material paint;

	private Mesh mesh;
	private Texture2D texture;
	private float scale = 128.0f;
	private float density = 4.0f;
	private float margin = 0.2f;
	private float push_factor = 1.0f;
	private float smoothing = 0.5f;
	private float thickness = 0.15f;
	private float deformation = 0.5f;
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
		int num_points_x = (int) Mathf.Ceil(length_x * density) + 1;
		int num_points_y = (int) Mathf.Ceil(length_y * density) + 1;
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
					occupied_front = (front.GetPixel(pix_x, pix_y).a > alpha_threshold);
					occupied_back = (back.GetPixel(pix_x, pix_y).a > alpha_threshold);
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
		float total_rotation = 0.0f;
		float first_rotation = 0.0f;
		float last_rotation = 0.0f / 0.0f;
		do {
			int last_last_idx = border_idx.Count > 1 ? border_idx[border_idx.Count - 2] : -1;
			int last_idx = border_idx[border_idx.Count - 1];
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
				new Tuple<int, int>(-1,+1),
				new Tuple<int, int>(+1,-1)
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
				if (neighbour_idx != last_idx && neighbour_idx != last_last_idx && occupied[neighbour_idx] == 2) {
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
				throw new Exception("CardboardBillboard: couldn't find neighbour");
			}
		} while (border_idx[0] != border_idx[border_idx.Count - 1]);
		total_rotation += Mathf.Repeat(first_rotation - last_rotation + Mathf.PI, 2.0f * Mathf.PI) - Mathf.PI;
		Debug.Log("total rotation: " + total_rotation.ToString());
		List<Vector2> border = new List<Vector2>(border_idx.Count - 1);
		for (int i = 0; i < border_idx.Count - 1; ++i) {
			border.Add(points[border_idx[i]]);
		}
		if (total_rotation < 0.0f) {
			Debug.Log("REVERSED!");
			border.Reverse();
		}

		List<Vector2> border_smoothed = new List<Vector2>();

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
		border_smoothed = new List<Vector2>(border.Count);
		for (int i = 0; i < border.Count; ++i) {
			Vector2 point = border[i];
			Vector2 point_next = border[(i + 1) % border.Count];
			Vector2 point_prev = border[i == 0 ? border.Count - 1 : i - 1];
			Vector2 point_avg = 0.5f * (point_next + point_prev);
			border_smoothed.Add(smoothing * point_avg + (1.0f - smoothing) * point);
		}
		border = border_smoothed;

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

		Debug.Log("border: " + String.Join(", ", border.ToArray()));
		//Polygon2D polygon = Polygon2D.Contour(border.ToArray());
    	//Triangulation2D triangulator = new Triangulation2D(polygon, 22.5f);
		//Mesh interior_mesh = triangulator.Build();
		//List<Vector3> interior = new List<Vector3>(interior_mesh.vertices);
		//List<int> triangles = new List<int>(interior_mesh.triangles);
		Triangulator triangulator = new Triangulator(border.ToArray());
		List<Vector2> interior = border;
		List<int> triangles = new List<int>(triangulator.Triangulate());
		Debug.Log("number of triangles: " + triangles.Count.ToString());

		// Make triangles for the two faces, then make triangles for the corrugated edge.
		int padding = (int) (front.height * margin);
		float v_bottom = (float) (front.height + 2 * padding) / (front.height + 2 * padding + side.height);
		List<Vector3> points3d = new List<Vector3>(interior.Count * 2 + border.Count * 2);
		List<int> triangles3d = new List<int>(triangles.Count * 2 + border.Count * 6);
		List<Vector2> texuv3d = new List<Vector2>(interior.Count * 6 + border.Count * 4);
		for (int i = 0; i < interior.Count; ++i) {
			points3d.Add(new Vector3(interior[i].x, interior[i].y, 0.5f * thickness));
			texuv3d.Add(new Vector2(
				Mathf.Lerp(0.0f, 0.5f, (interior[i].x + margin * length_x) / ((1.0f + 2 * margin) * length_x)),
				Mathf.Lerp(0.0f, v_bottom, (interior[i].y + margin * length_x) / ((1.0f + 2 * margin) * length_y))));
		}
		for (int i = 0; i < interior.Count; ++i) {
			points3d.Add(new Vector3(interior[i].x, interior[i].y, -0.5f * thickness));
			texuv3d.Add(new Vector2(
				Mathf.Lerp(0.5f, 1.0f, (interior[i].x + margin * length_x) / ((1.0f + 2 * margin) * length_x)),
				Mathf.Lerp(0.0f, v_bottom, (interior[i].y + margin * length_x) / ((1.0f + 2 * margin) * length_y))));
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
			points3d.Add(new Vector3(border[i].x, border[i].y, 0.5f * thickness));
			points3d.Add(new Vector3(border[i].x, border[i].y, -0.5f * thickness));
			triangles3d.Add(2 * interior.Count + 2 * i);
			triangles3d.Add(2 * interior.Count + 2 * i + 1);
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 2) : 0));
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 2) : 0));
			triangles3d.Add(2 * interior.Count + 2 * i + 1);
			triangles3d.Add(2 * interior.Count + ((i < border.Count - 1) ? (2 * i + 3) : 1));
			float border_pos = 2.0f * (float) i / (border.Count - 1);
			texuv3d.Add(new Vector2(border_pos, v_bottom));
			texuv3d.Add(new Vector2(border_pos, v_bottom + 0.25f * (1.0f - v_bottom)));
		}
		Debug.Log("texuvcs: " + String.Join(", ", texuv3d.ToArray()));
		Debug.Log("vertices: " + String.Join(", ", points3d.ToArray()));
		Debug.Log("triangles: " + String.Join(", ", triangles3d.ToArray()));

		// Assign the mesh.
		mesh = new Mesh();
		mesh.SetVertices(points3d.ToArray());
		mesh.SetTriangles(triangles3d.ToArray(), 0);
		mesh.SetUVs(0, texuv3d.ToArray());
		mesh.RecalculateNormals();

		// Make the texture.
		texture = new Texture2D(front.width + back.width + 4 * padding, front.height + side.height + 2 * padding, TextureFormat.RGBA32, true);
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
		Graphics.CopyTexture(front, 0, 0, 0, 0, front.width, front.height, texture, 0, 0, padding, padding);
		Graphics.CopyTexture(back, 0, 0, 0, 0, back.width, back.height, texture, 0, 0, front.width + 3 * padding, padding);
		int side_coord = 0;
		while (side_coord < front.width + back.width + 4 * padding) {
			int pix_remaining = front.width + back.width + 4 * padding - side_coord;
			int pix_available = side.width;
			Graphics.CopyTexture(side, 0, 0, 0, 0, Mathf.Min(pix_remaining, pix_available), side.height, texture, 0, 0, side_coord, front.height + 2 * padding);
			side_coord += Mathf.Min(pix_remaining, pix_available);
		}
		texture.Apply(true, true);
	}
}

