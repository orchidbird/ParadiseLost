using Battle.Damage;

namespace Battle.Skills {
    class Bianca_3_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActiveSkillAppliedToOwner(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;
			if(castingApply.GetSkill().IsFriendly())
				StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(target, passiveSkill, caster);
        }
    }
}
