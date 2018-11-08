using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Enums;
using GameData;
using UtilityMethods;

public class BattleTriggerManager : MonoBehaviour {
    private static BattleTriggerManager instance;
    public static BattleTriggerManager Instance {
        get { return instance; }
    }
	SceneLoader sceneLoader;
	public SceneLoader SceneLoader { get { return sceneLoader; } }
	public ResultPanel resultPanel;
	public List<BattleTrigger> triggers = new List<BattleTrigger>();

	public List<BattleTrigger> ActiveTriggers{
		get{
			return triggers.FindAll(IsTriggerActive);
		}
	}

	public void RevertTriggersCountedByLogs(List<Log> rewoundLogs) {	// 로그를 역재생할 떄, 그 로그들에 의해 count된 trigger들을 다시 되돌린다
		foreach(var log in rewoundLogs)
			foreach(var trig in log.countedTriggers)
				RevertBattleTrigger(trig);
	}

	public void RevertBattleTrigger(BattleTrigger trigger) {
		if(trigger.action == TrigActionType.UnderCount) {
			trigger.count = CountUnitOfCondition(trigger);
			if(CountUnitOfCondition(trigger) > trigger.reqCount)
				RevertActivatedTrigger(trigger);
		} else {
			trigger.count--;
			if(trigger.count < trigger.reqCount)
				RevertActivatedTrigger(trigger);
			else if (trigger.repeatable && trigger.acquired) {
				BattleData.rewardPoint -= trigger.reward;
				trigger.rewardAddedCount--;
			}
		}
	}

	public void CountBattleTrigger(BattleTrigger trigger) {
		Debug.Log(trigger.GetName + "을 Count");
        if( (trigger.repeatable || !trigger.acquired)
    	  &&(trigger.result == TrigResultType.Trigger || trigger.result == TrigResultType.MoveCamera 
			|| trigger.result == TrigResultType.Zoom) ){
            trigger.Trigger();
        }
		if(trigger.action == TrigActionType.UnderCount){
            trigger.count = CountUnitOfCondition(trigger);
			Debug.Log("trigger.count: " + trigger.count + " / " + trigger.reqCount);
            if (CountUnitOfCondition(trigger) <= trigger.reqCount)
				ActivateTrigger(trigger);
		}else{
			trigger.count += 1;
			if (trigger.count == trigger.reqCount)
				ActivateTrigger(trigger);
			else if (trigger.repeatable && trigger.acquired){
				BattleData.rewardPoint += trigger.reward;
                trigger.rewardAddedCount++;
			}
        }
        if (trigger.acquired && trigger.result == TrigResultType.Spawn && (trigger.count - trigger.reqCount) % trigger.period == 0)
            trigger.Trigger();
		
		BattleUIManager.Instance.UpdateSmallConditionTexts();
    }

	void RepeatBattleTrigger(BattleTrigger trigger, int count){
		for(int i = 0; i < count; i++) {CountBattleTrigger(trigger);}
	}

	void ActivateTrigger(BattleTrigger trigger){
		//Debug.Log(trigger.GetName + "을 발동");
		if (trigger.acquired != trigger.reverse) return;
		
		if(trigger.result == TrigResultType.Script) {trigger.Trigger();}
		trigger.acquired = !trigger.reverse;
	}

	void RevertActivatedTrigger(BattleTrigger trigger) {
		if(trigger.acquired != trigger.reverse) {
			trigger.acquired = trigger.reverse;
			Debug.Log("Revert Activated Trigger : " + trigger.GetName + " / " + trigger.acquired);
		}else Debug.Log(trigger.GetName + " is not Activated Yet");
	}

	public IEnumerator EndBattle(bool victory){
		//CheckExtraTriggersAtWinGame();
        BattleUIManager.Instance.SlideUIsOut(Setting.slideUISlowDuration);
		resultPanel.gameObject.SetActive(true);
		resultPanel.Initialize();

		if(victory) yield return resultPanel.PrintWinResult();
		else yield return resultPanel.PrintLoseResult();
	}

	//게임 종료시에 한꺼번에 체크해야 하는 트리거.
	/*void CheckExtraTriggersAtWinGame(){
		var exTrigs = triggers.FindAll(trig => trig.action == TrigActionType.Extra);
		var units = UnitManager.GetAllUnits();
		StageNum stage = VolatileData.progress.stageNumber;
		var repeatCounts = new Dictionary<BattleTrigger, int>(); //반복성 Extra Trigger의 반복 횟수
		var acqBools = new Dictionary<BattleTrigger, bool>(); //일회성 Extra Trigger의 acquire 여부

		if (exTrigs.Count == 0) return;
		
		if (stage == StageNum.S1_1){
			Unit damagedPC = null;
			acqBools.Add(exTrigs[0], true);
			foreach (var log in LogsOfType<HPChangeLog>()){
				if (!log.target.IsPC) continue;
				if (damagedPC == null)
					damagedPC = log.target;
				else if (damagedPC != log.target) {
					acqBools[exTrigs[0]] = false;
					break;
				}
			}

			var skillUsedPCs = new List<Unit>();
			foreach (var log in LogsOfType<CastLog>())
				if (log.actor.IsPC && !skillUsedPCs.Contains(log.actor))
					skillUsedPCs.Add(log.actor);
		}else if (stage == StageNum.S5_1) {
			var DamagedPC = new List<Unit>(); //피해를 입은 PC 명단(List)
			LogsOfType<HPChangeLog>().ForEach(log => {
				if (log.target.IsPC && DamagedPC.All(unit => unit != log.target)) {
					DamagedPC.Add(log.target);
				}
			});
			acqBools.Add(exTrigs[0], DamagedPC.Count <= 1);//1명 이하일 때 발동
		} else if (stage == StageNum.S7_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<HPChangeLog>().FindAll(log => (log.target.CodeName.StartsWith("child") && log.amount >= 50)
			                                                                      && log.parentEvent is CastLog && ((CastLog) log.parentEvent).actor.IsPC).Count);
		} else if (stage == StageNum.S8_1) {
			//남은 유닛 중 동료 NPC 1명마다 적용
			repeatCounts.Add(exTrigs[0], units.FindAll(unit => unit.GetSide() == Side.Ally && unit.IsAI).Count);
		} else if (stage == StageNum.S9_1) {
			Unit remainEnemy = units.Find(unit => unit.GetSide() == Side.Enemy);
			acqBools.Add(exTrigs[0], (remainEnemy == null || remainEnemy.GetHpRatio() < 0.5f));

			acqBools.Add(exTrigs[1], LogsOfType<StatusEffectLog>().Any(log => log.statusEffect is UnitStatusEffect
			                                                                  && log.statusEffect.GetCaster().CodeName == "sepia"
			                                                                  && ((UnitStatusEffect) log.statusEffect).GetOwner().CodeName == "sepia"
			                                                                  && log.type == StatusEffectChangeType.AmountChange
			                                                                  && log.GetShieldChangeAmount() < 0));

		} else if (stage == StageNum.S12_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<HPChangeLog>().FindAll(log => log.actor.GetSide() == Side.Ally && log.damage.relativeModifiers.Any(kv => kv.Key.name == "Height" && kv.Value > 1)).Count); //HeightModifier > 1.0f)).Count);
		} else if (stage == StageNum.S13_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<HPChangeLog>().FindAll(log => log.actor.GetSide() == Side.Ally && log.damage.relativeModifiers.Any(kv => kv.Key.name == "Height" && kv.Value > 1)).Count);
		}  else if (stage == StageNum.S14_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<StatusEffectLog>().FindAll(log => log.statusEffect.GetCaster().CodeName == "armorSchmidt"
			                                                                          && log.type == StatusEffectChangeType.Attach && log.statusEffect.IsTypeOf(StatusEffectType.EnemyAura)).Count);
		}else if (stage == StageNum.S15_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<StatusEffectLog>().FindAll(log => log.statusEffect.GetCaster().CodeName == "bianca" && log.statusEffect.DisplayName(true) == "속박").Count);
			repeatCounts.Add(exTrigs[1], LogsOfType<PositionChangeLog>().FindAll(log => log.actor.codeName == "arcadia" && log.forced).Count);
		} else if (stage == StageNum.S16_1) {
			repeatCounts.Add(exTrigs[0], units.FindAll(unit => unit.GetSide() == Side.Neutral && unit.GetHpRatio() >= 1 && !unit.IsObject).Count);
		} else if (stage == StageNum.S18_1) {
			repeatCounts.Add(exTrigs[0], LogsOfType<CastLog>().FindAll(log =>
				log.casting.Caster.GetSide() == Side.Enemy && log.casting.GetTargets().Any(unit => unit.GetSide() == Side.Ally)
				                                           && log.casting.GetTargets().Any(unit => unit.CodeName == "carriage01")).Count);
		}else if (stage == StageNum.S20_1){
			repeatCounts.Add(exTrigs[0], LogsOfType<CastLog>().FindAll(log => log.actor.IsPC && log.casting.applies.Any(apply => apply.GetDamage().HasTacticalBonus)).Count);
		}
		for(int i = 0; i < exTrigs.Count; i++){
			if (!exTrigs[i].repeatable) {
				if (acqBools[exTrigs[i]])
					CountBattleTrigger(exTrigs[i]);
			} else RepeatBattleTrigger(exTrigs[i], repeatCounts[exTrigs[i]]);
		}
	}*/

	List<T> LogsOfType<T>(){
		List<T> result = new List<T>();
		BattleData.logDisplayList.ForEach(dsp => {
			if(dsp.log is T){
				result.Add((T)Convert.ChangeType(dsp.log, typeof(T)));
			}
		});
		return result;
	}

	void Awake(){
        instance = this;
	}
	void Start () {
        BattleUIManager.Instance.resultPanel.Checker = this;
        sceneLoader = FindObjectOfType<SceneLoader>();
	}

	public void CountTriggers(TrigActionType actionType, Unit actor = null, string subType = "", Unit target = null, Log log = null, Tile dest = null){
		SteamAchievementManager.Instance.CheckOn(actionType, actor, target, dest);

		List<BattleTrigger> availableTriggers = ActiveTriggers.FindAll(trig => 
			CheckUnitType(trig, actor, TrigUnitCheckType.Actor) && CheckUnitType(trig, target, TrigUnitCheckType.Target)
			&& CheckActionType(trig, actionType) && trig.logs.All(item => item != log)
		); //Actor, Target, Action이 모두 일치하고 && 아직 해당 log에 의해 기록되지 않은 경우

		var targetTriggers = dest != null ?
			availableTriggers.FindAll(trig => trig.targetTiles.Contains(dest.location)) : availableTriggers.FindAll(trig => CheckSubType(trig, subType));
			
		//Escape의 경우를 예외 처리.
		targetTriggers.ForEach(trig => {
			if (log != null) {
				trig.logs.Add(log);
				log.countedTriggers.Add(trig);
			}
            CountBattleTrigger(trig);
		});
	}

	//트리거가 현재 활성화되어있는지 여부를 확인.
	//relation == Sequence이고 앞번째 트리거가 달성되지 않은 경우만 false, 그 외에 전부 true.
	public bool IsTriggerActive(BattleTrigger trigger){
		if(trigger.result == TrigResultType.Info){
			return false;
		}else if(trigger.result == TrigResultType.Bonus){
			return true;
		}else{
			List<BattleTrigger> checkList;
			BattleTrigger.TriggerRelation relation;
			if(trigger.result == TrigResultType.Win){
				checkList = triggers.FindAll(trig => trig.result == TrigResultType.Win);
				relation = triggers.Find(trig => trig.result == TrigResultType.Info).winTriggerRelation;
			}else{
				checkList = triggers.FindAll(trig => trig.result == TrigResultType.Lose);
				relation = triggers.Find(trig => trig.result == TrigResultType.Info).loseTriggerRelation;
			}

			if(relation == BattleTrigger.TriggerRelation.Sequence){
				for(int i = 0; i < checkList.Count; i++)
					if(checkList[i] == trigger)
						return i == 0 || checkList[i-1].acquired;
				
				Debug.LogError("checkList에 trigger가 없습니다!");
				return false;
			}
			
			return true;
		}
	}

	public int CountUnitOfCondition(BattleTrigger trigger){
		return UnitManager.GetAllUnits().Count(unit => CheckUnitType(trigger, unit, TrigUnitCheckType.Actor));
	}

	public bool CheckUnitType(BattleTrigger trigger, Unit unit, TrigUnitCheckType checkType){
		TrigUnitType unitType = trigger.target;
		List<string> names = trigger.targetUnitNames;
		if(checkType == TrigUnitCheckType.Actor){
			unitType = trigger.actor;
			names = trigger.actorUnitNames;
		}

		return (unit == null || unitType == TrigUnitType.Any) //TrigActionType.Phase 등 명시적인 행위주체가 없는 경우
			|| unitType == TrigUnitType.Name && names.Any (x => _String.Match(x, unit.CodeName))
			|| (unitType == TrigUnitType.Ally && unit.GetSide() == Side.Ally)
			|| (unitType == TrigUnitType.Enemy && unit.GetSide() == Side.Enemy)
			|| (unitType == TrigUnitType.PC && unit.IsPC && unit.GetSide() == Side.Ally)
			|| (unitType == TrigUnitType.NeutralChar && !unit.IsObject  && unit.GetSide() == Side.Neutral)
			|| (unitType == TrigUnitType.EnemyChar && !unit.IsObject && unit.GetSide() == Side.Enemy)
			|| (unitType == TrigUnitType.AllyNPC && unit.IsAllyNPC);
	}

	bool CheckActionType(BattleTrigger trigger, TrigActionType actionType){
		return trigger.action == actionType;
	}

	bool CheckSubType(BattleTrigger trigger, string subType){
		if(trigger.action == TrigActionType.MultiAttack){
			return int.Parse(subType) >= int.Parse(trigger.subType);
		}else{
			return trigger.subType == subType;
		}
	}
}
