// RPG.Core.DestroyAfterEffect
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestroy = null;

        void Update()
        {
            if (RPG.Control.PlayerController.isCutscenePlaying)
            {
                KillNow();
                return;
            }

            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps.IsAlive()) return;
            }
            KillNow();
        }

        public void KillNow()
        {
            if (targetToDestroy != null) Destroy(targetToDestroy);
            else Destroy(gameObject);
        }
    }
}
