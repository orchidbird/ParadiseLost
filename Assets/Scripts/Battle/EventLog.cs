using Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Turn;
using UnityEngine;
using GameData;

//EffectLog와 달리 어떤 일이 발생했다는 사실만 기록.
public class EventLog : Log {
    public Unit actor;
    public List<EffectLog> effectLogList = new List<EffectLog>();  // 이 Event로부터 발생한 Effect들
    public List<EffectLog> GetEffectLogList() { return effectLogList; }
	public bool FullExecuted{get { return executed && GetEffectLogList().All(log => log.executed); }}
	public void SetActor(Unit unit){actor = unit;}
	
    public override IEnumerator Execute(){
        var BTM = BattleTriggerManager.Instance;
        for (int i = 0; i < effectLogList.Count; i++) {
	        
            var effectLog = effectLogList[i];
	        if (effectLog.executed) continue;
	        //Debug.Log(effectLog.GetText() + "를 실행");
	        effectLog.executed = true;

			if (effectLog is VisualEffectLog || HasAfterLogOfSameType<UnitDestroyLog>(i) || HasAfterLogOfSameType<DisplayDamageTextLog>(i) || 
			    HasAfterLogOfSameType<PositionChangeLog>(i) && BattleData.IsUserTurn)
		        BTM.StartCoroutine(effectLog.Execute());
	        else
				yield return effectLog.Execute();
	        UnitManager.Instance.ActivateGuard(effectLog.tiles);

			if (effectLog.logDisplay != null)
				effectLog.CreateExplainObjects();
        }

	    if(!executed)
	    	AfterExecute();
    }

	bool HasAfterLogOfSameType<T>(int index){
		if (!(effectLogList[index] is T)) return false;
		for (int i = index+1; i < effectLogList.Count; i++)
			if (effectLogList[i] is T)
				return true;
		return false;
	}
	protected virtual void AfterExecute(){}
	
	public override void Rewind() {
		List<EffectLog> effectLogs = GetEffectLogList();
		effectLogs.Reverse();

		foreach (var log in effectLogs) {
			//Debug.Log("Rewind : " + log.GetText());
			log.Rewind();
		}
	}
}

public class BattleStartLog : EventLog {
    public override string GetText() {
        return "전투 시작";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateText("전투 시작");
    }

	protected override void AfterExecute(){
		BattleTriggerManager.Instance.CountTriggers(TrigActionType.Start, log: this);
		//UnitManager.Instance.ApplyUSEsAtBattleStart();
		UnitManager.Instance.CheckVigilAreaForAI();
	}
}

public class MoveLog : EventLog{
	public TileWithPath tileWithPath;
	public Vector2Int dest{get { return tileWithPath.dest.Pos; }}
	public float moveCost;
	public bool sightChanged = false; // 이 이동으로 인해 안 보였던 타일이 보이게 되었는지를 나타내는 변수. true라면 이동 취소가 불가능함

    public MoveLog(Unit unit, TileWithPath tileWithPath){
	    SetActor(unit);
	    this.tileWithPath = tileWithPath;
    }

    public override string GetText() {
        return actor.GetNameKor() + " : " + tileWithPath.fullPath.First().Pos + "에서 " + dest + "(으)로 이동";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(actor);
        logDisplay.CreateText("이동");
    }

	protected override void AfterExecute(){
		actor.ChangePosition(tileWithPath.fullPath);
		actor.UseActivityPoint(tileWithPath.RequireActivityPoint);
		LogManager.Instance.Record(new AddMoveOverloadLog(actor, tileWithPath.fullPath.Count-1, moveCost));
		    
		BattleTriggerManager.Instance.CountTriggers(TrigActionType.Escape, actor, dest: TileManager.Instance.GetTile(dest), log: this);
		BattleTriggerManager.Instance.CountTriggers(TrigActionType.StepOnTile, actor, TileManager.Instance.GetTile(dest).displayName, log: this);
	}
}

public class ChainLog : EventLog {  // 연계 대기
    public Casting casting;
    public ChainLog(Casting casting) {
        this.casting = casting;
        SetActor(casting.Caster);
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill activeSkill = casting.Skill;
        return caster.GetNameKor() + " : " + activeSkill.GetName() + " 연계 대기";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(actor);
        logDisplay.CreateImage(VolatileData.GetIcon(IconSprites.Chain));
        logDisplay.CreateSkillImage(casting.Skill, actor);
    }
}

public class CastLog : EventLog {   // 스킬 사용
    public bool triggered = false;  // 스킬 사용 후 발동하는 패시브 효과 등이 이미 발동했는지
    public bool IsInitiator{get { return actor == BattleData.turnUnit; }}
    public Casting casting;
    public CastLog(Casting casting){
	    SetActor(casting.Caster);
        this.casting = casting;
    }
    public override string GetText() {
        ActiveSkill activeSkill = casting.Skill;
        return actor.GetNameKor() + " : " + activeSkill.GetName() + " 사용";
    }
    
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(actor);
        logDisplay.CreateSkillImage(casting.Skill, casting.Caster);
    }

	public override IEnumerator Execute(){
		var BTM = BattleTriggerManager.Instance;
		
		for(int i = 0; i < effectLogList.Count; i++){
			var effectLog = effectLogList[i];
			if(effectLog.executed || effectLog is UnitDestroyLog || effectLog is CameraMoveLog || effectLog is HPChangeLog || effectLog is DisplayDamageTextLog) continue;
			effectLog.executed = true;
			//Debug.Log(casting.Skill.korName + "에 의한 로그 " + effectLog.GetText() + "를 1차 실행");
			BTM.StartCoroutine(effectLog.Execute());
			if (effectLog.logDisplay != null)
				effectLog.CreateExplainObjects();
		}

		if (!executed){
			yield return new WaitForSeconds(SkillAndChainStates.longestShowTimeOfSkill);
			SkillAndChainStates.longestShowTimeOfSkill = 0;
		}

		for(int i = 0; i < effectLogList.Count; i++){
			var effectLog = effectLogList[i];
			if(effectLog.executed || effectLog is UnitDestroyLog || effectLog is CameraMoveLog) continue;
			effectLog.executed = true;
			//Debug.Log(casting.Skill.korName + "에 의한 로그 " + effectLog.GetText() + "를 2차 실행");
			if (effectLog is HPChangeLog)
				yield return effectLog.Execute();
			else
				BTM.StartCoroutine(effectLog.Execute());
			if (effectLog.logDisplay != null)
				effectLog.CreateExplainObjects();
		}
		
		if (IsInitiator && !executed){
			yield return new WaitForSeconds(Configuration.textObjectDuration + Configuration.TextDisplayWaitTime);
			//UnitManager.Instance.DeactivateAllDamageTextObjects();
		}

		for(int i = 0; i < effectLogList.Count; i++){
			var effectLog = effectLogList[i];
			if(effectLog.executed) continue;
			effectLog.executed = true;
			if (effectLog is CameraMoveLog)
				yield return effectLog.Execute();
			else
				BTM.StartCoroutine(effectLog.Execute());
			if (effectLog.logDisplay != null)
				effectLog.CreateExplainObjects();
		}
		
		if(executed) yield break;
		
		BTM.CountTriggers(TrigActionType.Cast, actor, log: this);
		//(다른 진영 && 지형지물 아님)인 Target의 수
		int enemyTargetCount = casting.GetTargets().FindAll(unit => unit.GetSide() != actor.GetSide() && !unit.IsObject).Count;
		if(casting.Skill.IsOffensive() && enemyTargetCount >= 2)
			BTM.CountTriggers(TrigActionType.MultiAttack, actor, enemyTargetCount.ToString(), log: this);
	}
}

public class TurnEndLog : EventLog {
	public TurnEndLog(Unit turnEnder){
		SetActor(turnEnder);
	}
	public override string GetText() {
		return actor.GetNameKor() + " 턴 종료";
	}
	public override void CreateExplainObjects() {
		logDisplay.CreateUnitImage(actor);
		logDisplay.CreateText("턴 종료");
	}

	protected override void AfterExecute(){
		actor.EndTurn();	
	}
}

public class CollectStartLog : EventLog {
    Unit Collectable;
    public CollectStartLog(Unit collector, Unit Collectable) {
	    SetActor(collector);
        this.Collectable = Collectable;
    }
    public override string GetText() {
        return actor.GetNameKor() + " : " + Collectable.GetNameKor() + " 수집 시작";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(actor);
        logDisplay.CreateText("수집");
        logDisplay.CreateUnitImage(Collectable);
    }

	protected override void AfterExecute(){
		actor.SetChargeEffect();
	}
}

public class TrapOperatedLog : EventLog {
    TileStatusEffect trap;

    public TrapOperatedLog(TileStatusEffect trap) {
        this.trap = trap;
    }
    public override string GetText() {
        return "타일" + trap.GetOwnerTile().Location + " 에 있던 " + trap.DisplayName(true) + " 발동";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateText("함정 발동");
    }
}

public class TurnStartLog : EventLog {
    public TurnStartLog(Unit turnStarter) {
	    SetActor(turnStarter);
    }
    public override string GetText() {
        return actor.GetNameKor() + " 턴 시작";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(actor);
        logDisplay.CreateText("턴 시작");
    }

	protected override void AfterExecute(){
		actor.StartTurn();
	}
}

public class PhaseStartLog : EventLog {
    int phase;
    public PhaseStartLog(int phase) {
        this.phase = phase;
    }
    public override string GetText() {
        return "페이즈 " + phase + " 시작";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateText("페이즈 " + phase + " 시작");
    }
}
public class PhaseEndLog : EventLog {
    int phase;
    public PhaseEndLog(int phase) {
        this.phase = phase;
    }
    public override string GetText() {
        return "페이즈 " + phase + " 종료";
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateText("페이즈 " + phase + " 종료");
    }

	protected override void AfterExecute(){
		if(BattleData.currentPhase != 0)
			BattleTriggerManager.Instance.CountTriggers(TrigActionType.Phase, log: this);
	}
}
