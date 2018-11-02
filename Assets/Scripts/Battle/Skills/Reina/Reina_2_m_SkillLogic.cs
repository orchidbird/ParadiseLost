using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Reina_2_m_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply){
	    castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.15f);
	}
}
}
