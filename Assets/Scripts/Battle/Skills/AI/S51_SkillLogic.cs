using Battle.Damage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Skills {
    class S51_SkillLogic : BasePassiveSkillLogic {
        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            if (statusEffect.IsRestriction)
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(target, passiveSkill, target);
            return true;
        }
        public override void TriggerOnStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit)
        {
	        // 군중제어가 사라질 경우, 그리고 군중제어인 다른 cc기가 없을 경우
	        if (!statusEffect.IsRestriction ||
	            unit.statusEffectList.Any(se => (se != statusEffect && se.IsRestriction))) return;
	        {
		        UnitStatusEffect weakness = unit.statusEffectList.Find(se => se.GetOriginSkillName() == passiveSkill.Name);
		        unit.RemoveStatusEffect(weakness);
	        }
        }
    }
}
