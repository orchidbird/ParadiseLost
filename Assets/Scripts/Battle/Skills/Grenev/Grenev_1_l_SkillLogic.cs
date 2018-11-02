using Enums;
using Battle.Damage;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills{
	class Grenev_1_l_SkillLogic : BaseActiveSkillLogic {
		public override int GetRealAPWithOverload(int originAP, Unit caster)
		{
			UnitStatusEffect overloadSE = caster.statusEffectList.Find (se => se.GetOriginSkillName () == activeSkill.GetName () && se.IsTypeOf(StatusEffectType.Overload));
			if (overloadSE != null) {
				return originAP + (int)overloadSE.GetAmountOfType (StatusEffectType.Overload);
			} else {
				return originAP;
			}
		}

		public override void AttachOverload(Unit caster)
		{
			StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets (caster, caster, activeSkill, 1, add: true);
		}
    }
}
