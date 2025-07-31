using UnityEngine;


namespace RPG.combat
{
    public class ResetComboOnExit : StateMachineBehaviour
    {
        public override void OnStateMachineExit(Animator anim, int stateMachinePathHash)
        {
            var pc = anim.GetComponent<RPG.Combat.PlayerCombat>();
            if (pc == null) return;

            pc.ForceResetCombo();
            if (pc.WasTargeting)
                anim.SetBool("isTargeting", true);
        }
    }
}
