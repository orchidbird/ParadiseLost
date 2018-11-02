using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
    public class Reina_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target) {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, this.passiveSkill, target);
        }
        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            int numberOfBuffsFromOthers = caster.statusEffectList.Count(x => x.IsBuff && x.GetCaster() != caster);
            statusEffect.CalculateAmountManually(0, numberOfBuffsFromOthers);
            return true;
        }
    }
}
