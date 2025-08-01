using RPG.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using RPG.Attributes;
using RPG.Combat;

namespace RPG.Control
{
    public class EnemyLockOn : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField] float radius = 6f;
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float maxLockDistance = 10f;

        [Header("Look At Speed")]
        [SerializeField] float turnSpeed = 10f;

        [Header("Animator")]
        [SerializeField] Animator anim;

        Transform currentTarget;
        public Transform CurrentTarget => currentTarget;

        Mover mover;
        Fighter fighter;
        PlayerCombat pCombat;

        void Awake()               
        {
            if (!anim) anim = GetComponent<Animator>();  
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            pCombat = GetComponent<PlayerCombat>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleLockOn();
            }

            if (currentTarget)
            {
                if (currentTarget == null)
                {
                    ToggleLockOn();             // 락온 해제 & 변수 정리
                    return;
                }

                if (Vector3.Distance(transform.position, currentTarget.position) > maxLockDistance)
                {
                    HandleOutOfRange();
                    if (!currentTarget) return;
                }
                if (currentTarget.TryGetComponent(out Health hp) && hp.IsDead())
                {
                    Transform next = ScanNearBy();       // 죽은 적은 제외됨

                    if (next)                            // 새 적 있으면?
                    {
                        SwitchTarget(next);              // 바로 교체 (락온 끊김 X)
                    }
                    else
                    {
                        // 새 적 없으면 원래대로 락온 해제
                        ToggleLockOn();
                    }
                    return;
                }
                RotateBody();   
            }
        }

        void ToggleLockOn()
        {
            if (currentTarget)           
            {
                currentTarget = null;
                anim.SetBool("isTargeting", false);

                mover.enemyLocked = false;  
                mover.lockRotation = false;  

                Debug.Log("<color=red>[LockOff]</color>");
                return;
            }

            // 가장 가까운 적 한 명 찾기
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayers);
            float closeDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Health hp) && hp.IsDead()) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closeDist)
                {
                    closeDist = dist;
                    currentTarget = hit.transform;
                }
            }

            if (currentTarget)
            {
                Debug.Log("<color=lime>[LockOn] → " + currentTarget.name + "</color>");
                anim.SetBool("isTargeting", true);
                mover.enemyLocked = true;
                mover.lockRotation = true;
            }
            else
                Debug.Log("<color=yellow>[LockOn FAIL]</color>");
        }

        void RotateBody()
        {
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                  Time.deltaTime * turnSpeed);
        }
        void SwitchTarget(Transform newTarget)
        {
            currentTarget = newTarget;

            if (fighter) fighter.SetTarget(newTarget.gameObject);

            if (pCombat) pCombat.ResetCombo();

            anim.SetBool("isTargeting", true);
            mover.enemyLocked = true;
            mover.lockRotation = true;
        }
        Transform ScanNearBy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayers);
            float closeDist = float.MaxValue;
            Transform best = null;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Health hp) && hp.IsDead()) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closeDist)
                {
                    closeDist = dist;
                    best = hit.transform;
                }
            }
            return best;    
        }
        void HandleOutOfRange()
        {
            Transform next = ScanNearBy();          
            if (next)
                SwitchTarget(next);                   
            else
                ToggleLockOn();                     
        }
}
}