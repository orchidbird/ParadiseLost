using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;

public class S171_Pack_BattleTrigger : BattleTrigger {
	public S171_Pack_BattleTrigger(TrigResultType resultType, StringParser commaParser) : base(resultType, commaParser) {
		TriggerAction = () => {
			Unit target = ((UnitDestroyLog)logs.Last()).actor;
			Unit baggage = ((UnitDestroyLog)logs.Last()).target;

			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(baggage, baggage.GetPassiveSkillList()[0], target);
		};
	}
}
