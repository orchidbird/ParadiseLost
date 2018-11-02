using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills
{
public class Lenien_A1_SkillLogic : BaseActiveSkillLogic {
	public override bool CheckApplyPossibleToTargetTiles(Casting casting){
		return casting.Location.TargetTile.IsUnitOnTile();
	}
	// 조건부 기절 추가.
	public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
		return castingApply.Target.TileUnderUnit != castingApply.GetCasting().Location.TargetTile;
	}
}
}
