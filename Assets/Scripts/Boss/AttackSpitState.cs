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
            ctx.ResetGlobal();
            ctx.ResetSpit();

            if (ctx.agent) { ctx.agent.isStopped = true; ctx.agent.ResetPath(); }

            ctx.FacePlayerFlat();

            routine = ctx.StartCoroutine(Seq());
        }
        IEnumerator Seq()
        {
            GameObject prepFx = null;
            if (ctx.spitPrepVFX)
                prepFx = Object.Instantiate(ctx.spitPrepVFX,
                                            ctx.rigCtrl ? ctx.rigCtrl.mouth.position : ctx.transform.position,
                                            Quaternion.identity,
                                            ctx.transform);
            float t = 0f;
            while (t < ctx.spitPrepTime)
            {
                ctx.FacePlayerFlat();
                t += Time.deltaTime;
                yield return null;
            }
            GameObject prepFx2 = null;
            if (ctx.spitPrepVFX2)
            {
                Transform m = ctx.rigCtrl ? ctx.rigCtrl.mouth : ctx.transform;
                Quaternion rot = m.rotation * Quaternion.Euler(0f, 180f, 0f);
                prepFx2 = Object.Instantiate(ctx.spitPrepVFX2,
                                             m.position,
                                             rot,
                                             m);
            }
            SpawnSpitAoE();

            yield return new WaitForSeconds(0.4f);
            ctx.rigCtrl?.ResetAfterAttack();
            if (ctx.agent) ctx.agent.isStopped = false;
            ctx.StartPostAttackIdleLock();
            ctx.ChangeState(ctx.idleState);
        }
        void SpawnSpitAoE()
        {
            if (!ctx.spitAoEPrefab || !ctx.player) return;

            Vector3 target = ctx.GetClampedPlayerPoint();
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

            if (aoe.TryGetComponent(out SpitAoE spit))
            {
                spit.tickDamage = 10f;        
                spit.instigator = ctx.gameObject;
            }
            Object.Destroy(aoe, 7f);
        }
        public override void Exit()
        {
            if (routine != null) ctx.StopCoroutine(routine);
            if (ctx.agent) ctx.agent.isStopped = false;
        }
    }
}
