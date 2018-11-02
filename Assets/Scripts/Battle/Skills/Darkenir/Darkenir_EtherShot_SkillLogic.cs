using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Damage;
using UnityEngine;

namespace Battle.Skills {
    class Darkenir_EtherShot_SkillLogic : BaseActiveSkillLogic {
	    public static Dictionary<Tile, int> tileData = new Dictionary<Tile, int>();
	    public static Dictionary<Unit, int> unitData = new Dictionary<Unit, int>();

	    void AddUnit(Unit target, int extensionCount){
		    if (target == null || unitData.ContainsKey(target)) return;
		    unitData.Add(target, extensionCount);
	    }
	    public override void ActionInDamageRoutine(CastingApply castingApply){
		    var Caster = castingApply.Caster;
		    tileData.Clear();
		    unitData.Clear();
		    AddUnit(castingApply.Target, 0);
		    tileData.Add(castingApply.Target.TileUnderUnit, 0);

		    for (int i = 1; i <= 3; i++){
			    foreach (var kv in tileData){
				    if(!kv.Key.IsUnitOnTile() || kv.Key.GetUnitOnTile().IsObject) continue;
				    foreach (var dir in EnumUtil.directions){
					    var newTile = TileManager.Instance.GetTile(kv.Key.Location + Utility.DirToV2I(dir));
					    if(newTile != null)
						    AddUnit(newTile.GetUnitOnTile(), i);
				    }
			    }

			    foreach (var kv in unitData)
				    foreach (var tile in kv.Key.TilesUnderUnit)
					    if(!tileData.ContainsKey(tile))
						    tileData.Add(tile, i);
		    }

		    foreach (var kv in unitData){
			    if(kv.Value == 0) continue;
			    var damage = castingApply.GetSkill().powerFactor * Caster.GetStat(Stat.Power) * Math.Pow(0.5f, kv.Value);
			    kv.Key.ApplyDamageByNonCasting((float)damage, Caster, true);
		    }

		    if (unitData.Count >= 2 && Caster.GetPassiveSkillList().Any(passive => passive.korName == "공허의 순환"))
			    StatusEffector.FindAndAttachUnitStatusEffectsOfPrecalculatedAmounts(Caster, activeSkill, Caster,
				    new List<List<float>> {new List<float> {Caster.GetStat(Stat.Power) * 0.2f * unitData.Count}});
	    }
	    
	    public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
		    return false;
	    }
    }
}
