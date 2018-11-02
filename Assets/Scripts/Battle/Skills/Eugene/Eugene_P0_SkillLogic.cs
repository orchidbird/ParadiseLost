using System;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills {
	class Eugene_P0_SkillLogic : BasePassiveSkillLogic {
		public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
			var casting = castingApply.GetCasting();
			List<Unit> targets = casting.GetTargets();
			if (targets.Any(target => target.IsAllyTo(passiveSkill.owner)))
				castingApply.GetDamage().AddModifier(passiveSkill, 1 - Math.Min(6, targets.Count) * 0.05f);
		}
	}
}
