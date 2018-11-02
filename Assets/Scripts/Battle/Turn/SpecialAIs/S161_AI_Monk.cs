using System.Collections;
using System.Linq;
using Enums;

namespace Battle.Turn{
	public class S161_AI_Monk : AI{
		public override void SetFirstState() {
			state = "CantiumMonk";
		}

		public IEnumerator CantiumMonk(){
			ActiveSkill buffSkill = unit.GetActiveSkillList () [0]; // 칸티움의 울타리 - 모든 자기편 유닛(지형지물 제외)에게 보호막
			//쓸 수 있고, 자기편 중 한 명이라도 보호막이 없으면 일단 쓴다
			if (unit.IsThisSkillUsable (buffSkill) && !unit.GetAllies.All(ally => ally.HasStatusEffect(StatusEffectType.Shield))){
				var casting = new Casting (unit, buffSkill, new SkillLocation (unit.TileUnderUnit, unit.TileUnderUnit, unit.GetDir ()));
				yield return UseSkill (casting);
			}
			state = "InitialState";
		}
	}
}
