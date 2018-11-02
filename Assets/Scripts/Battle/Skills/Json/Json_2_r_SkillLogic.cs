using Battle.Damage;

namespace Battle.Skills {
	class Json_2_r_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerAfterCast(CastLog castLog) {
			Unit caster = castLog.casting.Caster;
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
		}
	}
}
