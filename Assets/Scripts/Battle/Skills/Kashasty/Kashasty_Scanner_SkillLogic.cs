using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    class Kashasty_Scanner_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerBeforeStartChain(List<Chain> chainList, Casting casting) {
            casting.SetTargets();
            if(casting.GetTargets().Count == 0) return;
				
			// 카샤스티의 모든 공격은 방향 보너스가 동일하므로 [0]으로 가져옴
            var virtualCastingApply = new CastingApply(casting, casting.GetTargets()[0]);
            DamageCalculator.CalculateAttackDamage(virtualCastingApply, chainList.Count);
            float directionBonus = virtualCastingApply.GetDamage().relativeModifiers[Resources.Load<Sprite>("Icon/Direction")];
            
            foreach(var chain in chainList)
	            chain.Casting.applies.ForEach(apply => apply.GetDamage().AddModifier(passiveSkill, directionBonus));
        }
    }
}
