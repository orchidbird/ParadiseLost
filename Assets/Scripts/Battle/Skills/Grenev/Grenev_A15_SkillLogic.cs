using UtilityMethods;

namespace Battle.Skills {
	class Grenev_A15_SkillLogic : AttachOnStart {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;

            int distance = Calculate.Distance(caster.Pos, target.Pos);
	        castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, (float)(1 + 0.02 * distance));
        }
    }
}
