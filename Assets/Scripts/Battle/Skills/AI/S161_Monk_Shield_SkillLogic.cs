using System;
using System.Collections;
using Enums;
using Battle.Damage;
using System.Collections.Generic;
using System.Linq;

namespace Battle.Skills {
	class S161_Monk_Shield_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			return castingApply.Target.GetSide () == castingApply.Caster.GetSide() && !castingApply.Target.IsObject;
		}
	}
}
