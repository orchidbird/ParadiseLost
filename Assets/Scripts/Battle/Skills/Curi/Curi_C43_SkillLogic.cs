using Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_C43_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        var target = castingApply.Target;
	        return target.GetHP <= target.GetMaxHealth() * 0.4f;
        }
    }
}
