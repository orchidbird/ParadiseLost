using Enums;
using UnityEngine;
using UtilityMethods;

namespace Battle.Skills {
	class Arcadia_Weathering_SkillLogic : BaseActiveSkillLogic {
		public override void ActionInDamageRoutine(CastingApply castingApply){
			var caster = castingApply.Caster;
			if (Calculate.Distance(caster, castingApply.Target) <= 1)
				castingApply.Target.ApplyDamageByNonCasting(caster.GetStat (Stat.Power) * 0.8f, caster, true);
		}
    }
}
