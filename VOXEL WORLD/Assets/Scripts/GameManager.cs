using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace DevPenguin.VOXELWORLD
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public GameObject player;

        [SerializeField] Vector3 playerSpawnOffset = new Vector3(0, 3, 0);

        private Vector3 _spawnPosition;

        //public TextMeshProUGUI debugText;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            player.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // Recreate world
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadSceneAsync(0);
            }

            if (player.transform.position.y < - 10)
                player.transform.SetPositionAndRotation(_spawnPosition, Quaternion.identity);
        }

        public void CreateNewWorld()
        {
            Debug.Log("Creating world");
            WorldGenerator.instance.StartCoroutine(WorldGenerator.instance.GenerateWorld());
        }

        public void QuitGame()
        {
            Debug.Log("Quiting game");
            Application.Quit();
        }

        public void SetupPlayer(Vector3 playerPosition, Quaternion playerRotation)
        {
            _spawnPosition = playerPosition + playerSpawnOffset;
            player.transform.SetPositionAndRotation(_spawnPosition, playerRotation);
            player.SetActive(true);
        }
    } 
}
