using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    public enum CubeSide { Top, Bottom, Front, Back, Right, Left }

    [SerializeField] private Material dirtMaterial;

    
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

    private int _currentQuad;
    private GameObject _quadObject;
    private Mesh _quadMesh;
    private MeshFilter _quadMeshFilter;
    private MeshRenderer _quadMeshRenderer;
    private CombineInstance[] _combineInstance;

    private GameObject _cubeObject;
    private MeshFilter _cubeMeshFilter;
    private MeshRenderer _cubeMeshRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        SetupMeshData();

        CreateCube();

        Destroy(_quadObject);
    }

    private void SetupMeshData()
    {
        // Create data required by mesh filter
        _vertices = new Vector3[4];
        _normals = new Vector3[4];
        _uvs = new Vector2[4];
        _triangles = new int[6];

        // All possible UVs
        _uv00 = new Vector2(0f, 0f);
        _uv10 = new Vector2(1f, 0f);
        _uv01 = new Vector2(0f, 1f);
        _uv11 = new Vector2(1f, 1f);

        // All possible vertices
        _vertice0 = new Vector3(-0.5f, -0.5f, 0.5f);
        _vertice1 = new Vector3(0.5f, -0.5f, 0.5f);
        _vertice2 = new Vector3(0.5f, -0.5f, -0.5f);
        _vertice3 = new Vector3(-0.5f, -0.5f, -0.5f);
        _vertice4 = new Vector3(-0.5f, 0.5f, 0.5f);
        _vertice5 = new Vector3(0.5f, 0.5f, 0.5f);
        _vertice6 = new Vector3(0.5f, 0.5f, -0.5f);
        _vertice7 = new Vector3(-0.5f, 0.5f, -0.5f);

        // Create temporary object
        _quadObject = new GameObject("Quad");
        _quadObject.transform.parent = transform;
        _quadMeshFilter = _quadObject.AddComponent<MeshFilter>();
        _quadMeshRenderer = _quadObject.AddComponent<MeshRenderer>();

    }

    private void CreateCube()
    {
        // Create new cube object
        _cubeObject = new GameObject("Cube");
        _cubeObject.transform.parent = transform;

        // Generate each side of the cube
        _combineInstance = new CombineInstance[6];
        _currentQuad = 0;
        
        CreateQuad(CubeSide.Front);
        CreateQuad(CubeSide.Back);
        CreateQuad(CubeSide.Right);
        CreateQuad(CubeSide.Left);
        CreateQuad(CubeSide.Top);
        CreateQuad(CubeSide.Bottom);
        CombineQuadsIntoCube();
    }

    private void CreateQuad(CubeSide cubeSide)
    {
        // Create new mesh
        _quadMesh = new Mesh();

        // Construct data to be passed to mesh
        switch (cubeSide)
        {
            case CubeSide.Bottom:
                _vertices = new Vector3[] { _vertice0, _vertice1, _vertice2, _vertice3 };
                _normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                break;
            case CubeSide.Top:
                _vertices = new Vector3[] { _vertice7, _vertice6, _vertice5, _vertice4 };
                _normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                break;
            case CubeSide.Left:
                _vertices = new Vector3[] { _vertice7, _vertice4, _vertice0, _vertice3 };
                _normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                break;
            case CubeSide.Right:
                _vertices = new Vector3[] { _vertice5, _vertice6, _vertice2, _vertice1 };
                _normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                break;
            case CubeSide.Front:
                _vertices = new Vector3[] { _vertice4, _vertice5, _vertice1, _vertice0 };
                _normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                break;
            case CubeSide.Back:
                _vertices = new Vector3[] { _vertice6, _vertice7, _vertice3, _vertice2 };
                _normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                break;
        }
        _uvs = new Vector2[] { _uv11, _uv01, _uv00, _uv10 };
        _triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        // Put data into the mesh itself
        _quadMesh.vertices = _vertices;
        _quadMesh.normals = _normals;
        _quadMesh.uv = _uvs;
        _quadMesh.triangles = _triangles;

        // Recalculate bounding box for rendering so that it is ocluded correctly
        _quadMesh.RecalculateBounds();

        // Add new created mesh to the stack
        _quadMeshFilter.mesh = _quadMesh;
        _quadMeshRenderer.material = dirtMaterial;

        // Combine all quads into single instance
        _combineInstance[_currentQuad].mesh = _quadMeshFilter.sharedMesh;
        _combineInstance[_currentQuad].transform = _quadMeshFilter.transform.localToWorldMatrix;
        _currentQuad++;
    }

    private void CombineQuadsIntoCube()
    {
        // Add combined mesh to the cube
        _cubeMeshFilter = _cubeObject.AddComponent<MeshFilter>();
        _cubeMeshFilter.mesh = new Mesh();
        _cubeMeshFilter.mesh.CombineMeshes(_combineInstance);

        // Add renderer to the cube
        _cubeMeshRenderer = _cubeObject.AddComponent<MeshRenderer>();
        _cubeMeshRenderer.material = dirtMaterial;
    }
}
