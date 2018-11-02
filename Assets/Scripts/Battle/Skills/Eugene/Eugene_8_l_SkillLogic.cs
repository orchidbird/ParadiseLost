using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Eugene_8_l_SkillLogic : BaseActiveSkillLogic {
        public override void TriggerStatusEffectAtReflection(Unit target, UnitStatusEffect statusEffect, Unit reflectTarget) {
			BattleManager battleManager = BattleManager.Instance;
            float maxHealth = target.GetMaxHealth();
            float percentage = statusEffect.GetAmount(1) / 100;
            target.RecoverHealth(maxHealth * percentage, target);
        }
        public override bool TriggerStatusEffectWhenStatusEffectApplied(Unit target, UnitStatusEffect statusEffect, UnitStatusEffect appliedStatusEffect) {
            appliedStatusEffect.DecreaseRemainPhase((int)statusEffect.GetAmount(2));
            if (appliedStatusEffect.Duration() <= 0) {
                return false;
            } else return true;
        }
    }
}
