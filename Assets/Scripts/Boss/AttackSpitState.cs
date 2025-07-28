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
            // ��ٿ� �ʱ�ȭ
            ctx.ResetGlobal();
            ctx.ResetSpit();

            // ���� ����
            if (ctx.agent) { ctx.agent.isStopped = true; ctx.agent.ResetPath(); }

            // ����(�Ӹ� ����)
            ctx.FacePlayerFlat();

            routine = ctx.StartCoroutine(Seq());
        }
        IEnumerator Seq()
        {
            float aimTime = 0.35f;
            yield return new WaitForSeconds(aimTime);

            SpawnSpitAoE();   // �� �̰͸�

            yield return new WaitForSeconds(0.4f);
            ctx.rigCtrl?.ResetAfterAttack();
            if (ctx.agent) ctx.agent.isStopped = false;
            ctx.StartPostAttackIdleLock();
            ctx.ChangeState(ctx.idleState);
        }
        void SpawnSpitAoE()
        {
            if (!ctx.spitAoEPrefab || !ctx.player) return;

            // �÷��̾� ��ġ (���� ���̸� ��� Ŭ����)
            Vector3 target = ctx.GetClampedPlayerPoint();
            // ���� ���� Raycast
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
            // ���� �ڵ� ����(��: 5��)
            Object.Destroy(aoe, 5f);
        }
        public override void Exit()
        {
            if (routine != null) ctx.StopCoroutine(routine);
            if (ctx.agent) ctx.agent.isStopped = false;
        }
    }
}
