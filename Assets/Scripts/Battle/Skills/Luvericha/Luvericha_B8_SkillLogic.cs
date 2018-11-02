using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;
using UnityEngine;
using Enums;

namespace Battle.Skills {
	class Luvericha_B8_SkillLogic : AttachOnStart {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            int numberOfInjuredAlly = 0;
            foreach(var unit in UnitManager.GetAllUnits()){
	            if (!unit.IsAllyTo(caster)) continue;
	            
	            if(unit.GetHpRatio () <= 0.4f) {
		            numberOfInjuredAlly ++;
	            }
            }
            return numberOfInjuredAlly;
        }
    }
}
