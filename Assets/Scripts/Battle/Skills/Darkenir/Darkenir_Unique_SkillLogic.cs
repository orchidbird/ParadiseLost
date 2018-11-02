using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    class Darkenir_Unique_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerAfterStatusEffectAttachedByCasting(UnitStatusEffect statusEffect, Unit target, Unit caster) {
			List<float> amounts = new List<float>();
			foreach (var actual in statusEffect.actuals)
				amounts.Add (actual.amount / 2);
			UnitStatusEffect newStatusEffect = new UnitStatusEffect((UnitStatusEffectInfo)statusEffect.myInfo, caster, caster, statusEffect.GetOriginSkill(), amounts);
			StatusEffector.AttachAndReturnUSE(caster, new List<UnitStatusEffect>{ newStatusEffect }, caster, false);
        }
    }
}
