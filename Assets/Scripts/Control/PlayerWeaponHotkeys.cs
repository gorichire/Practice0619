using UnityEngine;
using RPG.Combat;

namespace RPG.Control
{
    public class PlayerWeaponHotkeys : MonoBehaviour
    {
        [Header("Weapon Configs")]
        [SerializeField] WeaponConfig unarmedWeapon;   // 1
        [SerializeField] WeaponConfig swordWeapon;     // 2
        [SerializeField] WeaponConfig bowWeapon;       // 3
        [SerializeField] WeaponConfig iceWeapon;       // 4

        Fighter fighter;

        void Start()
        {
            fighter = GetComponent<Fighter>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Request(unarmedWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Request(swordWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Request(bowWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Request(iceWeapon);
        }

        void Request(WeaponConfig desired)
        {
            if (fighter.IsWeaponChanging()) return;           


            if (fighter.IsCurrentWeapon(desired) && fighter.IsWeaponDrawn())
            {
                fighter.StartSheath(unarmedWeapon);
                return;
            }

            if (fighter.IsWeaponDrawn())
                fighter.StartSheath(desired);   
            else
                fighter.StartDraw(desired); ;
        }
    }
}
