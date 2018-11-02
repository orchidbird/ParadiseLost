using Battle.Damage;

namespace Battle.Skills {
	class Deus_2_r_SkillLogic : BasePassiveSkillLogic {
		public override void OnMyCastingApply(CastingApply castingApply) {
			Unit caster = castingApply.Caster;
			Unit target = castingApply.Target;
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, target);
		}
	}
}
