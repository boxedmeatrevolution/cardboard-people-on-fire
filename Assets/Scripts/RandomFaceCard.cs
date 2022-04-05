using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFaceCard : MonoBehaviour
{
	public CardboardBillboard cardboard;
	public List<Texture2D> faces;
    // Start is called before the first frame update
    void Awake()
    {
        int index = (int) Random.Range(0.0f, faces.Count - 0.000001f);
		Texture2D tex = faces[index];
		cardboard.front = tex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
