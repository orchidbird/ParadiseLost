using Enums;
using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using UnityEngine;

namespace Battle.Skills {
    class Sepia_HalfMoon_SkillLogic : BaseActiveSkillLogic {
		public override void ActionInDamageRoutine(CastingApply castingApply) {activeSkill.intTemp++;}
	    
		public override void OnCast(Casting casting) {
			var caster = casting.Caster;
			int targetCount = activeSkill.intTemp;
			
			if (targetCount <= 0) return;
			float power = caster.GetStat (Stat.Power);
			float addedShieldAmount = targetCount * 0.2f * power;
			StatusEffector.FindAndAttachUnitStatusEffectsOfPrecalculatedAmounts (caster, activeSkill, caster, new List<List<float>>{ new List<float>{ addedShieldAmount } });
			activeSkill.intTemp = 0;
		}
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			return castingApply.Target == castingApply.Caster;
		}
    }
}
