using Enums;

namespace Battle.Skills {
    class Eugene_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnRest(Unit caster) {
            caster.RemoveStatusEffect(caster, StatusEffectCategory.Debuff, 1);
        }
    }
}
