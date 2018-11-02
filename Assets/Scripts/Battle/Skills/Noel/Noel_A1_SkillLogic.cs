using Enums;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    class Noel_A1_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
            return chain > 1;
        }
    }
}
