using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;

public class LogManager : MonoBehaviour {
    public LogDisplayPanel logDisplayPanel;
    public GameObject logDisplayPrefab;
    private static LogManager instance = null;
    public static LogManager Instance { get { return instance; } }
    public static void SetInstance() { instance = FindObjectOfType<LogManager>(); }
	public List<EventLog> eventLogs = new List<EventLog>();

    bool isExecutingEventLog;
	static bool duringPreview;
	public static void SetDuringPreview(bool onoff){
		duringPreview = onoff;
	}
    // EventLog가 실행 중일 때 추가된 EffectLog들. EventLog의 실행이 끝난 후 한 번에 추가된다.
    List<EffectLog> stashedEffectLogs = new List<EffectLog>();

    public void Record(Log log, bool execution = false){
        if (log is EffectLog) {
            if(((EffectLog)log).isMeaningless()) return;
            if (log is StatusEffectLog && !CheckToOffset((StatusEffectLog)log)) return;
			Debug.Assert(GetLastEventLog() != null, log.GetText() + "appears faster than event log");
	        var lastEventLog = GetLastEventLog();
		    if (!isExecutingEventLog){
			    lastEventLog.GetEffectLogList().Add((EffectLog)log);
			    ((EffectLog)log).parentEvent = lastEventLog;
		    }else
			    stashedEffectLogs.Add((EffectLog)log);
        }else
	        eventLogs.Add((EventLog)log);

	    log.Initialize();
	    
		if (!duringPreview) {
			var logDisplay = Instantiate (logDisplayPrefab).GetComponent<LogDisplay> ();
			int numLog = BattleData.logDisplayList.Count;

			logDisplay.log = log;
			log.logDisplay = logDisplay;
			BattleData.logDisplayList.Add (logDisplay);
			logDisplayPanel.AddLogDisplay (logDisplay, numLog + 1, DisplayThisLog (log));
		}

	    if (execution && log is EventLog)
		    StartCoroutine(ExecuteLastEventLogAndConsequences(log.logDisplay.name + "의 즉각 실행 요청"));
    }
    public IEnumerator ExecuteLastEventLogAndConsequences(string caller = "") {// 마지막 Event와 그로 인해 발생하는 모든 새로운 Effect와 Event를 실행한다.
	    int i = 1;
	    do{
		    //var log = GetLastEventLog();
		    //Debug.Log(caller + "에 의해 최근 로그 " + log.GetText() + " " + i + "번째 실행: 효과 " + log.GetEffectLogList().Count + "개 중 " + log.GetEffectLogList().Count(item => !item.executed) + "개가 미실행");
		    yield return ExecuteLastEventLog();
		    yield return ExecuteConsequentEventLogs();
		    i++;
	    }while(!GetLastEventLog().FullExecuted && i < 10);
		
	    BattleUIManager.Instance.UpdateSmallConditionTexts();
	    UnitManager.Instance.ResetLatelyHitUnits();
	    //UnitManager.Instance.UpdateFogOfWar();
	    foreach (var chain in ChainList.GetChainList())
		    chain.Casting.ResetRealRange();

	    yield return BattleManager.Instance.CheckWinOrLoseTriggers();
	    //Debug.Log("로그 실행 종료");
	}
    IEnumerator ExecuteConsequentEventLogs() {
        foreach (var eventLog in GenerateConsequentEventLogs()) {
            if (eventLog == null) yield break;
            yield return ExecuteLastEventLog();
        }
    }
    IEnumerable<Log> GenerateConsequentEventLogs() {   // 새로운 Event를 발생시킴
        foreach (var eventLog in TileManager.Instance.CheckAllTraps())
            yield return eventLog;
    }
    public void RemoveLastEventLog() {                     // 마지막 Event를 실행하지 않고 없앤 후 리턴한다(Preview에 쓰임)
		RemoveEventLogAndChildren(GetLastEventLog());
    }
	public void RewindLastEventLog() {
		EventLog lastEventLog = GetLastEventLog ();
		lastEventLog.Rewind();
		lastEventLog.GetEffectLogList().Reverse();

		List<Log> rewoundLogs = lastEventLog.GetEffectLogList().ConvertAll(effectLog => (Log)effectLog);
		rewoundLogs.Add(lastEventLog);

		BattleTriggerManager.Instance.RevertTriggersCountedByLogs(rewoundLogs);
		
		RemoveEventLogAndChildren(lastEventLog);
		UnitManager.Instance.UpdateBasicStatusEffects();
	}
	
    void GenerateConsequentEffectLogs(EventLog lastEvent){
        var UM = UnitManager.Instance;
		UM.UpdateStatsAtActionEnd();
		UM.TriggerPassiveSkillsAtActionEnd();
		if (lastEvent is CastLog && !((CastLog)lastEvent).triggered){
			var log = (CastLog) lastEvent;
            UM.TriggerPassiveSkillsAfterCast(log);
            log.triggered = true;
			UM.CheckDestroyedUnits(log.casting);
		}else
			UM.CheckDestroyedUnits();
		UM.TriggerStatusEffectsAtActionEnd();
		UM.UpdateStatusEffectsAtActionEnd ();
		TileManager.Instance.UpdateTileStatusEffectsAtActionEnd();
		UM.UpdateHealthViewersAtActionEnd();
	    UM.UpdateBasicStatusEffects();
		if (BattleData.turnUnit != null)
            BattleUIManager.Instance.UpdateSelectedUnitViewerUI(BattleData.turnUnit);
	}

	void DeleteUnexecutedEvents(){
		eventLogs.RemoveAll(log => !log.executed);
	}
    IEnumerator ExecuteLastEventLog()
    {
	    int count = 0;
        do{
	        count++;
	        var log = GetLastEventLog();

	        if (isExecutingEventLog){
		        //Debug.Log(log.GetText() + " 실행 보류");
		        yield break;
	        }
		    //Debug.Log(log.GetText() + " 실행");
		        
            isExecutingEventLog = true;
			if(!log.executed && log.logDisplay != null)
				log.CreateExplainObjects();
			yield return log.Execute();

			isExecutingEventLog = false;
            foreach (var effectLog in stashedEffectLogs) {
                log.GetEffectLogList().Add(effectLog);
                effectLog.parentEvent = log;
            }
            stashedEffectLogs = new List<EffectLog>();
			
			if (!log.executed) {
                if (log is CastLog) HandleAfterCastLog();
                else if (log is ChainLog) HandleAfterChainLog();
                else if (log is MoveLog) HandleAfterMoveLog();
                else if (log is TurnEndLog) HandleAfterTurnEndLog();
			}
			log.executed = true;
            GenerateConsequentEffectLogs(log);             // 이 Event로 인해 발생한 Effect들이 더 있는지 확인한다.
			RemoveInvalidEffectLogs();                  // 무의미한 Effect들을 없앤다. (Attach한 후 Stack을 0으로 한다던가)
		} while (IsThereNewEffect && count < 10);                    // 유의미한 Effect가 있다면 있다면 다시 실행한다.
    }

	private bool IsThereNewEffect{get{return GetLastEventLog().GetEffectLogList().Any(log => !log.executed);}}

	void HandleAfterCastLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
        BattleData.selectedSkill = null;
    }
    void HandleAfterChainLog() {
        BattleData.currentState = CurrentState.AfterAction;
        BattleData.selectedSkill = null;
    }
    void HandleAfterMoveLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
        BattleData.previewAPAction = null;
    }
    void HandleAfterTurnEndLog() {
        BattleData.currentState = CurrentState.AfterAction;
    }
    void HandleAfterMoveCancelLog() {
        BattleData.currentState = CurrentState.FocusToUnit;
    }

	public EventLog GetLastEventLog(){
		if(eventLogs.Count == 0)
			return null;

		for (int i = 10; i > 0; i--){
			if(i > eventLogs.Count) continue;
			var log = eventLogs[eventLogs.Count - i];
			if(!log.FullExecuted) return log;
		}
		return eventLogs.Last();
	}
    bool DisplayThisLog(Log log) {
        if(log is CameraMoveLog || log is SoundEffectLog || log is VisualEffectLog
            || log is DisplayDamageTextLog || log is AddLatelyHitInfoLog || log is WaitForSecondsLog
            || log is PaintTilesLog || log is DepaintTilesLog || log is AddChainLog || log is DisplayScriptLog
			|| log is DirectionChangeLog || log is AddMoveOverloadLog)   return false;
        return true;
    }
	void RemoveEventLogAndChildren(EventLog eventLog){
		eventLogs.Remove(eventLog);
		List<Log> logsToRemove = new List<Log> { eventLog };
		foreach (Log log in eventLog.GetEffectLogList())
			logsToRemove.Add(log);
		foreach(var log in logsToRemove)
			RemoveLog(log, false);
	}
	void RemoveLog(Log log, bool detachFromParent){
		if (!duringPreview) {
			LogDisplay logDisplay = log.logDisplay;
			BattleData.logDisplayList.Remove (logDisplay);
			if (logDisplay != null)
				Destroy (logDisplay.gameObject);
		}
        if(detachFromParent && log is EffectLog && ((EffectLog)log).parentEvent != null)
            ((EffectLog)log).parentEvent.GetEffectLogList().Remove((EffectLog)log);
    }
    
    // 매 event의 발생마다, 변하는 statusEffect들의 change를 기억한다. 
    Dictionary<StatusEffect, StatusEffectChange> SEChangeDict = new Dictionary<StatusEffect, StatusEffectChange>();

    bool CheckToOffset(StatusEffectLog log) {
        // AmountChange 2 -> 2나, AmountChange 2->3 직후에 3 -> 2 하는 것과 같이 의미없는 log들을 상쇄한다.
        if (log.type == StatusEffectChangeType.Attach) {
            StatusEffect alreadyAppliedSameEffect = null;
            if(log.GetOwner() != null)
                alreadyAppliedSameEffect = log.GetOwner().statusEffectList.Find(se => se.IsSameStatusEffect(log.statusEffect));
            else if(log.GetOwnerTile() != null)
                alreadyAppliedSameEffect = log.GetOwnerTile().StatusEffectList.Find(se => se.IsSameStatusEffect(log.statusEffect));
            if (alreadyAppliedSameEffect != null && !alreadyAppliedSameEffect.IsRenewable(log.statusEffect))	//이미 동일한 statusEffect가 있고 업데이트할 필요가 없다면
                return false;
        }

        StatusEffect statusEffect = log.statusEffect;
        if (!SEChangeDict.ContainsKey(statusEffect))
            SEChangeDict.Add(statusEffect, new StatusEffectChange(log));
        else SEChangeDict[statusEffect].Update(log);
        StatusEffectChange change = SEChangeDict[statusEffect];
        foreach (var logToOffset in change.logsOffsetBy(log)) {
            //Debug.Log("offset log : " + logToOffset.GetText());
            if(logToOffset != log)
                RemoveLog(logToOffset, true);
            else return false;
        }
        return true;
    }

    public void RemoveInvalidEffectLogs() {
        //Debug.Log("Removing Invalid EffectLogs");
        foreach (var kv in SEChangeDict) {
            foreach (var log in kv.Value.logsNotValid()) {
                RemoveLog(log, true);
            }
        }
        SEChangeDict.Clear();
    }

    class StatusEffectChange {
        Dictionary<StatusEffectChangeType, List<StatusEffectLog>> changes = new Dictionary<StatusEffectChangeType, List<StatusEffectLog>>();
        public StatusEffectChange(StatusEffectLog log) {
            Update(log);
        }
        public void Update(StatusEffectLog log) {
            if(changes.ContainsKey(log.type))
                changes[log.type].Add(log);
            else changes.Add(log.type, new List<StatusEffectLog> { log });
        }
        public List<StatusEffectLog> logsOffsetBy(StatusEffectLog log) {    // log에 의해 상쇄되는 것들
            if(CheckOffsetForNewLog(log)) {
                List<StatusEffectLog> logsToOffset = changes[log.type];
                changes.Remove(log.type);
                return logsToOffset;
            }
            return new List<StatusEffectLog>();
        }
        public List<StatusEffectLog> logsNotValid(){
	        // statusEffect를 Attatch하고 바로 Stack을 0으로 하는 것 등
	        return !isValid() ? AllLogs() : new List<StatusEffectLog>();
        }
        bool CheckOffsetForNewLog(StatusEffectLog log) {
            StatusEffectChangeType type = log.type;
            return type != StatusEffectChangeType.Attach && type != StatusEffectChangeType.Remove && 
                   changes.ContainsKey(log.type) && changes[log.type][0].beforeAmount == log.afterAmount;
        }
        bool isValid() {
	        if (!changes.ContainsKey(StatusEffectChangeType.Attach) ||
	            changes[StatusEffectChangeType.Attach][0].executed ||
	            !changes.ContainsKey(StatusEffectChangeType.StackChange)) return true;
	        
	        List<StatusEffectLog> stackChangeLogs = changes[StatusEffectChangeType.StackChange];
	        return stackChangeLogs[stackChangeLogs.Count - 1].afterAmount != 0;
        }
        List<StatusEffectLog> AllLogs() {
            List<StatusEffectLog> allLogs = new List<StatusEffectLog>();
            foreach(var kv in changes)
                allLogs.AddRange(kv.Value);
            return allLogs;
        }
    }
}
