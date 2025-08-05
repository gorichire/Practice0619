using UnityEngine;
using System.Collections;
using RPG.Attributes;

namespace BossFSM
{
    [RequireComponent(typeof(SphereCollider))]
    public class SpitAoE : MonoBehaviour
    {
        [Header("Timing")]
        public float enableDelay = 1f;
        public float activeDuration = 7f;
        public float tickInterval = 0.5f;
        [Header("Damage")]
        public float tickDamage = 10f;
        public LayerMask playerMask;
        [HideInInspector] public GameObject instigator;
        SphereCollider col;

        void Awake()
        {
            col = GetComponent<SphereCollider>();
        }

        void OnEnable() => StartCoroutine(TickRoutine());

        IEnumerator TickRoutine()
        {
            // 1 초 대기 후 본격 작동
            yield return new WaitForSeconds(enableDelay);

            float elapsed = 0f;
            while (elapsed < activeDuration)
            {
                ApplyTickDamage();
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            // 남은 VFX는 그대로 두고 충돌만 끔
            col.enabled = false;
        }

        void ApplyTickDamage()
        {
            // OverlapSphere는 컬라이더 on/off와 무관하게 반경을 기준으로 검색
            Collider[] hits = Physics.OverlapSphere(transform.position,
                                                    col.radius,
                                                    playerMask,
                                                    QueryTriggerInteraction.Ignore);

            foreach (var h in hits)
            {
                var hp = h.GetComponent<Health>();
                if (hp && !hp.IsDead())
                    hp.TakeDamage(instigator ? instigator : gameObject, tickDamage);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!col) col = GetComponent<SphereCollider>();
            Gizmos.color = new Color(0f, 1f, 0f, .25f);
            Gizmos.DrawSphere(transform.position, col ? col.radius : 1f);
        }
#endif
    }
}