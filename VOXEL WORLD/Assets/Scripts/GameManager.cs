using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace DevPenguin.VOXELWORLD
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public TextMeshProUGUI debugText;


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
            debugText.text = "Generating New World...";
            StartCoroutine(CreateNewWorld());
        }

        private IEnumerator CreateNewWorld()
        {
            WorldGenerator.instance.StartCoroutine(WorldGenerator.instance.GenerateWorld());

            yield return new WaitForSeconds(0);
        }

        // Update is called once per frame
        void Update()
        {
            // Recreate world
            if (Input.GetKeyDown(KeyCode.R))
            {
                debugText.text = "Generating New World...";
                SceneManager.LoadSceneAsync(0);
            }
        }
    } 
}
