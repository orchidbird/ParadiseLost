using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
    public class Eren_DarkArrow_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply){
	        UnitStatusEffect uniqueStatusEffect = castingApply.Caster.GetSEofDisplayNameKor("흡수");

	        if (uniqueStatusEffect == null) return;
	        int stack = uniqueStatusEffect.Stack;
	        castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 0.2f * stack + 1);
        }
    }
}
