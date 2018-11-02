using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
	public class Reina_3_m_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target){
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, target);
		}
	}
}
