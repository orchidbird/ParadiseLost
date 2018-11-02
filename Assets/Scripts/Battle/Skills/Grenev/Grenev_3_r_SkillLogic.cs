
using Enums;

namespace Battle.Skills {
    class Grenev_3_r_SkillLogic : BasePassiveSkillLogic{
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;

            if (caster.GetStat(Stat.Power) > target.GetStat(Stat.Power)){
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.25f);
                //castingApply.GetDamage().RelativeModifier *= 1.25f;
            }
        }
    }
}
