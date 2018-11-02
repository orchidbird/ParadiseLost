using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply) 
    {
		//float damageBonusPerTarget = 0.05f;
		
		float totalDamageBonus = 1.0f + castingApply.GetTargetCount() * 0.05f;
	    castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, totalDamageBonus);
		//castingApply.GetDamage().RelativeModifier *= totalDamageBonus;
	}
}
}
