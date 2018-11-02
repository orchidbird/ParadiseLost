using Battle.Damage;

namespace Battle.Skills {
    public class Curi_5_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnPhaseEnd(Unit caster) {
            if(caster.notMovedTurnCount == 0) {
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
            }
        }
    }
}
