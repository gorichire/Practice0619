using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Attributes;
using RPG.Control;

namespace RPG.Combat
{
    public class PlayerCombat : MonoBehaviour, IAction
    {
        Animator animator;
        SwordHitbox swordHitbox;
        Weapon currentWeapon;
        ActionSchduler scheduler;
        Health target;

        public GameObject attackEffectPrefab;
        public GameObject combo4EffectPrefab;
        int comboIndex = 0;
        bool canCombo = false;
        bool inputBuffered = false;
        bool wasTargeting;
        bool targetingToggledOff;
        public bool WasTargeting => wasTargeting;
        private void Start()
        {
            animator = GetComponent<Animator>();
            scheduler = GetComponent<ActionSchduler>();
            swordHitbox = GetComponentInChildren<SwordHitbox>();
            if (swordHitbox != null)
            {
                swordHitbox.SetOwner(gameObject);
            }
        }

        private void Update()
        {
            currentWeapon = GetComponent<Fighter>().GetCurrentWeapon();
        }


        public void TryComboAttack()
        {
            // 0) 준비
            currentWeapon = GetComponent<Fighter>().GetCurrentWeapon();
            if (currentWeapon == null || !currentWeapon.HasTag("Sword")) return;

            if (!IsComboAttacking())            
            {
                comboIndex = 0;                 
                canCombo = false;
                inputBuffered = false;
            }

            // 1) 첫 타 시작 여부 판단
            bool startFirstHit = (comboIndex == 0 && !canCombo);

            // 2) 타깃팅 토글: 첫 타일 때만 OFF
            wasTargeting = animator.GetBool("isTargeting");
            if (startFirstHit && wasTargeting)
            {
                animator.SetBool("isTargeting", false);
                targetingToggledOff = true;
            }

            // 3) 액션 스케줄러
            scheduler.StartAction(this);

            // 4) 로직 분기
            if (startFirstHit)
            {
                comboIndex = 1;
                animator.SetInteger("comboIndex", 1);
                animator.SetTrigger("comboAttack");
            }
            else if (comboIndex > 0)
            {
                inputBuffered = true;
            }
        }
        void OnAnimatorMove()
        {
            if (IsComboAttacking())
            {
                transform.position += animator.deltaPosition;
                transform.rotation = animator.rootRotation;
            }
        }
        public bool IsComboAttacking()
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("ComboAttack") && !animator.IsInTransition(0);
        }
        void PlayComboAnimation(int index)
        {
            animator.SetTrigger("comboAttack");
            animator.SetInteger("comboIndex", index); 
        }
        public void SpawnAttackEffect(float yRotation)
        {
            if (attackEffectPrefab == null) return;
            Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, yRotation);
            Instantiate(attackEffectPrefab, transform.position, rot);
        }

        public void SpawnCombo4Effect(float offset = 1f)
        {
            if (combo4EffectPrefab == null) return;
            // 플레이어 앞쪽(정면) 방향으로 offset만큼 이동한 위치
            Vector3 spawnPos = transform.position + transform.forward * offset + transform.up * offset;
            Quaternion rot = transform.rotation; // 그냥 플레이어 회전값만 적용

            Instantiate(combo4EffectPrefab, spawnPos, rot);
        }

        // 애니메이션 이벤트로 호출
        public void EnableHitbox()
        {
            swordHitbox?.Activate();
        }
        public void DisableHitbox()
        {
            swordHitbox?.Deactivate();
        }

        // 애니메이션 이벤트: 콤보 입력을 받을 수 있는 시점
        public void AllowCombo()
        {
            canCombo = true;
        }

        // 애니메이션 이벤트: 콤보 입력 종료
        public void EndComboWindow()
        {
            canCombo = false;

            if (inputBuffered && comboIndex < 4)
            {
                comboIndex++;
                inputBuffered = false;
                PlayComboAnimation(comboIndex);
            }
            else
            {
                comboIndex = 0;
                inputBuffered = false;

                if (wasTargeting)
                    animator.SetBool("isTargeting", true);
            }
        }
        public void ForceResetCombo()
        {
            InternalResetCore();

            if (targetingToggledOff)
            {
                var lockOn = GetComponent<RPG.Control.EnemyLockOn>();
                bool stillLocked = lockOn && lockOn.CurrentTarget != null; 

                if (stillLocked)
                    animator.SetBool("isTargeting", true); 

                targetingToggledOff = false;
            }
        }
        public void SetSwordHitbox(SwordHitbox hitbox)
        {
            swordHitbox = hitbox;
        }
        public void SetTarget(GameObject newTarget)  
        {
            target = newTarget.GetComponent<Health>();
        }
        public void ResetCombo()   
        {
            InternalResetCore();
        }

        public void Cancel()
        {
            StopAllCoroutines();
            InternalResetCore();
        }
        public void InternalResetCore()
        {
            comboIndex = 0;
            canCombo = false;
            inputBuffered = false;
            animator.ResetTrigger("comboAttack");
            animator.SetInteger("comboIndex", 0);
        }
    }

}
