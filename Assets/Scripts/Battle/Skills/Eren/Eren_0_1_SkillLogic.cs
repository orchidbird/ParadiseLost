using Enums;
using Battle.Damage;

namespace Battle.Skills{
	public class Eren_0_1_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
			if(actionType == TrigActionType.Kill) StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(hitInfo.caster, this.passiveSkill, hitInfo.caster);
		}
	}
}