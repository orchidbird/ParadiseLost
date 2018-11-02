using Enums;
using Battle.Damage;
using System.Collections;

namespace Battle.Skills{
    class Grenev_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
		    if(actionType == TrigActionType.Kill) StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(hitInfo.caster, passiveSkill, hitInfo.caster);
        }
    }
}
