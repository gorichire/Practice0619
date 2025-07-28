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

            // 충돌 안 하고 방향 고정
            ctx.FacePlayerFlat();
            ctx.rigCtrl?.EnterChargePrepPose(); // 없으면 생략

            routine = ctx.StartCoroutine(Seq());
        }

        IEnumerator Seq()
        {
            float prep = 0.4f;
            yield return new WaitForSeconds(prep);

            // === Dash 시작 직전 이펙트 1회 ===
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

                // === 돌진 히트 1회 판정 ===
                if (!chargeHitApplied && ctx.player)
                {
                    Vector3 dp = ctx.player.position - ctx.transform.position;
                    dp.y = 0f;
                    if (dp.sqrMagnitude <= ctx.chargeHitRadius * ctx.chargeHitRadius)
                    {
                        var h = ctx.player.GetComponent<RPG.Attributes.Health>();
                        if (h)
                        {
                            h.TakeDamage(ctx.gameObject, ctx.chargeDamage); // Health.TakeDamage 사용
                        }
                        chargeHitApplied = true;
                    }
                }
                // ==========================

                t += Time.deltaTime;
                yield return null;
            }
            ctx.rigCtrl?.ResetAfterAttack();   // (선택) 리그 원복
            ctx.StartPostAttackIdleLock();     // Idle 강제 시간 시작
            ctx.ChangeState(ctx.idleState);
        }
        void SpawnChargeStartFX()
        {
            var prefab = ctx.chargeStartEffectPrefab;
            if (!prefab) return;

            // 붙일 기준(머리나 Mouth가 있으면 그걸로)
            Transform parent = (ctx.rigCtrl != null && ctx.rigCtrl.mouth != null)
                ? ctx.rigCtrl.mouth
                : ctx.transform;

            // 월드 기준 위치/회전 계산
            Vector3 worldPos = parent.position + parent.forward * 1.0f; // 앞쪽 오프셋
            Quaternion worldRot = Quaternion.LookRotation(parent.forward, Vector3.up);

            // 부모 지정과 함께 생성 (이후 parent 따라감)
            var fx = Object.Instantiate(prefab, worldPos, worldRot, parent);

            // 로컬로 미세 조정 가능
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
