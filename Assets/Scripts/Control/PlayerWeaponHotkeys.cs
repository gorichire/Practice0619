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
            /* 1,3,4번 : 검을 낀 상태면 ‘Sheath’ → 끝나면 새 무기 장착 */
            if (Input.GetKeyDown(KeyCode.Alpha1)) HandleRequest(unarmedWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha3)) HandleRequest(bowWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha4)) HandleRequest(iceWeapon);

            /* 2번 : 검이 없으면 ‘Draw’ */
            if (Input.GetKeyDown(KeyCode.Alpha2) &&
                !fighter.IsCurrentWeapon(swordWeapon))
            {
                fighter.StartDraw(swordWeapon);
            }
        }

        void HandleRequest(WeaponConfig nextWeapon)
        {
            if (fighter.IsCurrentWeapon(swordWeapon) && fighter.IsWeaponDrawn())
                fighter.StartSheath(nextWeapon);   // 애니메이션 → 교체
            else
                fighter.EquipWeapon(nextWeapon);   // 즉시 교체
        }
    }
}
