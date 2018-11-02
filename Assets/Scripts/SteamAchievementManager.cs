using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;
using Steamworks;
using Enums;
using System;
using System.Linq;

public class SteamAchievementManager : MonoBehaviour {
	static SteamAchievementManager instance;
	public static SteamAchievementManager Instance {
		get{
			return instance;
		}
	}

	public void Awake() {
		instance = this;
	}

	public void CheckOn(TrigActionType actionType, Unit actor, Unit target, Tile tile) {
		IEnumerable<Log> logs = BattleData.logDisplayList.Select(disp => disp.log);
		if (actionType == TrigActionType.Escape) {
			// 4장에서 티엔이 자신 이외의 아군에게 이로운 효과를 한 번도 걸지 않고 맨 먼저 탈출
			if(VolatileData.progress.stageNumber == StageNum.S4_1) {
				if( // 티엔이 탈출
					actor.CodeName == "tien" &&
					// 최초로 탈출
					BattleTriggerManager.Instance.triggers.Find(trigger => trigger.action == TrigActionType.Escape).count == 0 &&
					// 아군에게 이로운 효과를 한 번도 걸지 않음
					!logs.Any(log => {
						if(!(log is CastLog))
							return false;
						Casting casting = ((CastLog)log).casting;
						return casting.Caster == actor && casting.GetTargets().Any(t => t.GetSide() == Side.Ally && t != actor) 
							&& casting.Skill.GetSkillApplyType() == SkillApplyType.Buff;
					})) {
					
					Achieve("Not my bussiness");
				}
			}
		}

		if (actionType == TrigActionType.Neutralize) {
			if(actor.IsPC && target.GetSide() == Side.Enemy)
				RecordData.neutralizeEnemyCount++;
			if (RecordData.neutralizeEnemyCount >= 3)
				Achieve("Battle Beginner");
			if(RecordData.neutralizeEnemyCount >= 200)
				Achieve("Battle Expert");
			if(RecordData.neutralizeEnemyCount >= 800)
				Achieve("Battle Specialist");
			// 도주 스테이지에서 모든 적을 격파
			if(VolatileData.stageData.GetBattleTriggers().Any(trig => trig.result == TrigResultType.Win && trig.action == TrigActionType.Escape)) {
				List<Unit> enemyUnits = UnitManager.GetAllUnits().FindAll(u => u.GetSide() == Side.Enemy);
				if(enemyUnits.Count == 0 || (enemyUnits.Count == 1 && enemyUnits[0] == target))
					Achieve("No witness alive");
			}
			// 한 스테이지에서 한 캐릭터로 8명 이상 이탈
			if(logs.Count(log => log is UnitDestroyLog && ((UnitDestroyLog)log).actor == actor) >= 8)
				Achieve("LEGENDARY!");
		}

		if (actionType == TrigActionType.Damage) {
			if (RecordData.critCount >= 100)
				Achieve("Crit Beginner");
			if (RecordData.critCount >= 500)
				Achieve("Crit Expert");
			if (RecordData.critCount >= 1000)
				Achieve("Crit Specialist");
		}
	}

	public void CheckOnLose() {
		if(RecordData.stageClearRecords.Sum(kv => kv.Value.Count(record => record.star == -1)) >= 10)
			Achieve("Homer's tenth nods");
	}

	public void CheckOnClear(int star) {
		IEnumerable<Log> logs = BattleData.logDisplayList.Select(disp => disp.log);
		// 일반 스테이지 클리어 업적
		Achieve("Clear_" + ((int)VolatileData.progress.stageNumber / 10).ToString());
		if(star == 3)
			Achieve("Clear_" + ((int)VolatileData.progress.stageNumber / 10).ToString() + "_3_Star");

		// 2스테이지 제어장치를 가장 먼저 부수고 클리어하는 업적
		if (VolatileData.progress.stageNumber == StageNum.S2_1) {
			foreach(var log in logs) {
				if(log is UnitDestroyLog) {
					Unit destroyed = ((UnitDestroyLog)log).target;
					if(destroyed.GetSide() == Side.Enemy) {
						if (destroyed.IsObject) {
							Achieve("Not that controller");
							break;
						} else break;
					}
				}
			}
		}

		// 6스테이지 어떠한 적도 이탈시키지 않고 클리어
		if (VolatileData.progress.stageNumber == StageNum.S6_1) {
			if(!logs.Any(log => log is UnitDestroyLog && ((UnitDestroyLog)log).target.GetSide() == Side.Enemy))
				Achieve("Environmentalist");
		}

		// 도주 스테이지에서 적을 한 명도 제거하지 않음
		if (VolatileData.stageData.GetBattleTriggers().Any(trig => trig.result == TrigResultType.Win && trig.action == TrigActionType.Escape)
				&& !logs.Any(log => log is UnitDestroyLog && ((UnitDestroyLog)log).target.GetSide() == Side.Enemy))
			Achieve("Ahimsa");

		// 사기가 10 이하인 캐릭터 존재 중 승리
		if (UnitManager.GetAllUnits().Any(u => u.actualStats[Stat.Will] <= 10))
			Achieve("Victory Anyway");

		// 19장 핀토스/하스켈 진영 클리어 업적
		if (VolatileData.progress.stageNumber == StageNum.S19_1) {
			if (BattleData.selectedFaction == Faction.Pintos)
				Achieve("Clear_19_Pintos");
			else Achieve("Clear_19_Haskell");
		}

		int allstageClearedDifficulty = (int)EnumUtil.hardest;
		foreach(StageNum stage in Enum.GetValues(typeof(StageNum))) {
			if(stage == StageNum.Invalid)
				continue;
			int maxDifficulty = (int) EnumUtil.easiest - 1;
			if (RecordData.stageClearRecords.ContainsKey(stage)) {
				foreach (var record in RecordData.stageClearRecords[stage]) {
					if (record.star != -1 && (int) record.difficulty > maxDifficulty) {
						maxDifficulty = (int) record.difficulty;
						if (maxDifficulty == (int) EnumUtil.hardest)
							break;
					}
				}
			}
			if(maxDifficulty < allstageClearedDifficulty)
				allstageClearedDifficulty = maxDifficulty;
			if(allstageClearedDifficulty < (int)EnumUtil.easiest)
				break;
		}
		if (allstageClearedDifficulty >= (int) Difficulty.Intro) {
			Achieve("Clear_Intro");
			Achieve("Clear_First_Chapter");
		}
		if(allstageClearedDifficulty >= (int)Difficulty.Adventurer)
			Achieve("Clear_Adventurer");
		if(allstageClearedDifficulty >= (int)Difficulty.Tactician)
			Achieve("Clear_Tactician");
		if(allstageClearedDifficulty >= (int)Difficulty.Legend)
			Achieve("Clear_Legend");
	}

	public void Achieve(string ID) {
		bool achieved;
		Debug.Log("Steam Achievement " + ID + " achieved!");

		if(!SteamManager.Initialized)
			return;

		SteamUserStats.GetAchievement(ID, out achieved);
		if (!achieved) {
			SteamUserStats.SetAchievement(ID);
			SteamUserStats.StoreStats();
		}
	}
	public void UnAchieve(string ID) {	// 디버깅용
		bool achieved;
		if (!SteamManager.Initialized)
			return;
		SteamUserStats.GetAchievement(ID, out achieved);

		if (achieved) {
			SteamUserStats.ClearAchievement(ID);
		}
	}
}
