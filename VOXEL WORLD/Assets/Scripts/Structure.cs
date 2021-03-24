using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [CreateAssetMenu(fileName = "NewStructure", menuName = "VOXEL WORLD/Structure")]
    public class Structure : ScriptableObject
    {
        public Vector3 size;
        public BlockData[] blocks;
    }
}
