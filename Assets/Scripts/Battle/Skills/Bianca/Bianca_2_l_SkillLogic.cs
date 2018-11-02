using System.Collections.Generic;

namespace Battle.Skills {
    class Bianca_A8_SkillLogic : BaseActiveSkillLogic {
        public override bool CheckApplyPossibleToTargetTiles(Casting casting) {
            List<Tile> targetTiles = casting.RealRange;
            foreach (Tile tile in targetTiles) {
                if (tile.IsUnitOnTile()) {
                    return false;
                }
            }
            return true;
        }

        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        return castingApply.GetCasting().RealRange.Count != 1;
        }
        public override bool TriggerTileStatusEffectApplied(TileStatusEffect tileStatusEffect) {
            return Trap.TriggerOnApplied(tileStatusEffect);
        }
    }
}
