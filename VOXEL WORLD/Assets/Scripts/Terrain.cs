using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [CreateAssetMenu(fileName = "NewTerrain", menuName = "VOXEL WORLD/Terrain")]
    public class Terrain : ScriptableObject
    {
        public int height = 48; // Where is the ground level
        public float smoothness = 0.02f; // The increment
        public float persistance = 0.5f; // The amplitude combined with the value
        public int octaves = 2; // How many waves will be combined
    }
}
