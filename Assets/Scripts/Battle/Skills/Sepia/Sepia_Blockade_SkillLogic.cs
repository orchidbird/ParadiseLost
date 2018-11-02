using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle.Skills {
	class Sepia_Blockade_SkillLogic : BasicChargeSkillLogic { // BasicChargeSkillLogic을 상속
		public override void ApplyAdditionalDamage(CastingApply castingApply) {
			Unit caster = castingApply.Caster;
			Unit target = castingApply.Target;
			if (target.IsAllyTo(caster))
				castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 0);
		}
    }
}
