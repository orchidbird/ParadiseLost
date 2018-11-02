using Enums;
using Battle.Damage;

namespace Battle.Skills {
	class Eugene_6_r_SkillLogic : AttachOnStart{
        public override void TriggerStatusEffectsOnRest(Unit target, UnitStatusEffect statusEffect) {
            if (!statusEffect.IsTypeOf(StatusEffectType.Aura)) {
                target.RemoveStatusEffect(statusEffect.GetCaster(), StatusEffectCategory.Debuff, 1);
            }
        }
    }
}
