using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Vector3 chunkSize = new Vector3(16, 8, 16);
    [SerializeField] private Material atlasMaterial;
    [SerializeField] private int currentBlockType;
    [SerializeField] private BlockType[] blocks;

    // Mesh data
    private Vector3[] _vertices;
    private Vector3[] _normals;
    private Vector2[] _uvs;
    private int[] _triangles;

    // Vertice points vectors
    private Vector3 _vertice0;
    private Vector3 _vertice1;
    private Vector3 _vertice2;
    private Vector3 _vertice3;
    private Vector3 _vertice4;
    private Vector3 _vertice5;
    private Vector3 _vertice6;
    private Vector3 _vertice7;

    private int[,,] _chunckBlockData; // Store what kind of block is in that position, being -1 an empty space
    private Vector3 _newQuadPos;
    private GameObject _quadObject;
    private Mesh _quadMesh;
    private MeshFilter _quadMeshFilter;

    private GameObject _chunckObject;
    private MeshFilter _chunckMeshFilter;
    private MeshRenderer _chunckMeshRenderer;
    private List<CombineInstance> _combineInstanceList;

    // Start is called before the first frame update
    private void Start()
    {
        // Generate world
        StartCoroutine(GenerateChunck());
    }

    private void SetupQuadMeshData()
    {
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

        // Instantiate mesh list
        _combineInstanceList = new List<CombineInstance>();

        // Create new cube object
        _chunckObject = new GameObject("Chunck");
        _chunckObject.transform.parent = transform;
    }

    private IEnumerator GenerateChunck()
    {
        // Create a quad which will serve as a template mesh
        SetupQuadMeshData();

        // Generate block dataset
        _chunckBlockData = new int[(int)(chunkSize.x), (int)(chunkSize.y), (int)(chunkSize.z)];
        for (int z = 0; z < chunkSize.z; z++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int x = 0; x < chunkSize.x; x++)
                {
                    // Make some holes for debug only
                    if (UnityEngine.Random.Range(0,100) < 20)
                        _chunckBlockData[x, y, z] = -1;
                    else
                        _chunckBlockData[x, y, z] = currentBlockType;
                }
            }
        }

        // Generate blocks meshes
        for (int z = 0; z < chunkSize.z; z++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int x = 0; x < chunkSize.x; x++)
                {
                    // If block is not empty
                    if (_chunckBlockData[x, y, z] > -1)
                    {
                        // Generate a cube at the position
                        _newQuadPos = new Vector3(x, y, z);
                        GenerateCube(x, y, z);
                    }
                }
            }
        }

        // Combine meshes
        CombineQuadsIntoSingleMesh();

        // Destroy template quad
        Destroy(_quadObject);

        yield return null;
    }
    
    private void GenerateCube(int x, int y, int z)
    {
        // Generate the top of the cube
        if (y + 1 == _chunckBlockData.GetLength(1) || _chunckBlockData[x, y + 1, z] == -1)
        {
            CreateQuad(CubeSide.Top, (int)_chunckBlockData[x, y, z]);
        }
        // Generate the bottom of the cube
        if (y - 1 < 0 || _chunckBlockData[x, y - 1, z] == -1)
        {
            CreateQuad(CubeSide.Bottom, (int)_chunckBlockData[x, y, z]);
        }
        // Generate the front of the cube
        if (z + 1 == _chunckBlockData.GetLength(2) || _chunckBlockData[x, y, z + 1] == -1)
        {
            CreateQuad(CubeSide.Front, (int)_chunckBlockData[x, y, z]);
        }
        // Generate the back of the cube
        if (z - 1 < 0 || _chunckBlockData[x, y, z - 1] == -1)
        {
            CreateQuad(CubeSide.Back, (int)_chunckBlockData[x, y, z]);
        }
        // Generate the right side of the cube
        if (x + 1 == _chunckBlockData.GetLength(0) || _chunckBlockData[x + 1, y, z] == -1)
        {
            CreateQuad(CubeSide.Right, (int)_chunckBlockData[x, y, z]);
        }
        // Generate the left side of the cube
        if (x - 1 < 0 || _chunckBlockData[x - 1, y, z] == -1)
        {
            CreateQuad(CubeSide.Left, (int)_chunckBlockData[x, y, z]);
        }
    }

    private void CreateQuad(CubeSide cubeSide, int blockType)
    {
        // Create new mesh
        _quadMesh = new Mesh();

        // Construct data to be passed to mesh
        switch (cubeSide)
        {
            case CubeSide.Bottom:
                _vertices = new Vector3[] { _vertice0, _vertice1, _vertice2, _vertice3 };
                _normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                _uvs = blocks[blockType].bottomUVs;
                break;
            case CubeSide.Top:
                _vertices = new Vector3[] { _vertice7, _vertice6, _vertice5, _vertice4 };
                _normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                _uvs = blocks[blockType].topUVs;
                break;
            case CubeSide.Left:
                _vertices = new Vector3[] { _vertice7, _vertice4, _vertice0, _vertice3 };
                _normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                _uvs = blocks[blockType].sideUVs; // right texture
                break;
            case CubeSide.Right:
                _vertices = new Vector3[] { _vertice5, _vertice6, _vertice2, _vertice1 };
                _normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                _uvs = blocks[blockType].sideUVs; // left texture
                break;
            case CubeSide.Front:
                _vertices = new Vector3[] { _vertice4, _vertice5, _vertice1, _vertice0 };
                _normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                _uvs = blocks[blockType].sideUVs; // back texture
                break;
            case CubeSide.Back:
                _vertices = new Vector3[] { _vertice6, _vertice7, _vertice3, _vertice2 };
                _normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                _uvs = blocks[blockType].sideUVs; // front texture
                break;
        }
        _triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        // Put data into the mesh itself
        _quadMesh.vertices = _vertices;
        _quadMesh.normals = _normals;
        _quadMesh.uv = _uvs;
        _quadMesh.triangles = _triangles;

        // Recalculate bounding box for rendering so that it is ocluded correctly
        _quadMesh.RecalculateBounds();

        // Update position
        _quadObject.transform.parent = _chunckObject.transform;
        _quadObject.transform.SetPositionAndRotation(_newQuadPos, Quaternion.identity);

        // Add new created mesh to the stack
        _quadMeshFilter.mesh = _quadMesh;

        // Combine all quads into single instance
        CombineInstance _newCombineInstance = new CombineInstance();
        _newCombineInstance.mesh = _quadMeshFilter.sharedMesh;
        _newCombineInstance.transform = _quadMeshFilter.transform.localToWorldMatrix;
        _combineInstanceList.Add(_newCombineInstance);
    }

    private void CombineQuadsIntoSingleMesh()
    {
        // Add combined mesh to the cube
        _chunckMeshFilter = _chunckObject.AddComponent<MeshFilter>();
        _chunckMeshFilter.mesh = new Mesh();
        _chunckMeshFilter.mesh.CombineMeshes(_combineInstanceList.ToArray());

        // Add renderer to the cube
        _chunckMeshRenderer = _chunckObject.AddComponent<MeshRenderer>();
        _chunckMeshRenderer.material = atlasMaterial;
    }
}

public enum CubeSide { Top, Bottom, Front, Back, Right, Left }
