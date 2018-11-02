using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
    public class Curi_2_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            List<UnitStatusEffect> statusEffectList = caster.statusEffectList;

            UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "신속 반응");
            if(statusEffect != null)
                caster.RemoveStatusEffect(statusEffect);
            else{
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
            }
        }
    }
}
