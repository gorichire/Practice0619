using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;

using RPG.Attributes;
using RPG.Control; // �ʰ� ���� Health ���ӽ����̽�

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
    public GameObject bossRoot;             // ���� ��ü(��Ų�޽ð� �޸� ��Ʈ)
    public GameObject fracturedRoot;        // ���� ������/��Ʈ(������ ���⼭ �ѱ�)
    Renderer[] _bossRenderers;

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        if (playerRoot) _playerRenderers = playerRoot.GetComponentsInChildren<Renderer>(true);
        if (bossRoot) _bossRenderers = bossRoot.GetComponentsInChildren<Renderer>(true);
    }

    // Health���� ��� �̺�Ʈ�� ������ ���⿡ ����
    public void OnBossDied()
    {
        PlayerController.isCutscenePlaying = true;
        if (played) return;
        played = true;
        HidePlayer();
        HideBossModel();
        // AI/�̵� ����
        if (agent) agent.enabled = false;
        if (ctx) ctx.enabled = false;  // FSM ������Ʈ ����

        // �ƾ� ���
        Time.timeScale = 1f;             // Ȥ�� ���θ��/�Ͻ����� ���̸� ����
        director.time = 0;
        director.Evaluate();             // ù ������ ����
        director.Play();

        // �ʿ�� �ٸ� ��ũ��Ʈ�鵵 ��Ȱ��ȭ
        foreach (var mb in scriptsToDisable)
            if (mb) mb.enabled = false;
    }
    public void HidePlayer()
    {
        foreach (var r in playerRoot.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        foreach (var tr in playerRoot.GetComponentsInChildren<TrailRenderer>(true)) tr.enabled = false;
        foreach (var lr in playerRoot.GetComponentsInChildren<LineRenderer>(true)) lr.enabled = false;

        // ��ƼŬ�� ���ֱ�(�ִٸ�)
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
