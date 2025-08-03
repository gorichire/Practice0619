using UnityEngine;
using System.Collections;
using Cinemachine;

namespace RPG.Combat
{
    public class ImpactFX : MonoBehaviour
    {
        public static ImpactFX I { get; private set; }

        [Header("Hit-Stop")]
        [SerializeField, Range(0f, 1f)] float minTimeScale = 0f;

        [Header("Camera Shake")]
        [SerializeField] CinemachineImpulseSource impulse;

        Coroutine current;
        float defaultScale = 1f;

        void Awake()
        {
            if (I == null) I = this;
            else Destroy(gameObject);
        }

        public void HitStop(float duration, float timeScale = 0f,
                            float camAmplitude = 1f, float camFrequency = 1f)
        {
            if (current != null)
            {
                StopCoroutine(current);
                Time.timeScale = defaultScale;
            }
            if (impulse)
            {
                var sig = impulse.m_DefaultVelocity;
                sig *= camAmplitude;
                impulse.m_DefaultVelocity = sig;
                impulse.GenerateImpulse(camFrequency);
            }
            current = StartCoroutine(HitStopRoutine(duration,
                       Mathf.Clamp(timeScale, minTimeScale, 1f)));
        }

        IEnumerator HitStopRoutine(float dur, float scale)
        {
            defaultScale = Time.timeScale;
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(dur);
            Time.timeScale = defaultScale;
            current = null;
        }
    }
}