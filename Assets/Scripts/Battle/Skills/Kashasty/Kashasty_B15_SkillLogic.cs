using System.Collections.Generic;

namespace Battle.Skills {
    class Kashasty_B15_SkillLogic : BaseActiveSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Tile casterTile = caster.TileUnderUnit;
            Unit target = castingApply.Target;
            Tile targetTile = target.TileUnderUnit;
			if (target.CanBeForcedToMove ()) {
				caster.ForceMove (new List<Tile> { casterTile, targetTile }, forced: false);
				target.ForceMove (new List<Tile> { targetTile, casterTile });
			}
        }
    }
}
