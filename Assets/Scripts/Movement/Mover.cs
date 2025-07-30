using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using Unity.VisualScripting;
using RPG.Attributes;
using Newtonsoft.Json.Linq;
using UnityEngine.SocialPlatforms;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour , IAction /* , ISaveable*/ , IJsonSaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        [SerializeField] float maxNavPathLength = 40f;
        [SerializeField] private bool isPlayer = false;

        NavMeshAgent navMeshAgent;
        Health Health;

        Vector3 lastFrameDelta = Vector3.zero;
        bool isKeyboardMoving = false;
        float currentSpeed = 0f;
        float speedSmoothVelocity = 0f;

        // Ÿ����
        [HideInInspector] public bool lockRotation = false;
        [SerializeField] float lockOnSpeed = 2.0f;
        [HideInInspector] public bool enemyLocked = false;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            Health = GetComponent<Health>();
        }
        void Update()
        {
            navMeshAgent.enabled = !Health.IsDead();

            navMeshAgent.updateRotation = !lockRotation;

            if (enemyLocked)
            {
                UpdateLockOnMove();
                return;
            }

            UpdateFreeMove();
        }
        void UpdateFreeMove()
        {
            Vector3 velocityToUse = Vector3.zero;

            if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > 0.01f)
                velocityToUse = navMeshAgent.velocity;
            else if (isPlayer && isKeyboardMoving)
                velocityToUse = transform.forward * maxSpeed;

            UpdateAnimator(velocityToUse);

            if (isPlayer && (!Input.anyKey ||
                (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)))
                isKeyboardMoving = false;
        }

        public void MoveWithDirection(Vector3 direction)
        {
            if (enemyLocked) return;
            isKeyboardMoving = direction.sqrMagnitude > 0.01f;

            if (!isKeyboardMoving)
            {
                lastFrameDelta = Vector3.zero;
                return;
            }

            navMeshAgent.ResetPath(); // ���콺 ������ ����

            // ���� �̵� ó��
            Vector3 moveDelta = direction.normalized * maxSpeed * Time.deltaTime;
            navMeshAgent.Move(moveDelta);

            // ȸ�� ó��
            if (!lockRotation)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      targetRotation,
                                                      Time.deltaTime * 10f);
            }
        }

        public void StartMoveAction(Vector3 destination , float speedFraction)
        {
            GetComponent<ActionSchduler>().StartAction(this);
            navMeshAgent.ResetPath(); // ���� ��� ����
            MoveTo(destination , speedFraction);
        }
        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > maxNavPathLength) return false;

            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel() 
        {
            navMeshAgent.isStopped = true;
        }


        private void UpdateAnimator(Vector3 velocity)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.magnitude;

            float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
            currentSpeed = Mathf.SmoothDamp(currentSpeed, normalizedSpeed, ref speedSmoothVelocity, 0.1f);

            float finalSpeed = Mathf.Lerp(0f, 3.29f, currentSpeed);
            GetComponent<Animator>().SetFloat("forwardSpeed", finalSpeed);

            GetComponent<Animator>().SetFloat("strafeX", localVelocity.x / maxSpeed);
            GetComponent<Animator>().SetFloat("strafeZ", localVelocity.z / maxSpeed);
        }
        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length < 2) return total;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total;
        }
        void UpdateLockOnMove()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"),
                                        0,
                                        Input.GetAxisRaw("Vertical"));

            bool hasInput = input.sqrMagnitude > 0.01f;

            Transform cam = Camera.main.transform;
            Vector3 camF = cam.forward; camF.y = 0; camF.Normalize();
            Vector3 camR = cam.right; camR.y = 0; camR.Normalize();
            Vector3 moveDir = (camR * input.x + camF * input.z).normalized;
            navMeshAgent.isStopped = !hasInput;
            navMeshAgent.Move(moveDir * lockOnSpeed * Time.deltaTime);

            GetComponent<Animator>().SetFloat("strafeX", input.x, 0.15f, Time.deltaTime);   
            GetComponent<Animator>().SetFloat("strafeZ", input.z, 0.15f, Time.deltaTime);   
            GetComponent<Animator>().SetFloat("forwardSpeed", 0);     
        }

        //public object CaptureState()
        //{
        //    return new SerializableVector3(transform.position);
        //}

        //public void RestoreState(object state)
        //{
        //    SerializableVector3 position = (SerializableVector3)state;
        //    navMeshAgent.enabled = false;
        //    transform.position = position.ToVector();
        //    navMeshAgent.enabled = true;
        //}

        public JToken CaptureAsJToken()
        {
            return transform.position.ToToken();
        }

        public void RestoreFromJToken(JToken state)
        {
            navMeshAgent.enabled = false;
            transform.position = state.ToVector3();
            navMeshAgent.enabled = true;
            GetComponent<ActionSchduler>().CancelCurrentAction();
        }


    }
}
