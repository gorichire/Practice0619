using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using RPG.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    UIDocument doc;
    Button startBtn, continueBtn, optBtn, quitBtn;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        var root = doc?.rootVisualElement;
        if (root == null) return;

        startBtn = root.Q<Button>("StartBtn");
        continueBtn = root.Q<Button>("ContinueBtn");
        optBtn = root.Q<Button>("OptionsBtn");
        quitBtn = root.Q<Button>("QuitBtn");

        if (startBtn != null) startBtn.clicked += OnStart;
        if (continueBtn != null) continueBtn.clicked += OnContinue;
        if (optBtn != null) optBtn.clicked += OnOptions;
        if (quitBtn != null) quitBtn.clicked += OnQuit;
    }

    void OnDisable()
    {
        if (startBtn != null) startBtn.clicked -= OnStart;
        if (continueBtn != null) continueBtn.clicked -= OnContinue;
        if (optBtn != null) optBtn.clicked -= OnOptions;
        if (quitBtn != null) quitBtn.clicked -= OnQuit;
    }
    void OnContinue()      
    {
        var wrapper = FindObjectOfType<SavingWrapper>();
        if (wrapper != null) wrapper.ContinueGame();
        else Debug.LogWarning("SavingWrapper ����");              
    }

    void OnStart()
    {
        // ���� ���� �� �̸����� ����
        SceneManager.LoadScene("Sandbox");
    }

    void OnOptions()
    {
        // ���� �ɼ� �г� ����
        Debug.Log("Open Options");
    }

    void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}