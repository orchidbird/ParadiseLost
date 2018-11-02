using System.Collections.Generic;
using UnityEngine;
using Enums;

public class AIData : MonoBehaviour {
	public bool isActive = false;
	public AIInfo info;

	// 도주 목표점들
	public List<Vector2Int> goalArea = new List<Vector2Int> ();

	public bool IsActive(){
		return isActive;
	}

	public void SetActive(bool isReal){
		if (isActive || !isReal) return;
		LogManager.Instance.Record(new AISetActiveLog(gameObject.GetComponent<Unit>()));
	}

	public void SetActiveByExternalFactor(){
		SetActive(info.actOnExternal);
	}

	public void SetGoalArea(Unit AIunit){
		var Checker = BattleTriggerManager.Instance;
		foreach (BattleTrigger trigger in Checker.triggers)
			if (trigger.action == TrigActionType.Escape && Checker.CheckUnitType(trigger, AIunit, TrigUnitCheckType.Actor)) 
				goalArea = trigger.targetTiles;
	}
}
