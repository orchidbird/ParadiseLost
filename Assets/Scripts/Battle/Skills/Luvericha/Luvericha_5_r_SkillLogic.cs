
namespace Battle.Skills {
    class Luvericha_5_r_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply)
        {
	        castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1 + 0.05f * castingApply.GetTargetCount());
            //castingApply.GetDamage().RelativeModifier *= 1 + (0.05f * castingApply.GetTargetCount());
        }
    }
}
