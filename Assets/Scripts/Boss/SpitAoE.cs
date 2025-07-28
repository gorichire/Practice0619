using UnityEngine;
using System.Collections;

public class SpitAoE : MonoBehaviour
{
    public float enableDelay = 1f;
    public float activeDuration = 0.5f; // 충돌 허용 시간
    public float damage = 20f;
    public LayerMask playerMask;
    SphereCollider col;
    bool damageDone;

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(enableDelay);
        col.enabled = true;

        // 이미 안에 플레이어가 서있으면 바로 판정
        CheckOverlapOnce();

        yield return new WaitForSeconds(activeDuration);
        col.enabled = false; // 비활성 후 잔류 VFX는 계속
    }

    void OnTriggerEnter(Collider other)
    {
        if (damageDone) return;
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            ApplyDamage(other.gameObject);
        }
    }

    void CheckOverlapOnce()
    {
        if (damageDone) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, col.radius, playerMask);
        foreach (var h in hits)
        {
            ApplyDamage(h.gameObject);
            break;
        }
    }

    void ApplyDamage(GameObject player)
    {
        damageDone = true;
        // TODO: player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<SphereCollider>();
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, col ? col.radius : 1f);
    }
#endif
}
