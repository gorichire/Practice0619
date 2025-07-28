using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] UnityEvent onHit;
        [SerializeField] string weaponTag;

        public void OnHit()
        {
            onHit.Invoke();
        }
        public string GetWeaponTag()
        {
            return weaponTag;
        }

        public bool HasTag(string tag)
        {
            return weaponTag == tag;
        }
    }

}