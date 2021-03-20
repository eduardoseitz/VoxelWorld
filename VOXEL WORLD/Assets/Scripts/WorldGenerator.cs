using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    #region Declarations
    [Header("World Setup")]
    [SerializeField] private Vector3 chunkSize = new Vector3(16, 16, 16);
    [Tooltip("World heigh in chuncks")]
    [SerializeField] private Vector3 worldSize = new Vector3(3, 3, 3);
    [SerializeField] private Material atlasMaterial;
    [SerializeField] private BlockType[] blocks;

    [Header("Debug")]
    [SerializeField] private int holesChance = 10;

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

    // Quads
    private Vector3 _newQuadPos;
    private GameObject _quadObject;
    private Mesh _quadMesh;
    private MeshFilter _quadMeshFilter;

    // Blocks
    [SerializeField] Dictionary<string, int> _blocksDataDictionary;

    // Chunks
    private GameObject _chunckObject;
    private MeshFilter _chunckMeshFilter;
    private MeshRenderer _chunckMeshRenderer;
    private List<CombineInstance> _combineInstanceList;
    #endregion

    #region Main Methods
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(GenerateWorld());
    }
    #endregion

    #region Helper Methods

    #region World and Chunks
    private IEnumerator GenerateWorld()
    {
        // Create a quad which will serve as a template mesh
        SetupQuadMeshData();

        // Generate blocks dataset
        _blocksDataDictionary = new Dictionary<string, int>(); // Exaple item: "5 -15 10" = 1 or "X5 Y-15 Z10" has the stone block
        for (int x = -(int)worldSize.x; x <= worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int z = -(int)worldSize.z; z <= worldSize.z; z++)
                {
                    GenerateBlocksData(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z);
                }
            }
        }
        //Debug.Log($"Generated {_blocksDataDictionary.Count} blocks data");

        // Generate world mesh
        for (int x = -(int)worldSize.x; x <= worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int z = -(int)worldSize.z; z <= worldSize.z; z++)
                {
                    GenerateChunck(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z);
                }
            }
        }

        // Destroy template quad
        Destroy(_quadObject);

        yield return null;
    }
    
    private void GenerateBlocksData(int startX, int startY, int startZ)
    {
        //Debug.Log($"Generating block data for chunk X:{startX} Y:{startY} Z:{startZ}");

        // Generate block dataset
        for (int x = startX; x < chunkSize.x + startX; x++)
        {
            for (int y = startY; y < chunkSize.y + startY; y++)
            {
                for (int z = startZ; z < chunkSize.z + startZ; z++)
                {
                    // Set blocks as empty by default
                    _blocksDataDictionary.Add($"{x} {y} {z}", -1);

                    // Choose which block to use by layer rules
                    for (int b = 0; b < blocks.Length; b++)
                    {
                        if (y >= blocks[b].minLayer && y <= blocks[b].maxLayer)
                        {
                            _blocksDataDictionary[$"{x} {y} {z}"] = b;

                            //Debug.Log($"Created block data X:{x} Y:{y} Z:{z} Type:{blocks[_blocksDataDictionary[$"{x} {y} {z}"]].screenName}");
                        }
                    }

                    // Make some holes for debug only
                    if (UnityEngine.Random.Range(0, 100) < holesChance) //
                        _blocksDataDictionary[$"{x} {y} {z}"] = -1; //
                }
            }
        }
    }

    private void GenerateChunck(int startX, int startY, int startZ)
    {
        //Debug.Log($"Generating mesh for chunk X:{startX} Y:{startY} Z:{startZ}");

        // Create new chunck object
        _chunckObject = new GameObject($"Chunck X:{startX} Y:{startY} Z:{startZ}");
        _chunckObject.transform.parent = transform;
        //_chunckObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);

        // Instantiate mesh list
        _combineInstanceList = new List<CombineInstance>();

        // Generate blocks meshes
        for (int x = startX; x < chunkSize.x + startX; x++)
        {
            for (int y = startY; y < chunkSize.y + startY; y++)
            {
                for (int z = startZ; z < chunkSize.z + startZ; z++)
                {
                    //Debug.Log(_blocksDataDictionary[$"{x} {y} {z}"]);

                    // If block is not empty
                    if (_blocksDataDictionary[$"{x} {y} {z}"] > -1)
                    {
                        // Generate a cube at the position
                        _newQuadPos = new Vector3(x, y, z);
                        GenerateBlock(x, y, z);
                    }
                }
            }
        }

        // Combine meshes
        CombineQuadsIntoSingleChunck();
    }
    #endregion

    #region Quads and Blocks
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
                _uvs = blocks[blockType].GetBottomUV();
                break;
            case CubeSide.Top:
                _vertices = new Vector3[] { _vertice7, _vertice6, _vertice5, _vertice4 };
                _normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                _uvs = blocks[blockType].GetTopUV();
                break;
            case CubeSide.Left:
                _vertices = new Vector3[] { _vertice7, _vertice4, _vertice0, _vertice3 };
                _normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                _uvs = blocks[blockType].GetSideUV(); // right texture
                break;
            case CubeSide.Right:
                _vertices = new Vector3[] { _vertice5, _vertice6, _vertice2, _vertice1 };
                _normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                _uvs = blocks[blockType].GetSideUV(); // left texture
                break;
            case CubeSide.Front:
                _vertices = new Vector3[] { _vertice4, _vertice5, _vertice1, _vertice0 };
                _normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                _uvs = blocks[blockType].GetSideUV(); // back texture
                break;
            case CubeSide.Back:
                _vertices = new Vector3[] { _vertice6, _vertice7, _vertice3, _vertice2 };
                _normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                _uvs = blocks[blockType].GetSideUV(); // front texture
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

    private void GenerateBlock(int x, int y, int z)
    {
        //Debug.Log($"Creating block mesh for block X:{x} Y:{y} Z:{z}");

        // Generate the top of the cube
        if (y + 1 == worldSize.y * chunkSize.y || _blocksDataDictionary[$"{x} {y + 1} {z}"] == -1)
        {
            CreateQuad(CubeSide.Top, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
        // Generate the bottom of the cube
        if (y - 1 < 0 || _blocksDataDictionary[$"{x} {y - 1} {z}"] == -1)
        {
            CreateQuad(CubeSide.Bottom, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
        // Generate the front of the cube
        if (z + 1 == chunkSize.z * (worldSize.z + 1) || _blocksDataDictionary[$"{x} {y} {z + 1}"] == -1)
        {
            CreateQuad(CubeSide.Front, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
        // Generate the back of the cube
        if (z - 1 < -chunkSize.z * worldSize.z || _blocksDataDictionary[$"{x} {y} {z - 1}"] == -1)
        {
            CreateQuad(CubeSide.Back, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
        // Generate the right side of the cube
        if (x + 1 == chunkSize.x * (worldSize.x + 1) || _blocksDataDictionary[$"{x + 1} {y} {z}"] == -1)
        {
            CreateQuad(CubeSide.Right, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
        // Generate the left side of the cube
        if (x - 1 < -chunkSize.x * worldSize.z || _blocksDataDictionary[$"{x - 1} {y} {z}"] == -1)
        {
            CreateQuad(CubeSide.Left, _blocksDataDictionary[$"{x} {y} {z}"]);
        }
    }

    private void CombineQuadsIntoSingleChunck()
    {
        // Add combined mesh to the cube
        _chunckMeshFilter = _chunckObject.AddComponent<MeshFilter>();
        _chunckMeshFilter.mesh = new Mesh();
        _chunckMeshFilter.mesh.CombineMeshes(_combineInstanceList.ToArray());

        // Add renderer to the cube
        _chunckMeshRenderer = _chunckObject.AddComponent<MeshRenderer>();
        _chunckMeshRenderer.material = atlasMaterial;
    }

    #endregion

    #endregion
}

public enum CubeSide { Top, Bottom, Front, Back, Right, Left }
