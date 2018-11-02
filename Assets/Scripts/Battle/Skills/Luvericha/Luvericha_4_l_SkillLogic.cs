
namespace Battle.Skills {
    class Luvericha_4_l_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit target = castingApply.Target;
            castingApply.GetDamage().baseDamage = target.GetCurrentActivityPoint() * 0.5f;
        }
    }
}
