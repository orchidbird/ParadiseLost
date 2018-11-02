
namespace Battle.Skills {
    class Json_Shadow_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            UnitStatusEffect mark = castingApply.Target.GetSEofDisplayNameKor("표식");
	        int stack = mark == null ? 0 : mark.Stack;
	        castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 1 + stack * 0.15f);
        }
    }
}
