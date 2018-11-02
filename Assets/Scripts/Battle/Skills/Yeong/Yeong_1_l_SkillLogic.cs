using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Skills
{
public class Yeong_1_l_SkillLogic : BaseActiveSkillLogic {
	public override void ApplyAdditionalDamage(CastingApply castingApply) 
    {
		//float damageBonus = 1.2f;

		List<UnitStatusEffect> statusEffects = castingApply.Caster.statusEffectList;
		//bool isUniquePassiveActive = ;
		if (statusEffects.Any(x => x.GetOriginSkillName() == "방랑자"))
			castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 1.2f);
			//castingApply.GetDamage().RelativeModifier *= damageBonus;
            
	}
}
}
