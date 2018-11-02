using Battle.Damage;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    class Reina_C15_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        var caster = castingApply.Caster;
            int count = caster.RemoveStatusEffect(caster, StatusEffectCategory.Buff, int.MaxValue);
	        if (count <= 0) return false;
	        
	        // 0.4 x 공격력
	        float amount = (float)statusEffect.actuals[0].formula.Substitute(caster.GetStat(Stat.Power));
	        statusEffect.SetAmount(0, amount * count);
	        return true;
        }
    }
}
