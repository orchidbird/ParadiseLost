using System;
using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_B8_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply)
	{
		int casterHeight = castingApply.Caster.GetHeight();
		int targetHeight = castingApply.Target.GetHeight();
        DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		// caster가 더 높을 경우 피해량이 높이차 * 15% 만큼 상승
		castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, Math.Max(casterHeight - targetHeight, 0) * 0.15f + 1);
	}
}
}
