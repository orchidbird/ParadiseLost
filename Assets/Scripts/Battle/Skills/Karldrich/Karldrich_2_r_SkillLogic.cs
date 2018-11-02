using UnityEngine;
using Enums;
using Battle.Damage;
using System;
using System.Collections.Generic;

namespace Battle.Skills {
    class Karldrich_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            List<Unit> units = Utility.UnitsInRange(Utility.TilesInDiamondRange(caster.Pos, 1, 2, 1));
			int count = units.FindAll(unit => unit.IsAllyTo(caster)).Count;
			StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(caster, caster, passiveSkill, count);
        }
    }
}
