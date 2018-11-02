
namespace Battle.Skills {
    class Luvericha_7_r_SkillLogic : BasePassiveSkillLogic{
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit target = castingApply.Target;
            float LostHpPercent = 1 - (float)target.GetHP / target.GetMaxHealth();
	        castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1 + LostHpPercent * 0.3f);
            //castingApply.GetDamage().RelativeModifier *= 1 + LostHpPercent * 0.3f;
        }
    }
}
