using UnityEngine;
using System.Collections;
using RPG.Core;        // IAction, ActionSchduler
using RPG.Movement;    // Mover
using RPG.Attributes;  // Health

namespace RPG.Control
{
    public class PlayerDodge : MonoBehaviour, IAction
    {
        [SerializeField] float dodgeDuration = 0.25f;
        [SerializeField] float cooldown = 3f;
        [SerializeField] string dodgeTrigger = "dodge";
        bool wasTargeting;

        Animator anim;
        Health hp;
        ActionSchduler scheduler;
        UnityEngine.AI.NavMeshAgent agent;

        Coroutine routine;
        bool isDodging;
        float lastDodgeTime = Mathf.NegativeInfinity;
        Vector3 dodgeDir;

        public bool IsDodging() => isDodging;

        void Awake()
        {
            if (!anim) anim = GetComponent<Animator>();
            hp = GetComponent<Health>();
            scheduler = GetComponent<ActionSchduler>();
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        // ȣ��: PlayerController ��� Shift �Է� ��
        public void TryDodge()
        {
            if (Time.time < lastDodgeTime + cooldown) return;    
            lastDodgeTime = Time.time;                           

            // ī�޶� ���� ���� ��ȯ
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 0.01f)
            {
                Transform cam = Camera.main.transform;
                Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
                dodgeDir = (fwd * input.z + right * input.x).normalized;
            }
            else dodgeDir = transform.forward;

            //  �׼� ���� 
            wasTargeting = anim.GetBool("isTargeting");
            if (wasTargeting) anim.SetBool("isTargeting", false);

            scheduler.StartAction(this);                            
            anim.ResetTrigger(dodgeTrigger);                      
            anim.SetTrigger(dodgeTrigger);                      

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(DodgeRoutine());          
        }

        IEnumerator DodgeRoutine()
        {
            isDodging = true;
            hp.SetInvulnerable(true);

            if (agent) agent.enabled = false;
            anim.applyRootMotion = true;

            yield return new WaitForSeconds(dodgeDuration);        // �ִ� ���̸�ŭ

            hp.SetInvulnerable(false);
            isDodging = false;

            anim.applyRootMotion = false;
            if (wasTargeting && GetComponent<RPG.Control.EnemyLockOn>().enabled)
            anim.SetBool("isTargeting", true);
            if (agent)
            {
                agent.enabled = true;
                agent.Warp(transform.position);                    // ��ġ ��ũ
            }

            scheduler.EndAction(this);
        }

        public void Cancel()                                   
        {
            if (routine != null) StopCoroutine(routine);
            hp.SetInvulnerable(false);
            isDodging = false;
            anim.applyRootMotion = false;
            if (agent) { agent.enabled = true; agent.Warp(transform.position); }
        }

        void OnAnimatorMove()
        {
            if (!isDodging) return;

            Vector3 delta = anim.deltaPosition; delta.y = 0f;      // ���� �̵���
            transform.position += delta;

            Quaternion targetRot = Quaternion.LookRotation(dodgeDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
        }
    }
}
