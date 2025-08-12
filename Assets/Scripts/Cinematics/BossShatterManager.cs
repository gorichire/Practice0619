using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShatterManager : MonoBehaviour
{

    public Animator bossAnimator;
    [Tooltip("��Ȱ�� �ڽı��� ��������")]
    public bool includeInactive = false;

    [Tooltip("������ �������� ���� ����(��). 0�̸� ���ÿ� ����")]
    public float delaySpread = 0f;

    List<BossShatter> parts;

    void Awake()
    {
        parts = new List<BossShatter>(GetComponentsInChildren<BossShatter>(includeInactive));
    }

    // Ÿ�Ӷ��� �ñ׳ο��� �� �޼��常 ȣ���ϸ� ��
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
        // ���� ���� ����ȭ(����)
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
