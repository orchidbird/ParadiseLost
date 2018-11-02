using Battle.Damage;
using System.Collections;

namespace Battle.Skills {
    class Grenev_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnPhaseStart(Unit caster, int phase) {
            if(caster.notMovedTurnCount >= 3) {
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
            }
        }
        public override void TriggerOnMove(Unit caster) {
            UnitStatusEffect statusEffectToRemove = caster.statusEffectList.Find(se => se.GetOriginSkillName() == passiveSkill.Name);
            caster.RemoveStatusEffect(statusEffectToRemove);
        }
    }
}
