using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeProperties : MonoBehaviour {

	MeshFilter meshFilter;
	public Text vertexPrefab;
	string allVs = "";

	// Use this for initialization
	void Start () {
		meshFilter = this.GetComponent<MeshFilter>();
		
		GameObject canvas = GameObject.FindWithTag("MainCanvas");
		Debug.Log("Vertices------");
		int c = 0;
		foreach(Vector3 v in meshFilter.mesh.vertices)
		{
			Debug.Log(v);
			string vname = v + "";

			Text vertexLabel;
			if(!GameObject.Find(vname))
			{
				vertexLabel = Instantiate(vertexPrefab, Vector3.zero, Quaternion.identity ) as Text;
				vertexLabel.gameObject.name = vname;
        		vertexLabel.transform.SetParent(canvas.transform);
				vertexLabel.text = v + ": " + c + "";
				Vector3 vertexLabelPos = Camera.main.WorldToScreenPoint(v + this.transform.position + new Vector3(0,c/80.0f,0));
            	vertexLabel.transform.position = vertexLabelPos;
            	
			}
			else
			{
				vertexLabel = GameObject.Find(vname).GetComponent<Text>();
				vertexLabel.text += " " + c; 
			}
			
			c++;
		}
		

		Debug.Log("Normals-----");
		foreach(Vector3 n in meshFilter.mesh.normals)
		{
			Debug.Log(n);
			
		}

		
		Debug.Log("UVs------");
		foreach(Vector2 u in meshFilter.mesh.uv)
		{
			Debug.Log(u);
			allVs += u + " ";
		}
		

		Debug.Log("Triangles----");
		foreach(int i in meshFilter.mesh.triangles)
		{
			Debug.Log(i);
			allVs += i + " ";
		}
		Debug.Log(allVs);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
