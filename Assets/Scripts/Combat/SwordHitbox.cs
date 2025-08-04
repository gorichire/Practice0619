using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Attributes;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class SwordHitbox : MonoBehaviour
    {
        private GameObject owner;
        private HashSet<Health> alreadyHit = new HashSet<Health>();
        private Collider hitbox;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] UnityEvent sowrdOnHit;

        private void Awake()
        {
            hitbox = GetComponent<Collider>();
            hitbox.enabled = false;
        }
        public void SetOwner(GameObject attacker)
        {
            owner = attacker;
        }

        public void Activate()
        {
            alreadyHit.Clear(); // 새 공격 시작 시 초기화
            hitbox.enabled = true;
        }

        public void Deactivate()
        {
            hitbox.enabled = false;
        }

        public void SowrdOnHit()
        {
            sowrdOnHit.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!hitbox.enabled) return;
            if (other.gameObject == owner) return;

            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;
            if (targetHealth.IsDead()) return;

            if (hitEffectPrefab != null)
            {
                Vector3 contact = other.ClosestPoint(transform.position);
                Quaternion rot = Quaternion.LookRotation(-transform.forward);

                GameObject fx = Instantiate(hitEffectPrefab, contact, rot);
            }
            SowrdOnHit();
            ImpactFX.I.HitStop(0.08f, 0f, 1f, 1f);

            if (alreadyHit.Contains(targetHealth)) return;

            var fighter = owner.GetComponent<Fighter>();
            float damage = fighter.CalculateAttackDamage(); 

            targetHealth.TakeDamage(owner, damage);
            alreadyHit.Add(targetHealth);
        }
    }
}