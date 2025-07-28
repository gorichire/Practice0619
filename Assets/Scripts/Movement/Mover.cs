using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using Unity.VisualScripting;
using RPG.Attributes;
using Newtonsoft.Json.Linq;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour , IAction , ISaveable , IJsonSaveable
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


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            Health = GetComponent<Health>();
        }
        private void Start()
        {

        }
        void Update()
        {
            navMeshAgent.enabled = !Health.IsDead();

            Vector3 velocityToUse = Vector3.zero;

            if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > 0.01f)
            {
                velocityToUse = navMeshAgent.velocity;
            }
            else if (isPlayer && isKeyboardMoving)
            {
                velocityToUse = transform.forward * maxSpeed;
            }
            else
            {
                velocityToUse = Vector3.zero;
            }
            UpdateAnimator(velocityToUse);

            // 
            if (isPlayer)
            {
                if (!Input.anyKey || Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
                {
                    isKeyboardMoving = false;
                }
            }
        }
        public void MoveWithDirection(Vector3 direction)
        {

            isKeyboardMoving = direction.sqrMagnitude > 0.01f;

            if (!isKeyboardMoving)
            {
                lastFrameDelta = Vector3.zero;
                return;
            }

            navMeshAgent.ResetPath(); // 마우스 목적지 제거

            // 실제 이동 처리
            Vector3 moveDelta = direction.normalized * maxSpeed * Time.deltaTime;
            navMeshAgent.Move(moveDelta);

            // 회전 처리
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        public void StartMoveAction(Vector3 destination , float speedFraction)
        {
            GetComponent<ActionSchduler>().StartAction(this);
            navMeshAgent.ResetPath(); // 이전 경로 제거
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

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3)state;
            navMeshAgent.enabled = false;
            transform.position = position.ToVector();
            navMeshAgent.enabled = true;
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(transform.position);
        }

        public void RestoreFromJToken(JToken state)
        {
            navMeshAgent.enabled = false;
            transform.position = state.ToObject<Vector3>();
            navMeshAgent.enabled = true;
            GetComponent<ActionSchduler>().CancelCurrentAction();

        }


    }
}
