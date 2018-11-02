using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply) 
    {
		//float damageBonusForPlantTypeUnit = 1.15f;

	    if (castingApply.Target.GetElement() == Enums.Element.Plant)
		    castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.15f);
			//castingApply.GetDamage().RelativeModifier *= damageBonusForPlantTypeUnit;
	}
}
}
