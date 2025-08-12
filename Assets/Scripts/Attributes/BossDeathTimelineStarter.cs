using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;

using RPG.Attributes;
using RPG.Control; // 너가 쓰는 Health 네임스페이스

public class BossDeathTimelineStarter : MonoBehaviour
{
    public PlayableDirector director;
    public BossFSM.BossContext ctx;     
    public Animator bossAnimator;       
    public NavMeshAgent agent;
    public MonoBehaviour[] scriptsToDisable;

    bool played;

    [Header("Player hide/show")]
    public GameObject playerRoot;      
    Renderer[] _playerRenderers;

    [Header("Boss hide/show")]
    public GameObject bossRoot;             // 보스 본체(스킨메시가 달린 루트)
    public GameObject fracturedRoot;        // 파편 프리팹/루트(있으면 여기서 켜기)
    Renderer[] _bossRenderers;

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        if (playerRoot) _playerRenderers = playerRoot.GetComponentsInChildren<Renderer>(true);
        if (bossRoot) _bossRenderers = bossRoot.GetComponentsInChildren<Renderer>(true);
    }

    // Health에서 사망 이벤트가 있으면 여기에 연결
    public void OnBossDied()
    {
        PlayerController.isCutscenePlaying = true;
        if (played) return;
        played = true;
        HidePlayer();
        HideBossModel();
        // AI/이동 정지
        if (agent) agent.enabled = false;
        if (ctx) ctx.enabled = false;  // FSM 업데이트 차단

        // 컷씬 재생
        Time.timeScale = 1f;             // 혹시 슬로모션/일시정지 중이면 복구
        director.time = 0;
        director.Evaluate();             // 첫 프레임 정렬
        director.Play();

        // 필요시 다른 스크립트들도 비활성화
        foreach (var mb in scriptsToDisable)
            if (mb) mb.enabled = false;
    }
    public void HidePlayer()
    {
        foreach (var r in playerRoot.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        foreach (var tr in playerRoot.GetComponentsInChildren<TrailRenderer>(true)) tr.enabled = false;
        foreach (var lr in playerRoot.GetComponentsInChildren<LineRenderer>(true)) lr.enabled = false;

        // 파티클도 꺼주기(있다면)
        foreach (var ps in playerRoot.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var r = ps.GetComponent<Renderer>(); if (r) r.enabled = false;
        }
    }

    public void ShowPlayer()
    {
        PlayerController.isCutscenePlaying = false;
        foreach (var r in playerRoot.GetComponentsInChildren<Renderer>(true)) r.enabled = true;
        foreach (var tr in playerRoot.GetComponentsInChildren<TrailRenderer>(true)) tr.enabled = true;
        foreach (var lr in playerRoot.GetComponentsInChildren<LineRenderer>(true)) lr.enabled = true;
        foreach (var ps in playerRoot.GetComponentsInChildren<ParticleSystem>(true)) ps.Play(true);
    }
    public void HideBossModel()
    {
        if (_bossRenderers == null && bossRoot)
            _bossRenderers = bossRoot.GetComponentsInChildren<Renderer>(true);
        if (_bossRenderers != null)
            foreach (var r in _bossRenderers) if (r) r.enabled = false;
    }
}
