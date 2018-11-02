using System.Collections.Generic;
using UtilityMethods;

namespace Battle.Skills {
	class Stage_20_2_SkillLogic : BaseActiveSkillLogic {
		public override bool CheckApplyPossibleToTargetTiles (Casting casting){
			Tile casterTile = casting.Location.CasterTile;
			if (casterTile.GetHeight () >= 3) {
				List<Tile> destCandidates = TileManager.Instance.GetTilesInGlobalRange ().FindAll (tile => tile.GetHeight () == casterTile.GetHeight () && !tile.IsUnitOnTile () && Calculate.Distance(tile.Location, casterTile.Location) > 15);
				return destCandidates.Count >= 1;
			}
			return false;
		}
		public override void OnCast(Casting casting) {
			Unit caster = casting.Caster;
			Tile casterTile = caster.TileUnderUnit;

			List<Tile> destCandidates = TileManager.Instance.GetTilesInGlobalRange ().FindAll (tile => tile.GetHeight () == casterTile.GetHeight () && !tile.IsUnitOnTile () && Calculate.Distance(tile.Location, casterTile.Location) > 15);

			int randNum = UnityEngine.Random.Range (0, destCandidates.Count);
			Tile destTile = destCandidates[randNum];

			caster.ForceMove (new List<Tile> { casterTile, destTile }, forced: false);
		}
	}
}
