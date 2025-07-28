using UnityEngine;

namespace BossFSM
{
    public class IdleState : BossState
    {
        public IdleState(BossContext c) : base(c) { }

        public override void Enter()
        {
            // Idle 포즈 (원하면 rigCtrl에서 별도 처리)
            ctx.idleBaseline = 0.3f; // 슬리더 최소 움직임 유지
        }

        public override void Tick()
        {
            if (!ctx.player) return;

            // 공격 후 강제 Idle 잠금이 있다면 아무 것도 안 함
            if (ctx.IsPostAttackLock()) return;

            // 통합 사거리 + 글로벌쿨 안이면 아무 것도 안 함
            bool inRange = ctx.InUnifiedAttackRange();
            bool globalReady = ctx.tGlobal >= ctx.globalAttackCD;

            if (ctx.IsPlayerInsideArea() && inRange && globalReady)
            {
                bool readyCharge = ctx.tCharge >= ctx.chargeCD;
                bool readySpit = ctx.tSpit >= ctx.spitCD;

                int options = (readyCharge ? 1 : 0) + (readySpit ? 1 : 0);
                if (options > 0)
                {
                    if (readyCharge && readySpit)
                    {
                        int pick = Random.Range(0, 2); // 0 or 1
                        if (pick == 0) { ctx.ChangeState(ctx.chargeState); return; }
                        else { ctx.ChangeState(ctx.spitState); return; }
                    }
                    else if (readyCharge) { ctx.ChangeState(ctx.chargeState); return; }
                    else { ctx.ChangeState(ctx.spitState); return; }
                }
            }

            // 공격 실패 → Chase 전환 조건 (히스테리시스)
            float dist = (ctx.player.position - ctx.transform.position).sqrMagnitude;
            float leaveChaseDist = (ctx.stopChaseDistance + 1.5f) * (ctx.stopChaseDistance + 1.5f);
            if (dist > leaveChaseDist && ctx.IsPlayerInsideArea())
            {
                ctx.ChangeState(ctx.chaseState);
            }
        }

    }
}
