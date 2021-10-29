using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] bool isInteractionEnabled = true;
        [SerializeField] private int blockReach = 5;

        private Camera _camera;
        private RaycastHit _chunckHit;

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

        private void Start()
        {
            _camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            if (isInteractionEnabled)
            {
                // Destroy blocks that are clicked on
                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _chunckHit, blockReach))
                    {
                        Vector3 _blockHit = _chunckHit.point - _chunckHit.normal / 2.0f;

                        //int x = (int)(Mathf.Round(_blockHit.x) - _meshHit.collider.gameObject.transform.position.x);
                        //int y = (int)(Mathf.Round(_blockHit.y) - _meshHit.collider.gameObject.transform.position.y);
                        //int z = (int)(Mathf.Round(_blockHit.z) - _meshHit.collider.gameObject.transform.position.z);

                        WorldGenerator.instance.UpdateChunck(_blockHit, _chunckHit.transform.position);

                        //Debug.Log("Clicked on " + _chunckHit.collider.gameObject.name + " at position " + _blockHit.ToString());
                    }
                }
            }
        }
    }
}
