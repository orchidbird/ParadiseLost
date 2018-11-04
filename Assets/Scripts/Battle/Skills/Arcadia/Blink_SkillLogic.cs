using System.Collections.Generic;

namespace Battle.Skills {
    class Blink_SkillLogic : BaseActiveSkillLogic {
        public override bool CheckApplyPossibleToTargetTiles(Casting casting){
            var targetTile = casting.RealRange[0];
            return !targetTile.IsUnitOnTile();
        }

        public override void ActionBeforeMainCasting(Casting casting){
            casting.Caster.ForceMove(new List<Tile>{casting.Caster.TileUnderUnit, casting.RealRange[0]});
        }
    }
}
