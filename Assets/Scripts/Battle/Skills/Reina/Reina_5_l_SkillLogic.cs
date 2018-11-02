using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_5_l_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply) 
    {
		//float damageBonus = 1.3f;

		if ((castingApply.GetSkill().GetName() == "화염 폭발") && (castingApply.GetTargetCount() >= 3))
			castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.3f);
			//castingApply.GetDamage().RelativeModifier *= damageBonus;
	}
}
}
