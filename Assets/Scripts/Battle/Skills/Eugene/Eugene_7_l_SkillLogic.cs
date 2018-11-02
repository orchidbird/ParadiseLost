using Enums;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Eugene_7_l_SkillLogic : BasePassiveSkillLogic {
        //실제로 약화가 사라지는 부분은 '청명수의 축복' 스킬 로직도 참조.
        public override void TriggerOnStageStart(Unit caster) {
            UnitStatusEffectInfo statusEffectInfo = StatusEffector.USEInfoList.Find(seInfo =>
                seInfo.GetOriginSkillName() == "청명수의 축복");
            var actual = new StatusEffect.ActualElement(StatusEffectType.Etc, new Formula(FormulaVarType.None, 0, 0), false, false);
            List<StatusEffect.ActualElement> actuals = statusEffectInfo.actuals;
            if(actuals.Count == 1)
                actuals.Add(null);
            actuals.Add(actual);
        }
    }
}
