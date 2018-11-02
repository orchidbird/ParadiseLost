using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_6_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(Unit caster, Unit yeong)
	{
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(yeong, this.passiveSkill, yeong);
	}
}
}
