
namespace Battle.Skills {
    class Luvericha_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;
            if(caster != target) {
                float LostHpPercent = 1 - (float)caster.GetHP / caster.GetMaxHealth();
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1 + LostHpPercent * 0.5f);
                //castingApply.GetDamage().RelativeModifier *= 1 + LostHpPercent * 0.5f;
            }
        }
    }
}
