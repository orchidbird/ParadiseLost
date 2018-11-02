using UnityEngine;
using Enums;
using Battle.Damage;

namespace Battle.Skills
{
public class Yeong_HardTarget_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(Unit caster, Unit yeong){
		if(yeong.IsEnemyTo(caster))
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(yeong, passiveSkill, caster);
	}
}
}
