using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class Terrain : ScriptableObject
    {
        public int height = 100; // Where is the ground level
        public float smoothness = 0.0015f; // The increment
        public float persistance = 0.5f; // The amplitude combined with the value
        public int octaves = 3; // How many waves will be combined
    }
}
