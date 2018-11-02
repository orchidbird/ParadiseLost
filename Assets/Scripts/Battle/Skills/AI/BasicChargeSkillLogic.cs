using UnityEngine;
using UtilityMethods;
using System.Collections.Generic;

namespace Battle.Skills {
	class BasicChargeSkillLogic : BaseActiveSkillLogic {
		public override void ActionBeforeMainCasting(Casting casting) {
			Vector2Int casterPos = casting.Location.CasterPos;
			if (casting.RealRange.Count == 0) // CheckApplyPossibleToTargetTiles에서 미리 확인했으니 시전시엔 반드시 count == 1이지만 혹시 모를 버그에서 게임이 멈추지 않게 하려고
				return;
			Tile targetTile = casting.RealRange [0]; // 1. 돌진의 효과범위는 항상 단일 타일 2. 대형 유닛에게 돌진하면 타겟 타일이 타겟 유닛의 가운데가 아닐 수 있으므로 타겟 유닛이 아니라 타일 기준

			casting.Caster.ForceMove(Utility.GetChargePath(casterPos, targetTile.location), forced: false, charge: true);
		}

		public override bool CheckApplyPossibleToTargetTiles(Casting casting) { // 대상의 바로 앞까지 이동 가능해야 시전 가능
			Vector2Int casterPos = casting.Location.CasterPos;
			List<Tile> targetTiles = casting.RealRange;
			if (targetTiles.Count == 0)
				return false;
			Tile targetTile = targetTiles [0];
			return Utility.GetChargePath (casterPos, targetTile.location).Count == Calculate.Distance(casterPos, targetTile.location);
		}
	}
}
