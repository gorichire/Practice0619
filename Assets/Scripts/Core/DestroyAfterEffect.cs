using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestroy = null;
        void Update()
        {
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps.IsAlive())
                {
                    return;
                }
            }

            if (targetToDestroy != null)
            {
                Destroy(targetToDestroy);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}