using UnityEngine;

namespace BossFSM
{
    public class IdleState : BossState
    {
        public IdleState(BossContext c) : base(c) { }

        public override void Enter()
        {
            // Idle ���� (���ϸ� rigCtrl���� ���� ó��)
            ctx.idleBaseline = 0.3f; // ������ �ּ� ������ ����
        }

        public override void Tick()
        {
            if (!ctx.player) return;

            // ���� �� ���� Idle ����� �ִٸ� �ƹ� �͵� �� ��
            if (ctx.IsPostAttackLock()) return;

            // ���� ��Ÿ� + �۷ι��� ���̸� �ƹ� �͵� �� ��
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

            // ���� ���� �� Chase ��ȯ ���� (�����׸��ý�)
            float dist = (ctx.player.position - ctx.transform.position).sqrMagnitude;
            float leaveChaseDist = (ctx.stopChaseDistance + 1.5f) * (ctx.stopChaseDistance + 1.5f);
            if (dist > leaveChaseDist && ctx.IsPlayerInsideArea())
            {
                ctx.ChangeState(ctx.chaseState);
            }
        }

    }
}
