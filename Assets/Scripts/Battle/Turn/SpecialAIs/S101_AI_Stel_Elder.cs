using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Turn{
	public class S101_AI_Stel_Elder : AI{
		public override void SetFirstState (){
			state = "Stel_Elder";
		}

		public IEnumerator Stel_Elder(){
			ActiveSkill buffSkill = unit.GetActiveSkillList () [1]; // 대지의 비늘(아군전체버프)
			if (unit.IsThisSkillUsable (buffSkill)) {
				Casting casting = new Casting (unit, buffSkill, new SkillLocation (unit.TileUnderUnit, unit.TileUnderUnit, unit.GetDir ()));
				yield return UseSkill (casting);
			}
			state = "NeverMoveCastingLoop";
		}
	}
}
