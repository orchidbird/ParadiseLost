using Battle.Damage;
using System.Collections.Generic;
using System;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
	class Eugene_P0_SkillLogic : BasePassiveSkillLogic {
		/*public override void TriggerExistingUnitPassiveBeforeCast(Casting casting, Unit existingUnit) {
			List<Unit> targets = casting.GetTargets();
			if (!casting.Skill.IsOffensive()
			    || casting.Caster.GetUnitClass() != UnitClass.Magic 
			    || targets.All(target => !target.IsAllyTo(existingUnit))) return;
			
			int targetNumber = targets.Count;
			targetNumber = Math.Min (6, targetNumber);
			casting.relativeDamageBonus *= (1 - targetNumber * 0.05f);
		}*/

		public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
			var casting = castingApply.GetCasting();
			List<Unit> targets = casting.GetTargets();
			if (casting.Caster.GetUnitClass() != UnitClass.Magic 
			    || targets.All(target => !target.IsAllyTo(passiveSkill.Owner))) return;
			
			castingApply.GetDamage().AddModifier(passiveSkill, 1 - Math.Min(6, targets.Count) * 0.05f);
		}
	}
}
