using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShatterManager : MonoBehaviour
{

    public Animator bossAnimator;
    [Tooltip("비활성 자식까지 포함할지")]
    public bool includeInactive = false;

    [Tooltip("폭발을 파츠마다 랜덤 지연(초). 0이면 동시에 터짐")]
    public float delaySpread = 0f;

    List<BossShatter> parts;

    void Awake()
    {
        parts = new List<BossShatter>(GetComponentsInChildren<BossShatter>(includeInactive));
    }

    // 타임라인 시그널에서 이 메서드만 호출하면 됨
    public void ExplodeAll()
    {
        if (bossAnimator != null)
            bossAnimator.enabled = false;
        if (delaySpread <= 0f)
        {
            foreach (var p in parts) if (p) p.Fly();
        }
        else
        {
            StartCoroutine(ExplodeWithSpread());
        }
    }

    IEnumerator ExplodeWithSpread()
    {
        // 파츠 순서 랜덤화(선택)
        for (int i = 0; i < parts.Count; i++)
        {
            int j = Random.Range(i, parts.Count);
            (parts[i], parts[j]) = (parts[j], parts[i]);
        }

        foreach (var p in parts)
        {
            if (p) p.Fly();
            yield return new WaitForSeconds(Random.Range(0f, delaySpread));
        }
    }
}
