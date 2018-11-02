using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class S71_AI_Holder : AI{
		public override void SetFirstState() {
			state = "ChildHolder";
		}

		public IEnumerator ChildHolder(){
			Unit eren = UnitManager.Instance.GetAnUnit ("eren");
			Unit nearChild = null;
			foreach (Direction direction in EnumUtil.directions) {
				Tile tileNearHolder = TileManager.Instance.GetTile (unit.Pos + Utility.DirToV2I(direction));
				if (tileNearHolder == null || !tileNearHolder.IsUnitOnTile()) continue;
				Unit nearUnit = tileNearHolder.GetUnitOnTile ();
				if (nearUnit.CodeName.StartsWith("child"))
					nearChild = nearUnit;
			}

			if (nearChild == null || eren == null){
				state = "InitialState";
			}else {
				movablePathDict = PathFinder.CalculatePaths(unit, false);
				var range = new List<Vector2Int> ();
				foreach (Direction direction in EnumUtil.directions) {
					range.Add (nearChild.Pos + Utility.DirToV2I (direction));
				}
				Tile destTile = Utility.GetFarthestTileToUnit (range, movablePathDict, eren);
				if (destTile != null) {
					yield return MoveToThePosition (destTile.Location);
				}
				state = "NeverMoveCastingLoop";
			}
			
			yield return null;
		}
	}
}
