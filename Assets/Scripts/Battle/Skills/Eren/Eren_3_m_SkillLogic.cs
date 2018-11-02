using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Eren_3_m_SkillLogic : BasePassiveSkillLogic {
	public override void OnCastingAmountCalculation(CastingApply castingApply) 
    {
		//float damageBonus = 1.15f;
		
		if (castingApply.Target.GetMaxHealth() < castingApply.Caster.GetMaxHealth())
			castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.15f);
			//castingApply.GetDamage().RelativeModifier *= damageBonus;
	}
}
}
