using System;
using System.Collections;
using Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle.Skills{
    class Sepia_A8_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			var caster = castingApply.Caster;
			List<Unit> units = Utility.UnitsInRange(Utility.TilesInDiamondRange(caster.Pos, 1, 2, 1));
			return units.Any (unit => unit.IsAllyTo(caster) && unit.GetHpRatio() < 0.5f);
        }
    }
}
