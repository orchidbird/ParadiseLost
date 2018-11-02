using Battle.Damage;
using Enums;

namespace Battle.Skills {
	class Luvericha_B15_SkillLogic : AttachOnStart {
		public override bool TriggerStatusEffectApplied(UnitStatusEffect unitStatusEffect, Unit caster, Unit target){
			return (caster == target) == unitStatusEffect.IsAura();
		}
    }
}
