using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills
{
public class Yeong_2_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(Unit caster, Unit yeong)
	{
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(yeong, this.passiveSkill, yeong);
	}
}
}
