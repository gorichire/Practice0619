using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Attributes;

namespace RPG.Combat
{
    public class PlayerCombat : MonoBehaviour, IAction
    {
        Animator animator;
        SwordHitbox swordHitbox;
        Weapon currentWeapon;
        ActionSchduler scheduler;

        public GameObject attackEffectPrefab;
        public GameObject combo4EffectPrefab;
        int comboIndex = 0;
        bool canCombo = false;
        bool inputBuffered = false;
        bool wasTargeting;
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
            wasTargeting = GetComponent<Animator>().GetBool("isTargeting");
            if (wasTargeting) GetComponent<Animator>().SetBool("isTargeting", false);
            scheduler.StartAction(this);

            currentWeapon = GetComponent<Fighter>().GetCurrentWeapon();

            if (currentWeapon != null && currentWeapon.HasTag("Sword"))
            {
                if (canCombo)
                {
                    inputBuffered = true;
                }
                else if (comboIndex == 0)
                {
                    comboIndex = 1;
                    animator.SetInteger("comboIndex", comboIndex);
                    animator.SetTrigger("comboAttack");
                }
            }
        }
        void OnAnimatorMove()
        {
            if (IsComboAttacking())
            {
                transform.position += animator.deltaPosition;
                transform.rotation = animator.rootRotation; // ȸ���� Root Motion�� ���߰� ������
            }
        }
        public bool IsComboAttacking()
        {
            // �ִϸ����Ϳ��� �޺� ���ݿ� "ComboAttack" �±׸� �ݵ�� ������ ��!
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("ComboAttack") && !animator.IsInTransition(0);
        }
        void PlayComboAnimation(int index)
        {
            animator.SetTrigger("comboAttack");
            animator.SetInteger("comboIndex", index); // Blend Tree or ���� �б��
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
            // �÷��̾� ����(����) �������� offset��ŭ �̵��� ��ġ
            Vector3 spawnPos = transform.position + transform.forward * offset + transform.up * offset;
            Quaternion rot = transform.rotation; // �׳� �÷��̾� ȸ������ ����

            Instantiate(combo4EffectPrefab, spawnPos, rot);
        }

        // �ִϸ��̼� �̺�Ʈ�� ȣ��
        public void EnableHitbox()
        {
            swordHitbox?.Activate();
        }
        public void DisableHitbox()
        {
            swordHitbox?.Deactivate();
        }

        // �ִϸ��̼� �̺�Ʈ: �޺� �Է��� ���� �� �ִ� ����
        public void AllowCombo()
        {
            canCombo = true;
        }

        // �ִϸ��̼� �̺�Ʈ: �޺� �Է� ����
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
        public void SetSwordHitbox(SwordHitbox hitbox)
        {
            swordHitbox = hitbox;
        }
        public void SetTarget(GameObject newTarget)  
        {
            //target = newTarget.GetComponent<Health>();
        }
        public void ResetCombo()   
        {
            comboIndex = 0;
            canCombo = false;
            inputBuffered = false;
            animator.ResetTrigger("comboAttack");
            animator.SetInteger("comboIndex", 0);
        }

        public void Cancel()
        {
            StopAllCoroutines();
            comboIndex = 0;             
            canCombo = false;            
            inputBuffered = false;       
            animator.ResetTrigger("comboAttack");
            animator.SetInteger("comboIndex", 0);
        }
}

}
