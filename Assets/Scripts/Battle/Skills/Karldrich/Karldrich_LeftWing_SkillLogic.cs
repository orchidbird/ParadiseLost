using UnityEngine;
using UtilityMethods;

namespace Battle.Skills {
    class Karldrich_LeftWing_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply){
            Unit target = castingApply.Target;
	        if (target.IsObject) return;
	        
            Vector2 targetFront = Utility.DirToV2I(target.GetDir());
            Vector2 targetLeft = Utility.Rotate(targetFront, Mathf.PI / 2);
            if(Calculate.NormalizeV2I(castingApply.Caster.Pos - target.Pos) == targetLeft)
	            castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 1.15f);
        }
    }
}
