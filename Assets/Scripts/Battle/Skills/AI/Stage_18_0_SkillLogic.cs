using UnityEngine;
using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
	public class Stage_18_0_SkillLogic : BaseActiveSkillLogic {
		//선교사의 '혼비백산' 스킬로직
		public override bool CheckApplyPossibleToTargetTiles(Casting casting) {
			Vector2Int casterPos = casting.Location.CasterPos;
			Vector2Int dirVec = Utility.DirToV2I(casting.Location.Dir);
			int range = casting.Skill.firstRange.max;
			Vector2Int virtualTargetPos = casterPos + dirVec * (range + 1); // 범용 돌진과 달리 끝에 대상 유닛이 없으니 가상 대상 위치좌표 생성

			return Utility.GetChargePath (casterPos, virtualTargetPos).Count == range + 1;
		}
		public override void ActionBeforeMainCasting(Casting casting) {
			TileManager TM = TileManager.Instance;
			Vector2Int dirVec = Utility.DirToV2I(casting.Location.Dir);
			var perpVec = new Vector2Int(dirVec.y, -dirVec.x);
			List<Tile> path = new List<Tile>();
			List<Unit> targets = new List<Unit>();
			List<Tile> targetTiles = new List<Tile>();
			path.Add (casting.Location.CasterTile);
			for(int i = 1; i <= casting.Skill.firstRange.max; i++) {
				Vector2Int pos = casting.Location.CasterPos + dirVec * i;
				Tile tile = TM.GetTile(pos);
				path.Add(tile);
				for(int j = -1; j <= 1; j += 2) {
					Tile nearTile = TM.GetTile(pos + perpVec * j);
					if (nearTile == null) continue;
					Unit target = nearTile.GetUnitOnTile();
					if (target == null) continue;
					targets.Add(target);
					targetTiles.Add(target.TileUnderUnit);
				}
			}
			casting.Caster.ForceMove(path, forced: false, charge: true);
			foreach (var target in targets) {
				StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets (casting.Caster, target, activeSkill, 1, add: true, byCasting: true);
			}
		}
	}
}
