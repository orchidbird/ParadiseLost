using Enums;

namespace Battle.Skills {
    class Noel_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnUnitDestroy(Unit unit, Unit destroyedUnit, TrigActionType actionType) {
            if (unit.IsEnemyTo(destroyedUnit) && actionType == TrigActionType.Retreat) {
                foreach (var otherUnit in UnitManager.GetAllUnits())
                    if (unit.IsAllyTo(otherUnit))
                        otherUnit.ChangeAP((int)(unit.GetStat(Stat.Agility) * 0.1f));
            }
        }
    }
}
