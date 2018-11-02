using Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_C43_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        var target = castingApply.Target;
	        return (target.GetHP <= target.GetMaxHealth() * 0.4f) && (target.GetUnitClass() == UnitClass.Magic);
        }
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            if(castingApply.Target.GetUnitClass() != UnitClass.Magic) {
	            castingApply.GetDamage().relativeModifiers.Add(Resources.Load<Sprite>("Icon/Magic"), 0);
            }
        }
    }
}
