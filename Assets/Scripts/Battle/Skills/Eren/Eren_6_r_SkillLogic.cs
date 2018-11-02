using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills
{
    public class Eren_6_r_SkillLogic : BaseActiveSkillLogic {
	    /*public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	    {
		    StatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
		    int stack = 0;

		    if (uniqueStatusEffect != null)
			    stack = uniqueStatusEffect.GetRemainStack();
		
		    // 0.6(+흡수 중첩당 0.1)x공격력
		    float amount = (0.6f + (stack * 0.1f)) * caster.GetActualStat(Stat.Power); 

		    var statusEffect1st = statusEffects.Find(se => se.GetOriginSkillName() == "광휘");
		    statusEffect1st.SetRemainPhase(999);
		    statusEffect1st.SetAmount(0, amount);
	    }*/
    }
}