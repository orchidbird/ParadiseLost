using System.Linq;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
	class S81_Link_SkillLogic : AttachOnStart {
        // S81 요정 공통 특성 "생명 연결" 스킬로직
		public override void TriggerOnMyTurnStart(Unit caster){
			var allys = UnitManager.GetAllUnits().FindAll(unit => unit.IsAllyTo(caster));
			var sumHP = allys.Sum(ally => ally.GetHP);
			var sumMax = allys.Sum(ally => ally.GetStat(Stat.MaxHealth));
			var HpRatio = (float)sumHP / sumMax;

			for(int i = 0; i < allys.Count; i++){
				allys[i].hp = (int)(allys[i].GetStat(Stat.MaxHealth) * HpRatio);
			}
		}
    }
}
