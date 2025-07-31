using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Attributes;
using static UnityEngine.Rendering.DebugUI;

namespace RPG.Combat
{
    public class SwordHitbox : MonoBehaviour
    {
        private GameObject owner;
        private HashSet<Health> alreadyHit = new HashSet<Health>();
        private Collider hitbox;

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

        private void OnTriggerEnter(Collider other)
        {
            if (!hitbox.enabled) return;
            if (other.gameObject == owner) return;

            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;
            if (targetHealth.IsDead()) return;

            // 중복 타격 방지
            if (alreadyHit.Contains(targetHealth)) return;

            var fighter = owner.GetComponent<Fighter>();
            float damage = fighter.CalculateAttackDamage(); 

            targetHealth.TakeDamage(owner, damage);
            alreadyHit.Add(targetHealth);
        }
    }
}