using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_6_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerAfterDamaged(Unit yeong, int finalDamage, Unit caster){
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(yeong, passiveSkill, yeong);
	}
}
}
