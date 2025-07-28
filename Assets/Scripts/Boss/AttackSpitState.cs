using UnityEngine;
using System.Collections;

namespace BossFSM
{
    public class AttackSpitState : BossState
    {
        Coroutine routine;
        public AttackSpitState(BossContext c) : base(c) { }

        public override void Enter()
        {
            // 쿨다운 초기화
            ctx.ResetGlobal();
            ctx.ResetSpit();

            // 추적 중지
            if (ctx.agent) { ctx.agent.isStopped = true; ctx.agent.ResetPath(); }

            // 조준(머리 방향)
            ctx.FacePlayerFlat();

            routine = ctx.StartCoroutine(Seq());
        }
        IEnumerator Seq()
        {
            float aimTime = 0.35f;
            yield return new WaitForSeconds(aimTime);

            SpawnSpitAoE();   // ← 이것만

            yield return new WaitForSeconds(0.4f);
            ctx.rigCtrl?.ResetAfterAttack();
            if (ctx.agent) ctx.agent.isStopped = false;
            ctx.StartPostAttackIdleLock();
            ctx.ChangeState(ctx.idleState);
        }
        void SpawnSpitAoE()
        {
            if (!ctx.spitAoEPrefab || !ctx.player) return;

            // 플레이어 위치 (영역 밖이면 경계 클램프)
            Vector3 target = ctx.GetClampedPlayerPoint();
            // 지면 높이 Raycast
            if (Physics.Raycast(target + Vector3.up * 5f,
                                Vector3.down,
                                out RaycastHit hit,
                                50f,
                                ctx.groundMask,
                                QueryTriggerInteraction.Ignore))
            {
                target = hit.point;
            }
            else
                target.y = ctx.transform.position.y;

            var aoe = Object.Instantiate(ctx.spitAoEPrefab, target, Quaternion.identity);
            // 수명 자동 정리(예: 5초)
            Object.Destroy(aoe, 5f);
        }
        public override void Exit()
        {
            if (routine != null) ctx.StopCoroutine(routine);
            if (ctx.agent) ctx.agent.isStopped = false;
        }
    }
}
