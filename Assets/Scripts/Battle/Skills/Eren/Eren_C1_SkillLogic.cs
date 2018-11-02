using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills
{
public class Eren_C1_SkillLogic : BaseActiveSkillLogic {
    public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply,int chain){
	    var caster = castingApply.Caster;
        UnitStatusEffect AbsorptionStatusEffect = caster.GetSEofDisplayNameKor("흡수");
        int stack = 0;
        if (AbsorptionStatusEffect != null)
            stack = AbsorptionStatusEffect.Stack;
        float power = caster.GetStat(Stat.Power);
        
        float amount = (float)statusEffect.actuals[0].formula.Substitute(stack);
        statusEffect.SetAmount(0, amount * power);
        return true;
    }
}
}
