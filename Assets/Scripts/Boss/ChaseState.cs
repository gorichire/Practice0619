using UnityEngine;

namespace BossFSM
{
    public class ChaseState : BossState
    {
        public ChaseState(BossContext c) : base(c) { }

        public override void Enter()
        {
            if (ctx.agent)
            {
                ctx.agent.isStopped = false;
                ctx.agent.speed = ctx.moveSpeed;
            }
            ctx.idleBaseline = 0.6f; // �����ϼ��� ���� �� ũ�� (��������)
        }

        public override void Tick()
        {
            if (!ctx.player || !ctx.agent) return;

            // �÷��̾ ���� ���̸� ������ ���� Idle��
            if (!ctx.IsPlayerInsideArea())
            {
                Vector3 boundary = ctx.GetBoundaryPointToPlayer();
                float d = (ctx.transform.position - boundary).sqrMagnitude;
                if (d > 1f) ctx.agent.SetDestination(boundary);
                else
                {
                    ctx.agent.ResetPath();
                    ctx.ChangeState(ctx.idleState);
                }
                return;
            }

            // ���̸� ����
            Vector3 target = ctx.GetClampedPlayerPoint();

            if (ctx.InUnifiedAttackRange() && ctx.tGlobal >= ctx.globalAttackCD)
            {
                bool readyCharge = ctx.tCharge >= ctx.chargeCD;
                bool readySpit = ctx.tSpit >= ctx.spitCD;
                int options = (readyCharge ? 1 : 0) + (readySpit ? 1 : 0);
                if (options > 0)
                {
                    if (readyCharge && readySpit)
                    {
                        int pick = Random.Range(0, 2);
                        if (pick == 0) { ctx.ChangeState(ctx.chargeState); return; }
                        else { ctx.ChangeState(ctx.spitState); return; }
                    }
                    else if (readyCharge) { ctx.ChangeState(ctx.chargeState); return; }
                    else { ctx.ChangeState(ctx.spitState); return; }
                }
            }
            ctx.agent.SetDestination(target);

        }

        public override void Exit()
        {
            if (ctx.agent)
            {
                ctx.agent.ResetPath();
                ctx.agent.isStopped = true;  // ��������� ����
            }
        }
    }
}
