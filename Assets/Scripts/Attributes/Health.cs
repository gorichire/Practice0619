using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RPG.Saving;
using Newtonsoft.Json.Linq;
using RPG.Stats;
using RPG.Core;
using RPG.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour , /*ISaveable ,*/ IJsonSaveable
    {
        [SerializeField] float regenerationPercentage = 100;
        [SerializeField] UnityEvent<float> takeDamage;
        [SerializeField] UnityEvent onDie;
        [SerializeField] bool canFlinch = true;


        LazyValue<float> healthPoints;

        bool isDead = false;
        bool isInvulnerable;

        public void SetInvulnerable(bool v) => isInvulnerable = v;
        private void Awake()
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }
        private void Start()
        {
            healthPoints.ForceInit();
        }
        private void OnEnable()
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
        }

        private void OnDisable()
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            if (isInvulnerable) 
            {
                print(" Dodge ");
                return;
            }
            //print(gameObject.name + " took damage: " + damage);

            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);
            if (healthPoints.value == 0)
            {
                onDie.Invoke();
                Die();
                AwardExperience(instigator);
            }
            else
            {
                if (canFlinch)
                    GetComponent<Animator>()?.SetTrigger("hit");
                takeDamage.Invoke(damage);
            }
        }
        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }
        public void Heal(float healthToRestore)
        {
            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints());
        }

        public float GetHealthPoints()
        {
            return healthPoints.value;
        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Die()
        {
            if (isDead) { return; }
            isDead = true;
            if (TryGetComponent(out Animator anim))
            {
                anim.SetTrigger("die");
            }
            if (TryGetComponent(out ActionSchduler scheduler))
            {
                scheduler.CancelCurrentAction();
            }
            if (TryGetComponent(out BossFSM.BossContext boss))
            {
                boss.StopAllCoroutines();        
                boss.enabled = false;            

                if (boss.agent && boss.agent.isOnNavMesh)
                {
                    boss.agent.ResetPath();
                    boss.agent.isStopped = true;
                    boss.agent.enabled = false;
                }
            }
        }

        private void RegenerateHealth()
        {
            float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);
        }

        //public object CaptureState()
        //{
        //    return healthPoints.value;
        //}

        //public void RestoreState(object state)
        //{
        //    healthPoints.value = (float)state;
        //    if (healthPoints.value <= 0)
        //    {
        //        Die();
        //    }
        //}
        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(healthPoints.value);
        }

        public void RestoreFromJToken(JToken state)
        {
            healthPoints.value = state.ToObject<float>();
            if (healthPoints.value <= 0)
            {
                Die();
            }
        }

    }
}
