using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [CreateAssetMenu(fileName = "NewTerrain", menuName = "VOXEL WORLD/Terrain")]
    public class Terrain : ScriptableObject
    {
        [Tooltip("Noise increment")]
        public float smoothness = 0.0015f;
        [Tooltip("Combined amplitude to the noise result")]
        public float persistance = 0.5f;
        [Tooltip("How many noise waves will be combined")]
        public int octaves = 3;
        [Tooltip("How high should the terrain raise to")]
        public int groundHeight = 48;
    }
}
