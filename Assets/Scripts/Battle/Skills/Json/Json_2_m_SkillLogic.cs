using Enums;
using System.Collections.Generic;
using System;
using Battle.Damage;

namespace Battle.Skills {
    class Json_2_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            UnitStatusEffectInfo statusEffectInfo = StatusEffector.USEInfoList.Find(seInfo =>
                seInfo.GetOriginSkillName() == "집중 공격" && seInfo.displayKor == "표식");
            var actual1 = new StatusEffect.ActualElement(StatusEffectType.DamageOverPhase, new Formula(FormulaVarType.Power, 0.2f, 0), false, false);
            List<StatusEffect.ActualElement> actuals = statusEffectInfo.actuals;
			if(actuals.Count <= 1)
				actuals.Add(actual1);
			else actuals[1] = actual1;
        }
    }
}
