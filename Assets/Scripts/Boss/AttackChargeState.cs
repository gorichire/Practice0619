using UnityEngine;
using System.Collections;

namespace BossFSM
{
    public class AttackChargeState : BossState
    {
        Coroutine routine;

        bool chargeHitApplied;
        public AttackChargeState(BossContext c) : base(c) { }

        public override void Enter()
        {
            ctx.ResetGlobal();
            ctx.tCharge = 0f;
            chargeHitApplied = false;

            if (ctx.agent) ctx.agent.enabled = false;

            // �浹 �� �ϰ� ���� ����
            ctx.FacePlayerFlat();
            ctx.rigCtrl?.EnterChargePrepPose(); // ������ ����

            routine = ctx.StartCoroutine(Seq());
        }

        IEnumerator Seq()
        {
            GameObject prepFx = null;
            if (ctx.chargePrepVFX)
                prepFx = Object.Instantiate(ctx.chargePrepVFX,
                                            ctx.rigCtrl ? ctx.rigCtrl.mouth.position : ctx.transform.position,
                                            Quaternion.identity,
                                            ctx.transform);

            float Lookt = 0f;
            while (Lookt < ctx.chargePrepTime)
            {
                ctx.FacePlayerFlat();
                Lookt += Time.deltaTime;
                yield return null;
            }

            SpawnChargeStartFX();
            ctx.rigCtrl?.EnterChargeDashPose();


            Vector3 dir = (ctx.GetBoundaryPointToPlayer() - ctx.transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude < 0.01f) dir = ctx.transform.forward;
            else dir.Normalize();

            float dashTime = 1.0f;
            float speed = 15f;
            float t = 0f;

            while (t < dashTime)
            {
                ctx.transform.position += dir * speed * Time.deltaTime;
                ctx.ClampSelfInsideArea();

                if (!chargeHitApplied && ctx.player)
                {
                    Vector3 dp = ctx.player.position - ctx.transform.position;
                    dp.y = 0f;
                    if (dp.sqrMagnitude <= ctx.chargeHitRadius * ctx.chargeHitRadius)
                    {
                        var h = ctx.player.GetComponent<RPG.Attributes.Health>();
                        if (h)
                        {
                            h.TakeDamage(ctx.gameObject, ctx.chargeDamage);
                        }
                        chargeHitApplied = true;
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }
            ctx.rigCtrl?.ResetAfterAttack();
            ctx.StartPostAttackIdleLock();
            ctx.ChangeState(ctx.idleState);
        }
        void SpawnChargeStartFX()
        {
            var prefab = ctx.chargeStartEffectPrefab;
            if (!prefab) return;

            // ���� ����(�Ӹ��� Mouth�� ������ �װɷ�)
            Transform parent = (ctx.rigCtrl != null && ctx.rigCtrl.mouth != null)
                ? ctx.rigCtrl.mouth
                : ctx.transform;

            // ���� ���� ��ġ/ȸ�� ���
            Vector3 worldPos = parent.position + parent.forward * 1.0f; // ���� ������
            Quaternion worldRot = Quaternion.LookRotation(parent.forward, Vector3.up);

            // �θ� ������ �Բ� ���� (���� parent ����)
            var fx = Object.Instantiate(prefab, worldPos, worldRot, parent);

            // ���÷� �̼� ���� ����
            // fx.transform.localPosition += new Vector3(0, 0, 0);
            // fx.transform.localRotation = Quaternion.identity;
        }


        public override void Exit()
        {
            if (routine != null) ctx.StopCoroutine(routine);
            if (ctx.agent && !ctx.agent.enabled) ctx.agent.enabled = true;
        }

    }
}
