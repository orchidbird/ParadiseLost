using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
	public class Eren_5_l_SkillLogic : AttachOnStart {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
			return UnitManager.GetAllUnits ().Count (x => x.GetSide () == caster.GetEnemySide ());
        }
    }
}
