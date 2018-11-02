using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

namespace Battle.Skills
{
public class Yeong_2_l_SkillLogic : BaseActiveSkillLogic {
	public override void ApplyAdditionalDamage(CastingApply castingApply) 
    {
		int totalEvasion = 0;

		List<UnitStatusEffect> statusEffects = castingApply.Caster.statusEffectList;
		foreach (var statusEffect in statusEffects)
		{
			int num = statusEffect.actuals.Count;
			for (int i = 0; i < num; i++)
			{
				if (statusEffect.IsOfType(i, StatusEffectType.EvasionChange))
				{
					totalEvasion += (int)statusEffect.GetAmount(i);
				}
			}
		}

	    castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, (float)(100 + totalEvasion) / 100);
		//float damageBonus = (float)(100 + totalEvasion) / 100;
		//castingApply.GetDamage().RelativeModifier *= damageBonus;
	}
}
}
