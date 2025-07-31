using UnityEngine;

namespace RPG.Combat   
{

    public class ResetComboOnDodgeExit : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator,
                                         AnimatorStateInfo stateInfo,
                                         int layerIndex)
        {
            var pCombat = animator.GetComponent<PlayerCombat>();
            if (pCombat == null) return;

            pCombat.ForceResetCombo();

            if (pCombat.WasTargeting)
                animator.SetBool("isTargeting", true);
        }
    }
}
