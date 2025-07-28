using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;   // �� �߰�
using System.Collections;

namespace RPG.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject pauseUI;     // PauseMenu �г�
        [SerializeField] GameObject optionsUI;   // OptionsPanel

        [Header("Buttons")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button backButton;      // Options �� Pause ��

        bool isPaused;

        void Awake()
        {
            // ��ư �̺�Ʈ ����
            resumeButton.onClick.AddListener(Resume);
            optionsButton.onClick.AddListener(OpenOptions);
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            backButton.onClick.AddListener(CloseOptions);

            // ���� �� �г� ���α�
            pauseUI.SetActive(false);
            optionsUI.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (optionsUI.activeSelf) CloseOptions();   // �ɼ�â ���������� �ڷ�
                else if (isPaused) Resume();
                else Pause();
            }
        }

        /* --------- Pause / Resume --------- */
        void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            TogglePlayerControl(false);

            pauseUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
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
    }
}
