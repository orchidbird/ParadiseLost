using Battle.Damage;
namespace Battle.Skills {
    public class AttachOnStart : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
        }
    }
}
