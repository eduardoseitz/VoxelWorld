﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class WorldGenerator : MonoBehaviour
    {
        #region Declarations
        [Header("World Setup")]
        [SerializeField] int terrainSeed = 468786;
        [SerializeField] bool randomizeSeed = true;
        [Tooltip("Chunk size in blocks")]
        [SerializeField] private Vector3 chunkSize = new Vector3(16, 16, 16);
        [Tooltip("World size in chunks")]
        [SerializeField] private Vector3 worldSize = new Vector3(3, 3, 3);
        [SerializeField] private int terrainMaxGroundHeight = 100; // Where is the ground level
        [SerializeField] private float terrainSmoothness = 0.0015f; // The increment
        [SerializeField] private float terrainPersistance = 0.5f; // The amplitude combined with the value
        [SerializeField] private int terrainOctaves = 3; // How many waves will be combined
        [SerializeField] private Material atlasMaterial;

        [Header("Blocks Setup")]
        [SerializeField] private BlockType[] allBlocks;
        [SerializeField] private BlockType[] ruleBlocks;
        [SerializeField] private BlockType bedBlock;
        [SerializeField] private BlockType surfaceBlock;

        [Header("Debug")]
        [SerializeField] private int holesChance = 10;
        private float _startTime;

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
        private Dictionary<string, BlockData> _blocksDictionary;

        // Chunks
        private Dictionary<string, GameObject> _chunksDictionary;
        private GameObject _chunkObject;
        private MeshFilter _chunkMeshFilter;
        private MeshRenderer _chunkMeshRenderer;
        private List<CombineInstance> _combineInstanceList;

        // Noise
        private NoiseGenerator noiseGenerator;
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
            // Debug
            _startTime = Time.realtimeSinceStartup;

            // Setup noise seed
            if (randomizeSeed)
                terrainSeed = Random.Range(-999999, 999999);
            noiseGenerator = new NoiseGenerator(terrainSmoothness, terrainOctaves, terrainPersistance, terrainMaxGroundHeight, terrainSeed);

            // Generate blocks dataset
            _blocksDictionary = new Dictionary<string, BlockData>(); // Exaple item: "5 -15 10" = 1 or "X5 Y-15 Z10" has the stone block
            for (int x = -(int)worldSize.x; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    for (int z = -(int)worldSize.z; z < worldSize.z; z++)
                    {
                        GenerateBlocksData(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z);
                    }
                }
            }

            // Create a quad which will serve as a template mesh
            SetupQuadMeshData();

            // Generate world mesh
            _chunksDictionary = new Dictionary<string, GameObject>(); // Holds current loaded chuncks for later use
            for (int x = -(int)worldSize.x; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    for (int z = -(int)worldSize.z; z < worldSize.z; z++)
                    {
                        GenerateChunk(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z);
                    }
                }
            }

            // Destroy template quad
            Destroy(_quadObject);

            // Debug
            Debug.Log($"Generated {_chunksDictionary.Count} chunks with {_blocksDictionary.Count} blocks in {(Time.realtimeSinceStartup - _startTime).ToString()} seconds");

            yield return null;
        }

        private void GenerateBlocksData(int startX, int startY, int startZ)
        {
            //Debug.Log($"Generating block data for chunk X:{startX} Y:{startY} Z:{startZ}");

            // Generate terrain dataset
            for (int x = startX; x < chunkSize.x + startX; x++)
            {
                for (int y = startY; y < chunkSize.y + startY; y++)
                {
                    for (int z = startZ; z < chunkSize.z + startZ; z++)
                    {
                        // Set blocks as empty by default
                        _blocksDictionary.Add($"{x} {y} {z}", new BlockData());

                        // Get height noise
                        int _topLayer = noiseGenerator.GetTerrainHeightNoise(x, z);
                        
                        // If it is bellow the noise height 
                        if (y <= _topLayer) 
                        {
                            // If it is the surface
                            if (y == _topLayer)
                            {
                                _blocksDictionary[$"{x} {y} {z}"].blockType = surfaceBlock.blockType;
                            }
                            // If it is the bedrock
                            else if (y == 0)
                            {
                                _blocksDictionary[$"{x} {y} {z}"].blockType = bedBlock.blockType;
                            }
                            // Else choose which block to use by layer rules
                            else
                            {
                                for (int b = 0; b < ruleBlocks.Length; b++)
                                {
                                    if (y > _topLayer - ruleBlocks[b].maxDepthLayer && y <= _topLayer - ruleBlocks[b].minDepthLayer)
                                    {
                                        _blocksDictionary[$"{x} {y} {z}"].blockType = ruleBlocks[b].blockType;
                                    }

                                    // Make some holes for mesh debug only
                                    if (y != _topLayer && _topLayer != 0 && UnityEngine.Random.Range(0, 100) < holesChance) //
                                        _blocksDictionary[$"{x} {y} {z}"].blockType = -1; //
                                }
                            }
                        }
                        // Else make it air
                        else
                        {
                            _blocksDictionary[$"{x} {y} {z}"].blockType = -1;
                        }

                        //Debug.Log($"Created block data X:{x} Y:{y} Z:{z} Type:{blocks[_blocksDataDictionary[$"{x} {y} {z}"]].screenName}");
                    }
                }
            }

            // TODO: Generate caves
            // TODO: Generate ores
            // TODO: Generate structures
            // TODO: Generate vegetation
            // Generate trees
            int _ramdonX = Random.Range(startX + 3, (int)chunkSize.x + startX - 3);
            int _ramdonZ = Random.Range(startZ + 3, (int)chunkSize.z + startZ - 3);
            int _randomY = noiseGenerator.GetTerrainHeightNoise(_ramdonX, _ramdonZ);
            _blocksDictionary[$"{_ramdonX} {_randomY + 1} {_ramdonZ}"].blockType = 3;
            _blocksDictionary[$"{_ramdonX} {_randomY + 2} {_ramdonZ}"].blockType = 3;
            _blocksDictionary[$"{_ramdonX} {_randomY + 3} {_ramdonZ}"].blockType = 3;
            _blocksDictionary[$"{_ramdonX} {_randomY + 4} {_ramdonZ}"].blockType = 3;
            _blocksDictionary[$"{_ramdonX} {_randomY + 5} {_ramdonZ}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX} {_randomY + 6} {_ramdonZ}"].blockType = 5;

            _blocksDictionary[$"{_ramdonX + 1} {_randomY + 3} {_ramdonZ}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX + 1} {_randomY + 4} {_ramdonZ}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX + 1} {_randomY + 5} {_ramdonZ}"].blockType = 5;

            _blocksDictionary[$"{_ramdonX - 1} {_randomY + 3} {_ramdonZ}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX - 1} {_randomY + 4} {_ramdonZ}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX - 1} {_randomY + 5} {_ramdonZ}"].blockType = 5;

            _blocksDictionary[$"{_ramdonX} {_randomY + 3} {_ramdonZ + 1}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX} {_randomY + 4} {_ramdonZ + 1}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX} {_randomY + 5} {_ramdonZ + 1}"].blockType = 5;

            _blocksDictionary[$"{_ramdonX} {_randomY + 3} {_ramdonZ - 1}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX} {_randomY + 4} {_ramdonZ - 1}"].blockType = 5;
            _blocksDictionary[$"{_ramdonX} {_randomY + 5} {_ramdonZ - 1}"].blockType = 5;
        }

        private void GenerateChunk(int startX, int startY, int startZ)
        {
            //Debug.Log($"Generating mesh for chunk X:{startX} Y:{startY} Z:{startZ}");

            // Create new chunk object
            _chunkObject = new GameObject($"Chunk {startX} {startY} {startZ}");
            _chunkObject.transform.parent = transform;
            _chunkObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
            _chunksDictionary.Add($"{startX} {startY} {startZ}", _chunkObject);

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
                        try
                        {
                            if (_blocksDictionary[$"{x} {y} {z}"].blockType > -1)
                            {
                                // Generate a cube at the position
                                _newQuadPos = new Vector3(x, y, z);
                                GenerateBlock(x, y, z);
                            }
                        }
                        catch
                        {
                            Debug.Log($"Could not get data from chunk {x} {y} {z}");
                        }
                    }
                }
            }

            // Combine meshes
            CombineQuadsIntoSingleChunk();
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
                    _uvs = allBlocks[blockType].GetBottomUV();
                    break;
                case CubeSide.Top:
                    _vertices = new Vector3[] { _vertice7, _vertice6, _vertice5, _vertice4 };
                    _normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    _uvs = allBlocks[blockType].GetTopUV();
                    break;
                case CubeSide.Left:
                    _vertices = new Vector3[] { _vertice7, _vertice4, _vertice0, _vertice3 };
                    _normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                    _uvs = allBlocks[blockType].GetSideUV(); // right texture
                    break;
                case CubeSide.Right:
                    _vertices = new Vector3[] { _vertice5, _vertice6, _vertice2, _vertice1 };
                    _normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                    _uvs = allBlocks[blockType].GetSideUV(); // left texture
                    break;
                case CubeSide.Front:
                    _vertices = new Vector3[] { _vertice4, _vertice5, _vertice1, _vertice0 };
                    _normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                    _uvs = allBlocks[blockType].GetSideUV(); // back texture
                    break;
                case CubeSide.Back:
                    _vertices = new Vector3[] { _vertice6, _vertice7, _vertice3, _vertice2 };
                    _normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                    _uvs = allBlocks[blockType].GetSideUV(); // front texture
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
            _quadObject.transform.parent = _chunkObject.transform;
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

            // Generate quads to the block faces needed
            try
            {
                // Generate the top of the cube
                if (y + 1 == worldSize.y * chunkSize.y || _blocksDictionary[$"{x} {y + 1} {z}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Top, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
                // Generate the bottom of the cube
                if (y - 1 < 0 || _blocksDictionary[$"{x} {y - 1} {z}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Bottom, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
                // Generate the front of the cube
                if (z + 2 > (chunkSize.z * worldSize.z) || _blocksDictionary[$"{x} {y} {z + 1}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Front, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
                // Generate the back of the cube
                if (z - 1 < -chunkSize.z * worldSize.z || _blocksDictionary[$"{x} {y} {z - 1}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Back, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
                // Generate the right side of the cube
                if (x + 2 > (chunkSize.x * worldSize.x) || _blocksDictionary[$"{x + 1} {y} {z}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Right, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
                // Generate the left side of the cube
                if (x - 1 < -chunkSize.x * worldSize.z || _blocksDictionary[$"{x - 1} {y} {z}"].blockType == -1)
                {
                    CreateQuad(CubeSide.Left, _blocksDictionary[$"{x} {y} {z}"].blockType);
                }
            }
            catch
            {
                Debug.Log($"Could not generate mesh for block X:{x} Y:{y} Z:{z}");
            }
        }

        private void CombineQuadsIntoSingleChunk()
        {
            // Add combined mesh to the cube
            _chunkMeshFilter = _chunkObject.AddComponent<MeshFilter>();
            _chunkMeshFilter.mesh = new Mesh();
            _chunkMeshFilter.mesh.CombineMeshes(_combineInstanceList.ToArray());

            // Add renderer to the cube
            _chunkMeshRenderer = _chunkObject.AddComponent<MeshRenderer>();
            _chunkMeshRenderer.material = atlasMaterial;

            // Add mesh collider
            _chunkObject.AddComponent<MeshCollider>();
        }

        #endregion

        #endregion
    }

    public enum CubeSide { Top, Bottom, Front, Back, Right, Left }
}
