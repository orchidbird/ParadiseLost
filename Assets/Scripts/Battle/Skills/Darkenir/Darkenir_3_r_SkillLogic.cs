using Enums;
using UtilityMethods;

namespace Battle.Skills {
    class Darkenir_3_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnUnitDestroy(Unit unit, Unit destroyedUnit, TrigActionType actionType) {
            if(Calculate.Distance(unit.Pos, destroyedUnit.Pos) <= 3 && actionType != TrigActionType.Escape) {
                unit.ChangeAP(5);
            }
        }
    }
}
