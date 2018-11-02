using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;

namespace Battle.Skills {
	class Bianca_ChewingTrap_SkillLogic : BaseActiveSkillLogic {
		public override bool CheckApplyPossibleToTargetTiles(Casting casting){
			return casting.RealRange.Any(tile => !tile.IsUnitOnTile());
		}

        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        return false;
        }
        public override bool TriggerTileStatusEffectApplied(TileStatusEffect tileStatusEffect) {
            return Trap.TriggerOnApplied(tileStatusEffect);
        }
    }
}
