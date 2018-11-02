
namespace Battle.Skills {
    class Karldrich_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            if(castingApply.Caster.GetHP < castingApply.Target.GetHP)
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.15f);
                //castingApply.GetDamage().RelativeModifier *= 1.15f;
        }
    }
}
