
namespace Battle.Skills {
    class Luvericha_8_l_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit target = castingApply.Target;
            castingApply.GetDamage().baseDamage = target.GetCurrentActivityPoint() * 0.9f;
        }
    }
}
