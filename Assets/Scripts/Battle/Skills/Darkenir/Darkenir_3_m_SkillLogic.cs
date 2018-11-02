using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Darkenir_3_m_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerAfterDamagedByCasting(Unit target, Unit caster) {
            if(caster.GetUnitClass() == UnitClass.Magic)
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(target, passiveSkill, target);
        }
    }
}
