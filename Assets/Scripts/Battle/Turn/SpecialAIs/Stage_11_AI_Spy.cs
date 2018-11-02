using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Turn{
	public class Stage_11_AI_spySword : AI{
		public override void SetFirstState (){
			state = "Approach";
		}

		/*public override IEnumerator Approach(){
			if (!unit.HasAnyWayToMove()) {
				state = "CastingLoop";
				yield break;
			}
			yield return AIUtil.CalculateBestEscapeRoute (unit, _AIData.goalArea);
			if (destTile == unit.TileUnderUnit) {
				state = "InitialState";
			} else {
				yield return MoveWithDestroyRoutine(destTile);
				state = "Standby";
			}
		}

		public override IEnumerator InitialState(){
			if (!unit.HasAnySkillToCast()) {
				state = "Standby";
				yield break;
			}
			if (!unit.HasAnyWayToMove()) {
				state = "CastingLoop";
				yield break;
			}
			
			var bestCasting = GetBestPlan();
			if (bestCasting == null)
				state = "Standby";
			else{
				yield return MoveWithDestroyRoutine (bestCasting.path.dest);
				state = "CastingLoop";
			}
		}*/
	}
}
