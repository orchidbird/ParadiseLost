using Battle.Damage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Skills {
	class S61_SkillLogic1 : BasePassiveSkillLogic{
        //6장 괴수 특성 "흉포함"
		
		public override void TriggerAfterDamaged(Unit target, int damage, Unit caster) {
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, target);
		}
    }
}
