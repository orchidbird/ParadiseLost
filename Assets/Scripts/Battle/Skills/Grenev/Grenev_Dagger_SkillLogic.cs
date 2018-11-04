using UnityEngine;
using System.Collections.Generic;
using UtilityMethods;

namespace Battle.Skills {
    class Grenev_Dagger_SkillLogic : BaseActiveSkillLogic {
		public override bool CheckApplyPossibleToTargetTiles(Casting casting){
            List<Tile> targetTiles = casting.RealRange;
			foreach(Tile tile in targetTiles){
				if (!tile.IsUnitOnTile()) continue;
				
				Unit target = tile.GetUnitOnTile();
				Vector2 targetDirection = Utility.DirToV2I(target.GetDir());
				Vector2 skillDirection = Calculate.NormalizeV2I(target.Pos - casting.Location.CasterPos);
				if(targetDirection == skillDirection) return true;
			}
            return false;
        }
        public override bool IgnoreShield(CastingApply castingApply) {
            return true;
        }
        public override float ApplyIgnoreDefenceAbsoluteValueBySkill(float defense, Unit caster, Unit target) {
			return System.Math.Min(0, defense);
        }
    }
}
