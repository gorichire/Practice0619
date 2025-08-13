using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  
using System.Collections;
using Cinemachine;

namespace RPG.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject pauseUI;    
        [SerializeField] GameObject optionsUI; 

        [Header("Buttons")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button backButton;     
        [SerializeField] CinemachineVirtualCamera vcam;

        bool isPaused;

        void Awake()
        {
            // 버튼 이벤트 연결
            resumeButton.onClick.AddListener(Resume);
            optionsButton.onClick.AddListener(OpenOptions);
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            backButton.onClick.AddListener(CloseOptions);

            // 시작 시 패널 꺼두기
            pauseUI.SetActive(false);
            optionsUI.SetActive(false);
            if(vcam == null) vcam = FindObjectOfType<CinemachineVirtualCamera>();
        }

        void Update()
        {
            if (RPG.Control.PlayerController.isCutscenePlaying) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (optionsUI.activeSelf) CloseOptions();
                else if (isPaused) Resume();
                else Pause();
            }
        }

        void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            ToggleCamera(false);
            TogglePlayerControl(false);

            pauseUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            ToggleCamera(true);
            TogglePlayerControl(true);

            pauseUI.SetActive(false);
            optionsUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /* --------- Options --------- */
        void OpenOptions()
        {
            pauseUI.SetActive(false);
            optionsUI.SetActive(true);
        }

        void CloseOptions()
        {
            optionsUI.SetActive(false);
            pauseUI.SetActive(true);
        }

        /* --------- Main Menu --------- */
        public void GoToMainMenu()
        {
            StartCoroutine(LoadMainMenuCo());
        }
        IEnumerator LoadMainMenuCo()
        {
            Time.timeScale = 1f;          
            yield return new WaitForEndOfFrame();

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        /* --------- Helper --------- */
        void TogglePlayerControl(bool enable)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            var pc = player.GetComponent<RPG.Control.PlayerController>();
            var hotkeys = player.GetComponent<RPG.Control.PlayerWeaponHotkeys>();
            var dodge = player.GetComponent<RPG.Control.PlayerDodge>();

            if (pc) pc.enabled = enable;
            if (hotkeys) hotkeys.enabled = enable;
            if (dodge) dodge.enabled = enable;
        }
        void ToggleCamera(bool enable)
        {
            if (!vcam) return;

            var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null) pov.enabled = enable;
        }

    }
}
