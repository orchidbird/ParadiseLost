using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
	public class Yeong_2_r_SkillLogic : AttachOnStart {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            var nearbyUnits = Utility.UnitsInRange(Utility.TilesInDiamondRange(owner.Pos, 1, 3, 1));
            var count = nearbyUnits.Count(nearUnit => nearUnit.IsEnemyTo(owner));
            return count;
        }
    }
}
