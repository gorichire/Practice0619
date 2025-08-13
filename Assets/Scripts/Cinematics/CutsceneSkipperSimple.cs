using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

[DefaultExecutionOrder(-10000)]           // Play On Awake보다 먼저 이벤트 구독
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
    public bool stopInsteadOfJump = true; // true: Stop()으로 종료(안전), false: 끝으로 점프 후 Stop()
    public UnityEvent onSkip;             // 씬 로드 등 후처리 연결

    PlayableDirector director;
    float holdTimer;
    bool listening;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.played += OnPlayed;     // 최대한 이르게 구독
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
        // 씬 들어오자마자 Play On Awake로 이미 재생 중이면 즉시 진입
        if (director && director.state == PlayState.Playing) OnPlayed(director);
    }

    void OnPlayed(PlayableDirector _)
    {
        listening = true;
        holdTimer = 0f;
        RPG.Control.PlayerController.isCutscenePlaying = true; // ESC무시 등과 연동
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

    // UI 버튼에서 이 메서드를 OnClick으로 연결하면 됨
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
                director.Stop();  // stopped 이벤트 → 컨트롤 복구 루틴과 궁합 좋음
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