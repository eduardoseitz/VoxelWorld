using TMPro;
using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class CanvasManager : MonoBehaviour
    {
        public static CanvasManager instance;

        public GameObject backgroundMenuPanel;
        public GameObject mainMenuPanel;
        public GameObject loadingPanel;
        public TextMeshProUGUI loadingInfoText;
        public GameObject pausedPanel;
        public GameObject hudPanel;
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
            mainMenuPanel.SetActive(true);
        }
    }
}
