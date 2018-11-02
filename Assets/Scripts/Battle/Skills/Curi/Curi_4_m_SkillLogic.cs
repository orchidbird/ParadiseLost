using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_B22_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
            return true;
        }
    }
}
