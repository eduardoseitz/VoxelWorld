using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenTerrain : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
	
		Vector3[] vertices = new Vector3[mesh.vertices.Length];
		
		for (var i=0;i<vertices.Length;i++)
		{
			vertices[i] = new Vector3(mesh.vertices[i].x, 
										Mathf.PerlinNoise(mesh.vertices[i].x,mesh.vertices[i].z)*3,
										mesh.vertices[i].z);
			
		}


		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
