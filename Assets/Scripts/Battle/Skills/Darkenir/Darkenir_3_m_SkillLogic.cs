using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Darkenir_3_m_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerAfterDamagedByCasting(Unit target, Unit caster) {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(target, passiveSkill, target);
        }
    }
}
