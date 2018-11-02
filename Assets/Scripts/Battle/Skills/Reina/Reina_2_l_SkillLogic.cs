using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
    public class Reina_2_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target)
        {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, this.passiveSkill, target);
        }
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            return caster.statusEffectList.Count(x => x.IsBuff && x.GetCaster() != caster);
        }
    }
}
