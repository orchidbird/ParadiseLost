using System.Collections.Generic;
using System.Linq;
using UtilityMethods;

namespace Battle.Skills {
	public class Curi_7_l_SkillLogic : AttachOnStart {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            List<Unit> unitsExceptThis = new List<Unit>();
            foreach(var unit in UnitManager.GetAllUnits()) {
                if(unit != owner) 
                    unitsExceptThis.Add(unit);
            }
            int distance = 0;
            if(unitsExceptThis.Count != 0)
                distance = unitsExceptThis.Min(x => Calculate.Distance(owner.Pos, x.Pos));
            return distance;
        }
    }
}
