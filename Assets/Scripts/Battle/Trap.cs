using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Battle.Damage;
using Battle.Skills;

class Trap {
    // 트랩 스킬로직에서 쓸 함수를 모아둔 모듈
    public static bool TriggerOnApplied(TileStatusEffect tileStatusEffect){
	    return tileStatusEffect.IsTypeOf(StatusEffectType.Trap);
    }
    public static void OperateTrap(TileStatusEffect trap) {  // 발동
        List<Unit> unitsInRange = GetUnitsInRange(trap);
        foreach (var unit in unitsInRange) {
			ActiveSkill originSkill = (ActiveSkill)trap.GetOriginSkill ();
			if(originSkill is ActiveSkill)
				StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets (trap.GetCaster(), unit, originSkill, 1, add: true);
        }
		if (unitsInRange.Count > 0)
			trap.GetOwnerTile().RemoveStatusEffect (trap);
        LogManager.Instance.Record(new SoundEffectLog("OperateTrap"));
        foreach (var unit in UnitManager.GetAllUnits())
            unit.GetListPassiveSkillLogic().TriggerOnTrapOperated(unit, trap);
    }
    public static EventLog Update(TileStatusEffect trap) {
		Unit unitOnTrap = trap.GetOwnerTile ().GetUnitOnTile ();
	    if (unitOnTrap == null || !unitOnTrap.GetListPassiveSkillLogic().TriggerOnSteppingTrap(unitOnTrap, trap)) return null;
	    EventLog trapOperatedLog = new TrapOperatedLog (trap);
	    LogManager.Instance.Record (trapOperatedLog);
	    OperateTrap (trap);
	    return trapOperatedLog;
    }

    private static List<Tile> GetTilesInRange(TileStatusEffect trap) { // 발동범위가 아니라 효과범위
        Tile tile = trap.GetOwnerTile();
        List<Vector2Int> rangeTiles = Utility.GetSquareRange(tile.Location, 0, (int)trap.GetAmountOfType(StatusEffectType.Trap));
        return TileManager.V2ToTiles(rangeTiles);
    }
    private static List<Unit> GetUnitsInRange(TileStatusEffect trap) {
        List<Unit> unitsInRange = new List<Unit>();
        List<Tile> trapRange = GetTilesInRange(trap);
        foreach (var tile in trapRange) {
            if (tile.IsUnitOnTile())
                unitsInRange.Add(tile.GetUnitOnTile());
        }
        return unitsInRange;
    }
}
