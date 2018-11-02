using Enums;

namespace Battle.Skills {
    class Grenev_6_m_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            if(caster.HasStatusEffect(StatusEffectType.Stealth)) {
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.5f);
                //castingApply.GetDamage().RelativeModifier *= 1.5f;
            }
        }
    }
}
