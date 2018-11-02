using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class S181_AI_Missionary : AI{
		public override void SetFirstState() {
			state = "Missionary";
		}

		public IEnumerator Missionary(){

			ActiveSkill debuffSkill = unit.GetActiveSkillList () [0]; // 혼비백산
			if (unit.IsThisSkillUsable (debuffSkill)) {
				int skillAP = unit.GetActualRequireSkillAP (debuffSkill);
				movablePathDict = PathFinder.CalculatePaths(unit, false);
				foreach (var pair in movablePathDict) {
					var tile = pair.Value.dest;

					int possibleSkillUseCount = (unit.GetCurrentActivityPoint() - pair.Value.RequireActivityPoint) / skillAP;
					if (possibleSkillUseCount <= 0)
						continue;

					List<Casting> castingCandidates = debuffSkill.GetPossibleCastings (unit, tile);
					int count = castingCandidates.Count;
					if(count > 0){
						yield return MoveToThePosition (tile.location);
						Casting casting = castingCandidates [Random.Range (0, count)];
						yield return UseSkill (casting);
						break;
					}
				}
			}

			ActiveSkill healSkill = unit.GetActiveSkillList () [1]; // 찬송
			if (unit.IsThisSkillUsable (healSkill)) {
				Casting casting = new Casting (unit, healSkill, new SkillLocation (unit.TileUnderUnit, unit.TileUnderUnit, unit.GetDir ()));
				yield return UseSkill (casting);
			}

			state = "EndTurn";
		}
	}
}
