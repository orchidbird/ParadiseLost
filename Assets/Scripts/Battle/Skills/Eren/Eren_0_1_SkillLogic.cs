using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
	public class Eren_0_1_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
			if(actionType == TrigActionType.Kill) StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(hitInfo.caster, this.passiveSkill, hitInfo.caster);
		}
	}
}