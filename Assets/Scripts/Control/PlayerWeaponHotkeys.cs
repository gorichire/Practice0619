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
            /* 1,3,4�� : ���� �� ���¸� ��Sheath�� �� ������ �� ���� ���� */
            if (Input.GetKeyDown(KeyCode.Alpha1)) HandleRequest(unarmedWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha3)) HandleRequest(bowWeapon);
            if (Input.GetKeyDown(KeyCode.Alpha4)) HandleRequest(iceWeapon);

            /* 2�� : ���� ������ ��Draw�� */
            if (Input.GetKeyDown(KeyCode.Alpha2) &&
                !fighter.IsCurrentWeapon(swordWeapon))
            {
                fighter.StartDraw(swordWeapon);
            }
        }

        void HandleRequest(WeaponConfig nextWeapon)
        {
            if (fighter.IsCurrentWeapon(swordWeapon) && fighter.IsWeaponDrawn())
                fighter.StartSheath(nextWeapon);   // �ִϸ��̼� �� ��ü
            else
                fighter.EquipWeapon(nextWeapon);   // ��� ��ü
        }
    }
}
