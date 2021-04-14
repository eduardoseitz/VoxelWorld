using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.Messaging.Internal;

namespace DevPenguin.VOXELWORLD
{
    public class WorldGenerator : MonoBehaviour
    {
        #region Declarations
        public static WorldGenerator instance;

        public bool IsWorldDynamic => isWorldDynamic;

        [Header("World Setup")]
        [SerializeField] int terrainSeed = 468786;
        [SerializeField] bool shouldRandomizeSeed = true;
        [Tooltip("Chunk size in blocks")]
        [SerializeField] private Vector3 chunkSize = new Vector3(16, 16, 16);
        [Tooltip("World size in chunks")]
        [SerializeField] private Vector3 worldSize = new Vector3(4, 2, 4);
        [SerializeField] private bool isWorldDynamic = true;
        [Tooltip("Render size in chunks, can't be bigger than world")]
        [SerializeField] private Vector3 drawDistance = new Vector3(2,2,2);
        [SerializeField] private int distanceBeforeUpdate = 4;
        [Space(5f)]

        [Header("Blocks Setup")]
        [SerializeField] private Block[] allBlocks;
        [Space(3f)]
        [SerializeField] private Block[] ruleBlocks;
        [Space(3f)]
        [SerializeField] private Block[] oresBlocks;
        [Space(3f)]
        [SerializeField] private Block surfaceBlock;
        [SerializeField] private Block underSurfaceBlock;
        [SerializeField] private Block bedBlock;
        [Space(3f)]
        [SerializeField] private Material blocksMaterial;
        [Space(5f)]

        [Header("Terrain Setup")]
        [Header("Base Terrain")]
        [SerializeField] private Terrain surfaceTerrain;
        [SerializeField] private Terrain underSurfaceTerrain;
        [Header("Cave Terrain")]
        [SerializeField] private Terrain caveTerrain;
        [SerializeField] private float caveChance = 0.42f;
        [Space(5f)]

        [Header("Structures Setup")]
        [SerializeField] Structure[] structures;

        [Header("Debug")]
        [SerializeField] private bool shouldMakeHoles = false;
        [SerializeField] private int holesChance = 1;
        [Space(5f)]

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
        private ConcurrentDictionary<string, BlockData> _blocksDictionary; // A key looks like this $"{x} {y} {z}"

        // Chunks
        private ConcurrentDictionary<string, GameObject> _chunksDictionary;  // A key looks like this $"{x} {y} {z}"
        private GameObject _chunkObject;
        private MeshFilter _chunkMeshFilter;
        private MeshRenderer _chunkMeshRenderer;
        private List<CombineInstance> _combineInstanceList;

        // Noise
        private NoiseGenerator noiseGenerator;

        // Player
        private Transform _player;
        private Vector3 _worldOrigin;
        private Vector3 _lastPlayerPosition;
        private bool _isUpdatingWorld;
        #endregion

        #region Main Methods
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            // World dynamic loading setup
            _player = GameManager.instance.player.transform;
            _worldOrigin = new Vector3(0, 0, 0);
            if (isWorldDynamic == false)
            {
                drawDistance = worldSize;
            }
        }
        #endregion

        #region Helper Methods

        #region World and Chunks

        public IEnumerator GenerateNewWorld()
        {
            // Debug
            float _startTime = Time.realtimeSinceStartup;
            float _uiDelay = 0.01f;
            int _numberOfSteps = ((int)worldSize.z * (int)worldSize.x * (int)worldSize.y) * ((int)worldSize.z * (int)worldSize.x * (int)worldSize.y) / 2;
            float _currentStep = 0f;
            float _currentProgress = 1f;

            // Update UI
            CanvasManager.instance.mainMenuPanel.SetActive(false);
            CanvasManager.instance.loadingInfoText.text = "Setting up...\n" + Mathf.Clamp(_currentProgress, 0, 100).ToString("00") + "%";
            CanvasManager.instance.loadingPanel.SetActive(true);

            yield return new WaitForSeconds(_uiDelay);

            // Setup noise seed
            if (shouldRandomizeSeed)
                terrainSeed = Random.Range(-999999, 999999);
            noiseGenerator = new NoiseGenerator(terrainSeed);

            // Generate blocks dataset
            _blocksDictionary = new ConcurrentDictionary<string, BlockData>(); // Exaple item: "5 -15 10" = 1 or "X5 Y-15 Z10" has the stone block
            for (int z = -(int)worldSize.z; z < (int)worldSize.z; z++)
            {
                for (int x = -(int)worldSize.x; x < (int)worldSize.x; x++)
                {
                    for (int y = 0; y < worldSize.y; y++)
                    {
                        // Update loading screen
                        _currentStep += 1;
                        _currentProgress = _currentStep / _numberOfSteps * 100;
                        CanvasManager.instance.loadingInfoText.text = "Generating chunks...\n" + Mathf.Clamp(_currentProgress, 0, 100).ToString("00") + "%";
                        yield return new WaitForSeconds(_uiDelay);

                        StartCoroutine(GenerateBlocksData(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z));
                    }
                }
            }

            // Create a quad which will serve as a template mesh
            SetupQuadMeshData();

            // Generate world mesh
            _chunksDictionary = new ConcurrentDictionary<string, GameObject>(); // Holds current loaded chuncks for later use
            for (int z = -(int)worldSize.z; z < worldSize.z; z++)
            {
                for (int x = -(int)worldSize.x; x < worldSize.x; x++)
                {
                    for (int y = 0; y < worldSize.y; y++)
                    {
                        // Update loading screen
                        _currentStep += 1;
                        _currentProgress = _currentStep / _numberOfSteps * 100;
                        CanvasManager.instance.loadingInfoText.text = "Building world...\n " + Mathf.Clamp(_currentProgress, 0, 100).ToString("00") + "%";
                        yield return new WaitForSeconds(_uiDelay);

                        StartCoroutine(GenerateChunk(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z));
                    }
                }
            }
            // Unhide chunks withing player view
            for (int z = -(int)drawDistance.z; z < drawDistance.z; z++)
            {
                for (int x = -(int)drawDistance.x; x < drawDistance.x; x++)
                {
                    for (int y = 0; y < drawDistance.y; y++)
                    {
                        StartCoroutine(GenerateChunk(x * (int)chunkSize.x, y * (int)chunkSize.y, z * (int)chunkSize.z));
                    }
                }
            }

            // Destroy template quad
            //Destroy(_quadObject);

            // Update UI
            CanvasManager.instance.loadingPanel.SetActive(false);
            CanvasManager.instance.backgroundMenuPanel.SetActive(false);
            CanvasManager.instance.hudPanel.SetActive(true);

            // Setup player
            int _topGrassLayer = noiseGenerator.GetTerrainHeightNoise(0, 0, surfaceTerrain.smoothness, surfaceTerrain.octaves, surfaceTerrain.persistance, surfaceTerrain.groundHeight);
            int _topStoneLayer = noiseGenerator.GetTerrainHeightNoise(0 + 5, 0 + 5, underSurfaceTerrain.smoothness, underSurfaceTerrain.octaves, underSurfaceTerrain.persistance, underSurfaceTerrain.groundHeight) - 5;
            GameManager.instance.SetupPlayer(new Vector3(0, Mathf.Max(_topGrassLayer, _topStoneLayer), 0), Quaternion.identity);

            // Debug
            string _debugMessage = $"Generated world with {_chunksDictionary.Count} chunks with {_blocksDictionary.Count} blocks in {(Time.realtimeSinceStartup - _startTime).ToString("00.00")} seconds";
            Debug.Log(_debugMessage);
            CanvasManager.instance.debugText.text = _debugMessage;

            yield return null;
        }

        public IEnumerator UpdateWorldAroundPlayer()
        {
            _lastPlayerPosition = _player.position;

            while (isWorldDynamic)
            {
                bool _shouldUpdate = false;

                // If player has moved enough
                if (Mathf.Abs(_player.position.magnitude - _lastPlayerPosition.magnitude) > 0.9f)
                {
                    _lastPlayerPosition = _player.position;

                    // Check new player position
                    if (_player.position.x >= _worldOrigin.x + (drawDistance.x * chunkSize.x / distanceBeforeUpdate))
                    {
                        _worldOrigin.x += chunkSize.x;
                        _shouldUpdate = true;
                    }
                    else if (_player.position.x <= _worldOrigin.x - (drawDistance.x * chunkSize.x / distanceBeforeUpdate))
                    {
                        _worldOrigin.x -= chunkSize.x;
                        _shouldUpdate = true;
                    }
                    if (_player.position.z >= _worldOrigin.z + (drawDistance.z * chunkSize.z / distanceBeforeUpdate))
                    {
                        _worldOrigin.z += chunkSize.z;
                        _shouldUpdate = true;
                    }
                    else if (_player.position.z <= _worldOrigin.z - (drawDistance.z * chunkSize.z / distanceBeforeUpdate))
                    {
                        _worldOrigin.z -= chunkSize.z;
                        _shouldUpdate = true;
                    }

                    // If would has to be updated
                    if (_shouldUpdate)
                    {
                        float _startTime = Time.realtimeSinceStartup;

                        // Generate new blocks needed
                        for (int z = -(int)drawDistance.z; z < (int)drawDistance.z; z++)
                        {
                            for (int x = -(int)drawDistance.x; x < (int)drawDistance.x; x++)
                            {
                                for (int y = 0; y < drawDistance.y; y++)
                                {
                                    _isUpdatingWorld = true;
                                    StartCoroutine(GenerateBlocksData(x * (int)chunkSize.x + (int)_worldOrigin.x, y * (int)chunkSize.y, z * (int)chunkSize.z + (int)_worldOrigin.z));
                                    while (_isUpdatingWorld)
                                        yield return null;
                                }
                            }
                        }
                        yield return new WaitForSeconds(0.1f);

                        // Generate new chuncks needed
                        for (int z = -(int)drawDistance.z; z < drawDistance.z; z++)
                        {
                            for (int x = -(int)drawDistance.z; x < drawDistance.x; x++)
                            {
                                for (int y = 0; y < drawDistance.y; y++)
                                {
                                    _isUpdatingWorld = true;
                                    StartCoroutine(GenerateChunk(x * (int)chunkSize.x + (int)_worldOrigin.x, y * (int)chunkSize.y, z * (int)chunkSize.z + (int)_worldOrigin.z));
                                    while (_isUpdatingWorld)
                                        yield return null;
                                }
                            }
                        }
                        yield return new WaitForSeconds(0.1f);

                        // Destroy chuncks no longer needed
                        for (int z = -(int)drawDistance.z; z < drawDistance.z; z++)
                        {
                            for (int x = -(int)drawDistance.z - 1; x <= drawDistance.x; x += (int)drawDistance.x * 2 + 1)
                            {
                                for (int y = 0; y < drawDistance.y; y++)
                                {
                                    _isUpdatingWorld = true;
                                    StartCoroutine(RemoveChunck(x * (int)chunkSize.x + (int)_worldOrigin.x, y * (int)chunkSize.y, z * (int)chunkSize.z + (int)_worldOrigin.z));
                                    while (_isUpdatingWorld)
                                        yield return null;

                                }
                            }
                        }
                        for (int x = -(int)drawDistance.x; x < drawDistance.x; x++)
                        {
                            for (int z = -(int)drawDistance.z - 1; z <= drawDistance.z; z += (int)drawDistance.z * 2 + 1)
                            {
                                for (int y = 0; y < drawDistance.y; y++)
                                {
                                    _isUpdatingWorld = true;
                                    StartCoroutine(RemoveChunck(x * (int)chunkSize.x + (int)_worldOrigin.x, y * (int)chunkSize.y, z * (int)chunkSize.z + (int)_worldOrigin.z));
                                    while (_isUpdatingWorld)
                                        yield return null;

                                }
                            }
                        }

                        // Debug
                        //Debug.Log($"World update took {(Time.realtimeSinceStartup - _startTime).ToString("00.00")} seconds");
                    }
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
            }
        }

        private IEnumerator GenerateBlocksData(int startX, int startY, int startZ)
        {
            // Debug.Log($"Generating block data for chunk X:{startX} Y:{startY} Z:{startZ}");

            // If blocks data do not exist then create it
            if (_blocksDictionary.ContainsKey($"{startX} {startY} {startZ}") == false)
            {
                // Generate terrain dataset
                yield return null;
                GenerateTerrain(startX, startY, startZ);

                // Generate trees and houses
                yield return null;
                GenerateStructures(startX, startY, startZ);
            }

            _isUpdatingWorld = false;
            yield return null;
        }

        private void GenerateTerrain(int startX, int startY, int startZ)
        {
            try
            {
                for (int z = startZ; z < chunkSize.z + startZ; z++)
                {
                    for (int x = startX; x < chunkSize.x + startX; x++)
                    {
                        // Get height noise
                        int _topGrassLayer = noiseGenerator.GetTerrainHeightNoise(x, z, surfaceTerrain.smoothness, surfaceTerrain.octaves, surfaceTerrain.persistance, surfaceTerrain.groundHeight);
                        int _topStoneLayer = noiseGenerator.GetTerrainHeightNoise(x + 5, z + 5, underSurfaceTerrain.smoothness, underSurfaceTerrain.octaves, underSurfaceTerrain.persistance, underSurfaceTerrain.groundHeight) - 5;

                        for (int y = startY; y < chunkSize.y + startY; y++)
                        {
                            // Set blocks as empty by default
                            _blocksDictionary.TryAdd($"{x} {y} {z}", new BlockData());

                            // Else make it air by default
                            _blocksDictionary[$"{x} {y} {z}"].blockType = -1;

                            // If it is bellow the stone level 
                            if (y <= _topStoneLayer)
                            {
                                // Get terrain holes
                                float _caveFactor = noiseGenerator.Get3DNoise(x, y, z, caveTerrain.smoothness, caveTerrain.octaves, caveTerrain.persistance);

                                // Check if it isn't a cave
                                if (_caveFactor >= caveChance)
                                {
                                    // Else choose which block to use by layer rules
                                    for (int b = 0; b < ruleBlocks.Length; b++)
                                    {
                                        // Check if it is in the correct layer
                                        if (y > _topStoneLayer - ruleBlocks[b].maxDepthLayer && y <= _topStoneLayer - ruleBlocks[b].minDepthLayer)
                                        {
                                            // Check the chance of that block being
                                            _blocksDictionary[$"{x} {y} {z}"].blockType = ruleBlocks[b].blockType;
                                        }

                                        // Make some holes for mesh debug only
                                        if (shouldMakeHoles)
                                        {
                                            if (y != _topGrassLayer && _topGrassLayer != 0 && UnityEngine.Random.Range(0, 100) < holesChance)
                                                _blocksDictionary[$"{x} {y} {z}"].blockType = -1;
                                        }
                                    }

                                    // Generate ores
                                    for (int o = 0; o < oresBlocks.Length; o++)
                                    {
                                        if (y > _topStoneLayer - oresBlocks[o].maxDepthLayer && y <= _topStoneLayer - oresBlocks[o].minDepthLayer)
                                        {
                                            if (_caveFactor < oresBlocks[o].generationChance)
                                            {
                                                if (Random.Range(0, 100) < oresBlocks[o].generationChance * 100)
                                                {
                                                    _blocksDictionary[$"{x} {y} {z}"].blockType = oresBlocks[o].blockType;
                                                }
                                            }
                                        }
                                    }
                                }
                                // Else if it is a cave make a hole
                                else
                                {
                                    _blocksDictionary[$"{x} {y} {z}"].blockType = -1;
                                }
                            }
                            // If it is the surface
                            else if (y == _topGrassLayer)
                            {
                                // First layer is grass
                                _blocksDictionary[$"{x} {y} {z}"].blockType = surfaceBlock.blockType;

                                // Under the grass is dirt
                                _blocksDictionary[$"{x} {y - 1} {z}"].blockType = 0;
                                _blocksDictionary[$"{x} {y - 2} {z}"].blockType = 0;
                                _blocksDictionary[$"{x} {y - 3} {z}"].blockType = 0;

                                // Under the dirt is stone
                                _blocksDictionary[$"{x} {y - 4} {z}"].blockType = 2;
                                _blocksDictionary[$"{x} {y - 5} {z}"].blockType = 2;
                            }

                            // If it is the bedrock
                            if (y == 0)
                            {
                                _blocksDictionary[$"{x} {y} {z}"].blockType = bedBlock.blockType;
                            }

                            // Debug
                            //if (_blocksDictionary[$"{x} {y} {z}"].blockType > -1)
                            //{
                            //    Debug.Log($"Created block at X:{x} Y:{y} Z:{z} ");
                            //    Debug.Log($"Type:{allBlocks[_blocksDictionary[$"{x} {y} {z}"].blockType].screenName}");
                            //}
                        }
                    }
                }
            }
            catch
            {
                Debug.LogError($"Could not generate terrain for the chunk {startX} {startY} {startZ}");
            }
        }

        private void GenerateStructures(int startX, int startY, int startZ)
        {
            try {
                // Loop through all strucutres
                for (int i = 0; i < structures.Length; i++)
                {
                    // Pick a random number between 1 and max spawn
                    int _amount = Random.Range(1, structures[i].maxAmount);
                    for (int a = 0; a < _amount; a++)
                    {
                        // Check the chance of having it
                        if (Random.Range(0, 100) < structures[i].spawnChance)
                        {
                            // Pick a random position in the surface
                            int _randomX = Random.Range(startX + (int)structures[i].size.x, startX + (int)chunkSize.x - (int)structures[i].size.x);
                            int _randomZ = Random.Range(startZ + (int)structures[i].size.z, startZ + (int)chunkSize.z - (int)structures[i].size.z);
                            int _randomY = noiseGenerator.GetTerrainHeightNoise(_randomX, _randomZ, surfaceTerrain.smoothness, surfaceTerrain.octaves, surfaceTerrain.persistance, surfaceTerrain.groundHeight);

                            // Check if the y layer matches the needed block of the structure
                            if (_blocksDictionary[$"{_randomX} {_randomY} {_randomZ}"].blockType == structures[i].underBlockType)
                            {
                                // Check if structure y size doesn't exceed the worldsize height
                                if (_randomY + structures[i].size.y < chunkSize.y * worldSize.y)
                                {
                                    // Build all the required blocks
                                    for (int b = 0; b < structures[i].blocks.Length; b++)
                                    {
                                        int _posX = _randomX + (int)structures[i].blocks[b].location.x;
                                        int _posY = _randomY + (int)structures[i].blocks[b].location.y;
                                        int _posZ = _randomZ + (int)structures[i].blocks[b].location.z;

                                        _blocksDictionary[$"{_posX} {_posY} {_posZ}"].blockType = structures[i].blocks[b].blockType;
                                    }
                                }
                            }
                        }
                    } 
                }
            }
            catch
            {
                Debug.LogError($"Could not generate structures for the chunk {startX} {startY} {startZ}");
            }
        }

        private IEnumerator GenerateChunk(int startX, int startY, int startZ)
        {
            // Debug.Log($"Generating mesh for chunk X:{startX} Y:{startY} Z:{startZ}");

            // If chunk mesh exists then unhide it
            if (_chunksDictionary.ContainsKey($"{startX} {startY} {startZ}") == true)
            {
                if (_chunksDictionary[$"{startX} {startY} {startZ}"] != null)
                {
                    if (_chunksDictionary[$"{startX} {startY} {startZ}"].activeInHierarchy == false)
                        _chunksDictionary[$"{startX} {startY} {startZ}"].SetActive(true);
                }
            }
            // If blocks data do not exist then create it
            else
            {
                // Create new chunk object
                _chunkObject = new GameObject($"Chunk {startX} {startY} {startZ}");
                _chunkObject.transform.parent = transform;
                _chunkObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
                _chunkObject.SetActive(false);
                _chunksDictionary.TryAdd($"{startX} {startY} {startZ}", _chunkObject);

                // Instantiate mesh list
                _combineInstanceList = new List<CombineInstance>();

                // Generate blocks meshes
                for (int z = startZ; z < chunkSize.z + startZ; z++)
                {
                    for (int x = startX; x < chunkSize.x + startX; x++)
                    {
                        for (int y = startY; y < chunkSize.y + startY; y++)
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
                                Debug.LogError($"Could not generate block at position {x} {y} {z}");
                            }
                        }
                    }
                }
                yield return null;

                // Combine meshes
                CombineQuadsIntoSingleChunk();
            }

            _isUpdatingWorld = false;
            yield return null;
        }

        private IEnumerator RemoveChunck(int startX, int startY, int startZ)
        {
            //Debug.Log($"Removing mesh for chunk X:{startX} Y:{startY} Z:{startZ}");

            // If chunck data do exist then remove it
            if (_chunksDictionary.ContainsKey($"{startX} {startY} {startZ}") == true)
            {
                if (_chunksDictionary[$"{startX} {startY} {startZ}"] != null)
                {
                    // If chunk is visible then hide it
                    if (_chunksDictionary[$"{startX} {startY} {startZ}"].activeInHierarchy)
                        _chunksDictionary[$"{startX} {startY} {startZ}"].SetActive(false);
                }
            }

            // StartCoroutine(DeleteChunck(startX, startY, startZ));

            _isUpdatingWorld = false;
            yield return null;
        }

        private IEnumerator DeleteChunck(int startX, int startY, int startZ)
        {
            yield return new WaitForSeconds(5f);

            // If it is hidden for a long time destroy it
            if (_chunksDictionary.ContainsKey($"{startX} {startY} {startZ}") == true)
            {
                if (_chunksDictionary[$"{startX} {startY} {startZ}"] != null)
                {
                    if (_chunksDictionary[$"{startX} {startY} {startZ}"].activeInHierarchy == false)
                    {
                        Destroy(_chunksDictionary[$"{startX} {startY} {startZ}"].gameObject);
                        _chunksDictionary[$"{startX} {startY} {startZ}"] = null;
                    }
                }
            }
            yield return null;
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
                //Debug.Log($"Could not generate mesh for block X:{x} Y:{y} Z:{z}");
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
            _chunkMeshRenderer.material = blocksMaterial;

            // Add mesh collider
            _chunkObject.AddComponent<MeshCollider>();
        }
        #endregion

        #endregion
    }

    public enum CubeSide { Top, Bottom, Front, Back, Right, Left }
}
