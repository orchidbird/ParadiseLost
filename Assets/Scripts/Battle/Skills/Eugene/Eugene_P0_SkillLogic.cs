using Battle.Damage;
using System.Collections.Generic;
using System;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
	class Eugene_P0_SkillLogic : BasePassiveSkillLogic {
		public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
			var casting = castingApply.GetCasting();
			List<Unit> targets = casting.GetTargets();
			if (targets.Any(target => target.IsAllyTo(passiveSkill.Owner)))
				castingApply.GetDamage().AddModifier(passiveSkill, 1 - Math.Min(6, targets.Count) * 0.05f);
		}
	}
}
