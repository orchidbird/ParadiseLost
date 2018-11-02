using UnityEngine;
using Enums;
using Battle.Damage;
using System;
using System.Collections.Generic;

namespace Battle.Skills {
	class Ratice_3_r_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target){
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, this.passiveSkill, target);
		}
	}
}
