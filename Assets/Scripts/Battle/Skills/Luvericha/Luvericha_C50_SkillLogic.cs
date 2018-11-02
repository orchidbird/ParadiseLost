using System.Collections.Generic;

namespace Battle.Skills {
    class Luvericha_C50_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
            return castingApply.Caster != castingApply.Target;
        }
    }
}
