using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [System.Serializable]
    public class BlockData
    {
        public int blockType = -1; // -1 equals air
        public Vector3 location;
        //public int state; // For example a torch can be lit or unlit and a door can be closed or open
    }
}
