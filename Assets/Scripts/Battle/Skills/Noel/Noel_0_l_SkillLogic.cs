using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Noel_0_l_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount, bool duringAIDecision) {
			if (!duringAIDecision) {
				attacker.ApplyDamageByNonCasting (amount, shieldCaster, true);
			}
        }
    }
}
