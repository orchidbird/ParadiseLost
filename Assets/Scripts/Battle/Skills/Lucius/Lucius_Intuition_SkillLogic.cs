using UnityEngine;
using UtilityMethods;

namespace Battle.Skills {
    class Lucius_Intuition_SkillLogic : BasePassiveSkillLogic {
        public override bool IgnoreCasting(CastingApply apply, int chainCombo){
	        //Debug.Log(caster.CodeName + caster.Pos + " / " + target.CodeName + target.Pos + " / 거리: " + Calculate.Distance(caster, target));
	        return apply.GetSkill().IsOffensive() && Calculate.Distance(apply.Caster, apply.Target) <= 1;
        }
    }
}
