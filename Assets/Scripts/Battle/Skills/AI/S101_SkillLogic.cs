using System;
using System.Collections;
using Enums;
using Battle.Damage;
using System.Collections.Generic;
using System.Linq;

namespace Battle.Skills {
	class S101_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			return castingApply.Caster.IsAllyTo (castingApply.Target); // 효과는 버프와 기절 두 가지가 있는데, 기절은 Overload 태그가 붙어있어서 시전자 자신에게만 붙음
		}
	}
}
