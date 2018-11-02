using Battle.Damage;

namespace Battle.Skills {
    public class IgnoreRestriction : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
        }

	    public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){
		    return !statusEffect.IsRestriction;
	    }
    }
}
