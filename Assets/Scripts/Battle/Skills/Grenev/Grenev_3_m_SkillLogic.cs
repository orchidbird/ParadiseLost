
namespace Battle.Skills {
    class Grenev_3_m_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamageFromTargetStatusEffect(CastingApply castingApply, UnitStatusEffect statusEffect) {
            Unit caster = castingApply.Caster;

			if(statusEffect.GetCaster() == caster) {
				castingApply.GetDamage ().relativeModifiers.Add (activeSkill.icon, 1.4f);
            }
        }
    }
}
