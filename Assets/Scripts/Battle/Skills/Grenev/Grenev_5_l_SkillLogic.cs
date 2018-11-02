
namespace Battle.Skills {
    class Grenev_5_l_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;

            if(caster.GetHeight() <= target.GetHeight() - 3) {
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.5f);
                //castingApply.GetDamage().RelativeModifier *= 1.5f;
            }
        }
    }
}
