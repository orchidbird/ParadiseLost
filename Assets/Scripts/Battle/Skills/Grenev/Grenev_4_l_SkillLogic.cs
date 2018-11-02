
namespace Battle.Skills {
    class Grenev_4_l_SkillLogic : BaseActiveSkillLogic {
        public override float ApplyIgnoreDefenceRelativeValueBySkill(float defense, Unit caster, Unit target) {
            return defense * 0.5f;
        }
    }
}
