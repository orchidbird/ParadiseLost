using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
    class Sepia_A15_SkillLogic : BasePassiveSkillLogic {
        // 현재 AP/0.7 내에서 이동할 수 있는 경로들을 찾은 후, 최종 타일이 동료의 옆 타일인 경로에 대해서는 requireAP에 0.7을 곱한다.
        // 그 후에, requireAP가 현재 AP 이하인 경로들만 남긴다.
        public override Dictionary<Vector2Int, TileWithPath> GetMovablePath(Unit unit) {
            int maxAP = (int)(unit.GetCurrentActivityPoint() / (1 - 0.3f));
            Dictionary<Vector2Int, TileWithPath> path = PathFinder.CalculatePaths(unit, false, maxAP);
            var resultPath = new Dictionary<Vector2Int, TileWithPath>();
            foreach(var kv in path) {
                bool allyIsNearby = false;
                foreach(var direction in EnumUtil.directions) {
                    Tile nearbyTile = TileManager.Instance.GetTile(kv.Key + Utility.DirToV2I(direction));
                    if(nearbyTile != null && nearbyTile.IsUnitOnTile() && nearbyTile.GetUnitOnTile() != unit 
                            && nearbyTile.GetUnitOnTile().IsAllyTo(unit))
                        allyIsNearby = true;
                }
				if (allyIsNearby)
					kv.Value.MultiplyRequireAP (0.7f);
                
                if (kv.Value.RequireActivityPoint <= unit.GetCurrentActivityPoint())
                    resultPath.Add(kv.Key, kv.Value);
            }
            return resultPath;
        }
    }
}
