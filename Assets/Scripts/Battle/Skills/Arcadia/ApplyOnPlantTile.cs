using Enums;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class ApplyOnPlantTile : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            List<UnitStatusEffect> statusEffectList = caster.statusEffectList;
	        UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == passiveSkill.Name);
			
            caster.RemoveStatusEffect(statusEffect);
            if(statusEffect == null)
	            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
        }
    }
}
