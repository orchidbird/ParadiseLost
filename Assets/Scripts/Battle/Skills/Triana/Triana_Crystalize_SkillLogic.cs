using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    class Triana_Crystalize_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        return (castingApply.Caster.GetElement() == Element.Water) == statusEffect.IsBuff;
        }
    }
}
