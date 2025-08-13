using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

[DefaultExecutionOrder(-10000)]           // Play On Awake���� ���� �̺�Ʈ ����
[RequireComponent(typeof(PlayableDirector))]
public class CutsceneSkipperSimple : MonoBehaviour
{
    [Header("Input")]
    public KeyCode skipKey = KeyCode.Space;
    public bool holdToSkip = true;
    public float holdSeconds = 0.8f;

    [Header("Fade")]
    public bool fadeOnSkip = true;
    public float fadeOutSeconds = 0.3f;
    public float fadeInSeconds = 0.3f;

    [Header("Behavior")]
    public bool stopInsteadOfJump = true; // true: Stop()���� ����(����), false: ������ ���� �� Stop()
    public UnityEvent onSkip;             // �� �ε� �� ��ó�� ����

    PlayableDirector director;
    float holdTimer;
    bool listening;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.played += OnPlayed;     // �ִ��� �̸��� ����
        director.stopped += OnStopped;
    }

    void OnDestroy()
    {
        if (!director) return;
        director.played -= OnPlayed;
        director.stopped -= OnStopped;
    }

    void Start()
    {
        // �� �����ڸ��� Play On Awake�� �̹� ��� ���̸� ��� ����
        if (director && director.state == PlayState.Playing) OnPlayed(director);
    }

    void OnPlayed(PlayableDirector _)
    {
        listening = true;
        holdTimer = 0f;
        RPG.Control.PlayerController.isCutscenePlaying = true; // ESC���� ��� ����
    }

    void OnStopped(PlayableDirector _)
    {
        listening = false;
        holdTimer = 0f;
        RPG.Control.PlayerController.isCutscenePlaying = false;
    }

    void Update()
    {
        if (!listening) return;

        if (holdToSkip)
        {
            if (Input.GetKey(skipKey))
            {
                holdTimer += Time.unscaledDeltaTime;
                if (holdTimer >= holdSeconds) Skip();
            }
            if (Input.GetKeyUp(skipKey)) holdTimer = 0f;
        }
        else
        {
            if (Input.GetKeyDown(skipKey)) Skip();
        }
    }

    // UI ��ư���� �� �޼��带 OnClick���� �����ϸ� ��
    public void Skip()
    {
        if (!listening) return;
        StartCoroutine(SkipRoutine());
    }

    System.Collections.IEnumerator SkipRoutine()
    {
        listening = false;

        if (fadeOnSkip && RPG.SceneManagement.Fader.Instance != null)
        {
            RPG.SceneManagement.Fader.Instance.FadeOut(fadeOutSeconds);
            yield return new WaitForSecondsRealtime(fadeOutSeconds);
        }

        if (director)
        {
            if (stopInsteadOfJump)
            {
                director.Stop();  // stopped �̺�Ʈ �� ��Ʈ�� ���� ��ƾ�� ���� ����
            }
            else
            {
                director.time = director.duration;
                director.Evaluate();
                director.Stop();
            }
        }

        onSkip?.Invoke();

        if (fadeOnSkip && RPG.SceneManagement.Fader.Instance != null)
        {
            RPG.SceneManagement.Fader.Instance.FadeIn(fadeInSeconds);
        }
    }

}