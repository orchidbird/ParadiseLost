using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    class AttachAura : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting (UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			return statusEffect.IsTypeOf (StatusEffectType.Aura);
		}
    }
}
