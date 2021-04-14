using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class PlayerController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (WorldGenerator.instance)
                WorldGenerator.instance.StartCoroutine(WorldGenerator.instance.UpdateWorldAroundPlayer());
        }

        private void OnDisable()
        {
            if (WorldGenerator.instance)
                WorldGenerator.instance.StopCoroutine(WorldGenerator.instance.UpdateWorldAroundPlayer());
        }
    }
}
