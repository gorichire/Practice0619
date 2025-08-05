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
            // 1 �� ��� �� ���� �۵�
            yield return new WaitForSeconds(enableDelay);

            float elapsed = 0f;
            while (elapsed < activeDuration)
            {
                ApplyTickDamage();
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            // ���� VFX�� �״�� �ΰ� �浹�� ��
            col.enabled = false;
        }

        void ApplyTickDamage()
        {
            // OverlapSphere�� �ö��̴� on/off�� �����ϰ� �ݰ��� �������� �˻�
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