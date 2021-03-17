using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] Material quadMaterial;
    private Mesh _quadMesh;

    // Mesh data
    private Vector3[] _vertices;
    private Vector3[] _normals;
    private Vector2[] _uvs;
    private int[] _triangles;

    // UV vectors
    private Vector2 _uv00;
    private Vector2 _uv01;
    private Vector2 _uv10;
    private Vector2 _uv11;

    // Vertice points vectors
    private Vector3 _vertice0;
    private Vector3 _vertice1;
    private Vector3 _vertice2;
    private Vector3 _vertice3;
    private Vector3 _vertice4;
    private Vector3 _vertice5;
    private Vector3 _vertice6;
    private Vector3 _vertice7;

    private GameObject _quadObject;

    // Start is called before the first frame update
    void Start()
    {
        CreateQuad();    
    }

    private void CreateQuad()
    {
        // Create new mesh
        _quadMesh = new Mesh();
        _quadMesh.name = "Cube Mesh";

        //all possible UVs
        _uv00 = new Vector2(0f, 0f);
        _uv10 = new Vector2(1f, 0f);
        _uv01 = new Vector2(0f, 1f);
        _uv11 = new Vector2(1f, 1f);

        //all possible vertices
        _vertice0 = new Vector3(-0.5f, -0.5f, 0.5f);
        _vertice1 = new Vector3(0.5f, -0.5f, 0.5f);
        _vertice2 = new Vector3(0.5f, -0.5f, -0.5f);
        _vertice3 = new Vector3(-0.5f, -0.5f, -0.5f);
        _vertice4 = new Vector3(-0.5f, 0.5f, 0.5f);
        _vertice5 = new Vector3(0.5f, 0.5f, 0.5f);
        _vertice6 = new Vector3(0.5f, 0.5f, -0.5f);
        _vertice7 = new Vector3(-0.5f, 0.5f, -0.5f);

        // Construct data to be passed to mesh
        _vertices = new Vector3[] { _vertice4, _vertice5, _vertice1, _vertice0 };
        _normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        _uvs = new Vector2[] { _uv11, _uv01, _uv00, _uv10 };
        _triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        // Put data into the mesh itself
        _quadMesh.vertices = _vertices;
        _quadMesh.normals = _normals;
        _quadMesh.uv = _uvs;
        _quadMesh.triangles = _triangles;

        // Recalculate bounding box for rendering so that it is ocluded correctly
        _quadMesh.RecalculateBounds();

        _quadObject = new GameObject("Quad");
        _quadObject.transform.parent = transform;
        _quadObject.AddComponent<MeshFilter>().mesh = _quadMesh;
        _quadObject.AddComponent<MeshRenderer>().material = quadMaterial;
    }
}
