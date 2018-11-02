using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Reina_4_m_SkillLogic : BaseActiveSkillLogic {
	public override void ApplyAdditionalDamage(CastingApply castingApply) 
    {
		// 공버프가 총 30% 면 이 값은 1.3이 된다
        Unit caster = castingApply.Caster;
		float damageBonusToAttackBuff = (float)caster.GetStat(Stat.Power) / caster.GetBaseStat(Stat.Power);
	    castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, Math.Max(damageBonusToAttackBuff, 1));
		
		/*if (damageBonusToAttackBuff < 1.0f)
			damageBonusToAttackBuff = 1.0f;

		castingApply.GetDamage().RelativeModifier *= damageBonusToAttackBuff;*/
	}
}
}
