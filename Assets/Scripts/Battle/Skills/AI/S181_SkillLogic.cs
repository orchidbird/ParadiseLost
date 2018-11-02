using UnityEngine;
using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
	public class S181_SkillLogic : BaseActiveSkillLogic {
		// 선교사의 '찬송' 스킬로직
		public override void ActionInDamageRoutine(CastingApply castingApply) {
			Unit caster = castingApply.Caster;
			int amount = (int)(caster.GetStat(Stat.Agility) * 0.3f);
			castingApply.Target.ChangeAP(amount);
		}
	}
}
