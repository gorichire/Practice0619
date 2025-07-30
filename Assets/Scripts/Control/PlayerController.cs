using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using RPG.Attributes;
using UnityEngine.EventSystems;
using UnityEngine.AI;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        Health health;
        private PlayerCombat playerCombat;
        PlayerDodge dodge;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;

        private void Awake()
        {
            health = GetComponent<Health>();
            playerCombat = GetComponent<PlayerCombat>();
            dodge = GetComponent<PlayerDodge>();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dodge.TryDodge();
                return;
            }
            if (dodge.IsDodging()) return;

            if (InteractWithUI()) return;
            if (health.IsDead())
            {
                SetCursor(CursorType.None);
                return;
            }
            var playerCombat = GetComponent<PlayerCombat>();

            if (Input.GetMouseButtonDown(0))
            {
                ComboButton(playerCombat);
                return;
            }
            if (playerCombat != null && playerCombat.IsComboAttacking())
            {
                return;
            }

            if (HandleKeyboardMovement()) return;

            if (InteractWithComponent()) return;
            if (InteractWithMovement()) { return; }

            SetCursor(CursorType.None);
        }

        private void ComboButton(PlayerCombat playerCombat)
        {
            var lockOn = GetComponent<EnemyLockOn>();

            if (lockOn && lockOn.CurrentTarget)
            {
                playerCombat?.TryComboAttack();
            }
            else
            {
                playerCombat?.TryComboAttack();
            }
        }

        private bool HandleKeyboardMovement()
        {

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(h, 0, v);

            if (inputDir.magnitude < 0.1f) return false;

            GetComponent<Fighter>().Cancel();

            // 카메라 기준 방향 회전 적용
            Vector3 moveDir = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * inputDir;

            GetComponent<Mover>().MoveWithDirection(moveDir);
            return true;
        }

        private bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }
        RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }

        private bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);

            if (hasHit)
            {
                if (!GetComponent<Mover>().CanMoveTo(target)) return false;

                if (Input.GetMouseButton(1))
                {
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }
        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();

            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit) return false;

            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;

            target = navMeshHit.position;

            return true;
        }
        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }
        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
