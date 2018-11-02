using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_1_r_SkillLogic : BasePassiveSkillLogic
{
	public override void TriggerUsingSkill(Casting casting, List<Unit> targets)
	{
        Unit caster = casting.Caster;
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, this.passiveSkill, caster);
	}
}
}
