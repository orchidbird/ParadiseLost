using System.Collections.Generic;

namespace Battle.Skills {
    class Eugene_3_m_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain) {
            List<UnitStatusEffect> statusEffectList = castingApply.Target.statusEffectList;
            UnitStatusEffect statusEffectToChange = statusEffectList.Find(se => (se.GetOriginSkillName() == "순백의 방패"
                                        && se.IsTypeOf(Enums.StatusEffectType.Aura)));
			
			if (statusEffectToChange == null) return false;
			castingApply.Target.RemoveStatusEffect(statusEffectToChange);
			return true;
		}
    }
}
