using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using System.Text;
using Battle.Damage;
using Battle.Skills;
using Steamworks;

class Aura{
    //각 unit이 target이 가진 오오라 범위 안에 있을 경우 true 태그를 닮.
    //Aura는 StatusEffect.amount가 범위를 의미한다.
	private static Dictionary<Unit, bool> TagUnitInRange(Vector2Int ownerPos, StatusEffect aura){
        Dictionary<Unit, bool> unitDictionary = new Dictionary<Unit, bool>();
		var auraRange = new List<Vector2Int>();
        // Unit이 가지고 있는 Aura는 DiamondRange, Tile이 가지고 있는 Aura(마법진)은 SquareRange
        // 추후에 아닌 예시가 등장하면 csv를 통해 입력하도록 바꿔야 함
        if(aura is UnitStatusEffect)
            auraRange = Utility.TilesInDiamondRange(ownerPos, 0, (int)aura.GetAmountOfType(StatusEffectType.Aura), 1).ConvertAll(tile => tile.Pos);
        else if(aura is TileStatusEffect)
            auraRange = Utility.GetSquareRange(ownerPos, 0, (int)aura.GetAmountOfType(StatusEffectType.Aura));
    
		Skill skill = aura.GetOriginSkill ();
		BaseCommonSkillLogic skillLogic;
		if(skill is ActiveSkill)
			skillLogic = ((ActiveSkill) skill).SkillLogic;
		else
			skillLogic = ((PassiveSkill) skill).SkillLogic;

		foreach (var unit in UnitManager.GetAllUnits()) {
			unitDictionary.Add (unit, skillLogic.IsAuraTarget(unit) && auraRange.Contains (unit.Pos)
			                          && (skill is PassiveSkill || !((ActiveSkill)skill).SkillLogic.IgnoredBySpecialAuraCondition (unit, aura)));
		}
		if (skill is ActiveSkill)
			((ActiveSkill)skill).SkillLogic.AfterAuraEffectedCounting (aura);
        return unitDictionary;
    }

	static Dictionary<Unit, Dictionary<Skill, int>> stackDic = new Dictionary<Unit, Dictionary<Skill, int>> ();
	static Dictionary<Skill, Unit> auraCasterInfo = new Dictionary<Skill, Unit> ();

	public static void UpdateAllAuraUnitEffects(List<Unit> allUnits){
		stackDic = new Dictionary<Unit, Dictionary<Skill, int>> ();
		auraCasterInfo = new Dictionary<Skill, Unit> ();
		foreach (var unit in allUnits)
			foreach (var statusEffect in unit.statusEffectList)
				if (statusEffect.IsTypeOf (StatusEffectType.Aura))
					UpdateStackDicByAnAura (statusEffect, unit.Pos);
		
		UpdateEffectsUsingStackDic ();
	}

	public static void UpdateAllAuraOnTileEffects(List<Tile> allTiles){
		stackDic = new Dictionary<Unit, Dictionary<Skill, int>> ();
		auraCasterInfo = new Dictionary<Skill, Unit> ();
		foreach (var tile in allTiles) {
			foreach (var statusEffect in tile.StatusEffectList) {
				if (statusEffect.IsTypeOf (StatusEffectType.Aura)) {
					UpdateStackDicByAnAura (statusEffect, tile.location);
				}
			}
		}
		UpdateEffectsUsingStackDic ();
	}

	static void UpdateEffectsUsingStackDic(){
		foreach (var kv in stackDic) {
			Unit unit = kv.Key;
			foreach (var skillStackKV in kv.Value){
				Skill skill = skillStackKV.Key;
				StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(auraCasterInfo [skill], unit, skill, skillStackKV.Value, byAura: true);
			}
		}
	}

	static void UpdateStackDicByAnAura(StatusEffect aura, Vector2Int ownerPos){
		Skill originSkill = aura.GetOriginSkill();
		if (!auraCasterInfo.ContainsKey (originSkill))
			auraCasterInfo [originSkill] = aura.GetCaster ();
        Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(ownerPos, aura);
        foreach (var kv in unitInRangeDictionary){
			Unit unit = kv.Key;
			if (!stackDic.ContainsKey (unit))
				stackDic [unit] = new Dictionary<Skill, int> ();
			if (!stackDic [unit].ContainsKey (originSkill))
				stackDic [unit] [originSkill] = 0;
			if (kv.Value)
				stackDic [unit] [originSkill] += aura.Stack;
        }
    }

	public static void TriggerOnAuraRemoved(Unit auraOwner, StatusEffect aura){
		Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange (auraOwner.Pos, aura);
		foreach (var kv in unitInRangeDictionary) {
			if (!kv.Value) continue;
			
			var seToRemove = kv.Key.statusEffectList.Find (se => se.GetOriginSkill () == aura.GetOriginSkill () && !se.IsAura ());
			if (seToRemove != null)
				seToRemove.DecreaseRemainStack (1); // 1이었는데 0이 되었다면 잘 없어질 거고, 2 이상이었는데 1 줄어들면 어차피 다음 UpdateAllAuraUnitEffects에서 맞는 값으로 갱신될 것이니 상관없음
		}
	}
}
