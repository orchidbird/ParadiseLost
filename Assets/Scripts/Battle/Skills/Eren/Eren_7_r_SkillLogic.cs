using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
	public class Eren_7_r_SkillLogic : AttachOnStart {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            UnitStatusEffect AbsorptionStatusEffect = owner.statusEffectList.Find(se => se.DisplayName(true) == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.Stack;
            return stack;
        }
    }
}
