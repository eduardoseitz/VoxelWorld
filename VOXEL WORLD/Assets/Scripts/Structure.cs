using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [CreateAssetMenu(fileName = "NewStructure", menuName = "VOXEL WORLD/Structure")]
    public class Structure : ScriptableObject
    {
        [Range(1, 100)]
        public float spawnChance = 50f;
        [Range(1,10)]
        public int maxAmount = 4;
        public Vector3 size;
        public int underBlockType;
        public BlockData[] blocks;
    }
}
