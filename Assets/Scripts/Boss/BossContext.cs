using UnityEngine;
using UnityEngine.AI;

namespace BossFSM
{
    public class BossContext : MonoBehaviour
    {
        [Header("Refs")]
        public NavMeshAgent agent;
        public Transform player;
        public BossRigController rigCtrl;

        [Header("Movement Area (원형)")]
        public float areaRadius = 30f;
        [Tooltip("에이전트가 경계 바로 위로 가는 것을 방지할 내부 여유")]
        public float areaEdgePadding = 0.5f;

        [Header("Detection")]
        public float detectionRadius = 25f;
        public float stopChaseDistance = 8f;

        [Header("Post Attack Idle")]
        public float postAttackIdleDuration = 5f;   // 공격 끝난 뒤 강제 Idle 시간
        [HideInInspector] public float postAttackIdleTimer; // >0 이면 잠금
        public bool IsPostAttackLock() => postAttackIdleTimer > 0f;

        [Header("Charge Attack")]
        public float minChargeDist = 8f;
        public float maxChargeDist = 18f;
        public float chargeCD = 6f;
        public float globalAttackCD = 2f;

        [HideInInspector] public float tCharge;
        [HideInInspector] public float tGlobal;

        [HideInInspector] public AttackChargeState chargeState;
        [Header("Charge FX")]
        public GameObject chargeStartEffectPrefab;


        [Header("Spit Attack")]
        public float spitMinDist = 10f;      // 이 거리 이상일 때만
        public float spitCD = 5f;

        [HideInInspector] public float tSpit;

        [Header("Unified Attack Range")]
        public float attackRangeMin = 8f;
        public float attackRangeMax = 18f;

        [Header("Spit Prefabs / Refs")]
        public GameObject spitAoEPrefab;

        [HideInInspector] public AttackSpitState spitState;
        [Header("Ground")]
        public LayerMask groundMask;

        [Header("Damage Values")]
        public float chargeDamage = 30f;
        public float chargeHitRadius = 2f;


        [HideInInspector] public IdleState idleState;
        [HideInInspector] public ChaseState chaseState;
        BossState current;
        [HideInInspector] public float idleBaseline = 0.3f;


        public float moveSpeed = 4f; // NavMeshAgent.speed와 동일하게 맞추

        // 출생 중심
        public Vector3 SpawnCenter { get; private set; }

        void Awake()
        {
            SpawnCenter = transform.position;
            idleState = new IdleState(this);
            chaseState = new ChaseState(this);
            chargeState = new AttackChargeState(this);
            spitState = new AttackSpitState(this);
        }
        void Start()
        {
            ChangeState(idleState);
        }

        void Update()
        {
            current?.Tick();

            // 슬리더 업데이트 (moveFactor 계산 + baseline)
            if (rigCtrl)
            {
                float moveFactor = 0f;
                if (agent && agent.enabled)
                    moveFactor = Mathf.Clamp01(agent.velocity.magnitude / moveSpeed);
                moveFactor = Mathf.Max(moveFactor, idleBaseline);
                rigCtrl.TickSlither(Time.deltaTime, moveFactor);
            }
            tCharge += Time.deltaTime;
            tGlobal += Time.deltaTime;
            tSpit += Time.deltaTime;

            postAttackIdleTimer -= Time.deltaTime;
            bool lockJustEnded = postAttackIdleTimer > 0f && (postAttackIdleTimer - Time.deltaTime) <= 0f;
            postAttackIdleTimer -= Time.deltaTime;
            if (postAttackIdleTimer < 0f) postAttackIdleTimer = 0f;

            if (lockJustEnded && current == idleState && player && IsPlayerInsideArea())
            {
                ChangeState(chaseState);
            }
        }


        public Vector3 ClampPointInArea(Vector3 worldPoint)
        {
            Vector3 dir = worldPoint - SpawnCenter;
            dir.y = 0f;
            float dist = dir.magnitude;
            float max = areaRadius - areaEdgePadding;
            if (dist > max)
            {
                if (dist > 0.0001f) dir = dir / dist; 
                worldPoint = SpawnCenter + dir * max;
            }
            return worldPoint;
        }



        /// <summary> 수동 이동(Charge) 후 경계 밖으로 넘어가면 강제 안쪽으로 Clamp </summary>
        public void ClampSelfInsideArea()
        {
            Vector3 pos = transform.position;
            Vector3 dir = pos - SpawnCenter;
            dir.y = 0f;
            float dist = dir.magnitude;
            float max = areaRadius - areaEdgePadding;
            if (dist > max)
            {
                if (dist > 0.0001f) dir = dir / dist;
                pos = SpawnCenter + dir * max;
                transform.position = pos;
            }
        }
        public bool IsPlayerInsideArea()
        {
            if (!player) return false;
            Vector3 flat = player.position; flat.y = SpawnCenter.y;
            return (flat - SpawnCenter).sqrMagnitude <= areaRadius * areaRadius;
        }

        public Vector3 GetClampedPlayerPoint()
        {
            if (!player) return transform.position;
            return ClampPointInArea(player.position);
        }

        // Chase/Charge 전용: 플레이어가 밖이면 경계점(=고정 목표)만 반환
        public Vector3 GetBoundaryPointToPlayer()
        {
            if (!player) return transform.position;
            Vector3 dir = player.position - SpawnCenter;
            dir.y = 0f;
            float dist = dir.magnitude;
            float max = areaRadius - areaEdgePadding;
            if (dist <= max) return player.position; // 안쪽
            if (dist > 0.0001f) dir /= dist;
            return SpawnCenter + dir * max;
        }
        public void ChangeState(BossState next)
        {
            if (next == null || next == current) return;
            current?.Exit();
            current = next;
            current.Enter();
        }

        public bool ChargeReady() => tCharge >= chargeCD && tGlobal >= globalAttackCD;
        public void ResetGlobal() { tGlobal = 0f; }
        public void FacePlayerFlat()
        {
            if (!player) return;
            Vector3 d = player.position - transform.position;
            d.y = 0;
            if (d.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 0.5f);
        }

        public bool SpitReady() => tSpit >= spitCD && tGlobal >= globalAttackCD;
        public void ResetSpit() { tSpit = 0f; }
        public void StartPostAttackIdleLock()
        {
            postAttackIdleTimer = postAttackIdleDuration;
        }
        public bool InUnifiedAttackRange()
        {
            if (!player) return false;
            float d2 = (player.position - transform.position).sqrMagnitude;
            return d2 >= attackRangeMin * attackRangeMin &&
                   d2 <= attackRangeMax * attackRangeMax;
        }
        // 기즈모
        void OnDrawGizmosSelected()
        {
            // 에디터 상에서도 중심 유지 표시
            Vector3 center = Application.isPlaying ? SpawnCenter : transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, areaRadius);
            Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
            Gizmos.DrawSphere(center, 0.3f); // 중심점
        }
    }
}
