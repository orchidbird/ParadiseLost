using System.Linq;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Bianca_3_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerAfterCast(CastLog castLog) {
            Unit caster = castLog.casting.Caster;
            if(castLog.casting.Skill.tileStatusEffectList.Any(se => 
                se.actuals.Any(actual => actual.statusEffectType == StatusEffectType.Trap))) {
                    StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
                }
        }
    }
}
