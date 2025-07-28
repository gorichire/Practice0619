using UnityEngine;

namespace BossFSM
{
    public class BossRigController : MonoBehaviour
    {
        [Header("Segments (앞 -> 뒤)")]
        public Transform[] segments;

        public Transform mouth;

        [Header("Wave Params")]
        public float waveSpeed = 2f;
        public float phaseSpacing = 0.4f;
        public float lateralAmplitude = 0.25f;
        public float twistAmplitudeDeg = 20f;
        public float minBaseline = 0.25f;

        Vector3[] baseLocalPos;
        Quaternion[] baseLocalRot;
        float phase;

        void Awake()
        {
            if (segments != null && segments.Length > 0)
            {
                baseLocalPos = new Vector3[segments.Length];
                baseLocalRot = new Quaternion[segments.Length];
                for (int i = 0; i < segments.Length; i++)
                {
                    baseLocalPos[i] = segments[i].localPosition;
                    baseLocalRot[i] = segments[i].localRotation;
                }
            }
        }

        public void TickSlither(float dt, float moveFactor)
        {
            if (segments == null || segments.Length == 0) return;
            moveFactor = Mathf.Clamp01(moveFactor);
            phase += dt * waveSpeed * Mathf.Max(minBaseline, moveFactor);

            for (int i = 0; i < segments.Length; i++)
            {
                float off = i * phaseSpacing;
                float wave = Mathf.Sin(phase - off);

                // 위치 좌우
                Vector3 p = baseLocalPos[i];
                p.x += wave * lateralAmplitude * moveFactor;
                segments[i].localPosition = p;

                // 회전 Yaw
                float yaw = wave * twistAmplitudeDeg * moveFactor;
                segments[i].localRotation = baseLocalRot[i] * Quaternion.Euler(0f, yaw, 0f);
            }
        }
        public void EnterChargePrepPose() { /* TODO: liftRig 등 weight 설정 */ }
        public void EnterChargeDashPose() { /* TODO */ }
        public void ResetAfterAttack() { /* TODO: weight 원상복구 */ }
    }
}
