using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class ConditionPanel : MonoBehaviour{
	public VerticalLayoutGroup winConditions;
	public VerticalLayoutGroup loseConditions;
    public GameObject conditionTextPrefab;

	public static void UpdateTriggerText(ConditionText conditionText, bool beforeUnitGenerate = false) {
        // 텍스트를 트리거 달성 상황에 따라 업데이트한다. 한 스테이지에 같은 텍스트를 가진 트리거 쌍은 없다고 가정한다.
        Text text = conditionText.GetComponent<Text>();
        BattleTrigger trigger = conditionText.trigger;
	    text.text = "- " + trigger.GetName;
	    if (trigger.reqCount <= 0 || beforeUnitGenerate && trigger.action == TrigActionType.UnderCount) return;
	    
	    if (!beforeUnitGenerate && trigger.action == TrigActionType.UnderCount)
		    trigger.count = BattleTriggerManager.Instance.CountUnitOfCondition(trigger);
	    text.text += "(";
	    if (trigger.count < trigger.reqCount || (trigger.action == TrigActionType.UnderCount && trigger.count > trigger.reqCount))
		    text.text += "<color=red>" + trigger.count + "</color>";
	    else
		    text.text += "<color=green>" + trigger.count + "</color>";
	    text.text += "/" + trigger.reqCount + ")";
    }

	string GetRelationText(BattleTrigger.TriggerRelation relation){
		if(relation == BattleTrigger.TriggerRelation.All)
		    return "\t다음 조건을 모두 충족 :";
		if(relation == BattleTrigger.TriggerRelation.Sequence)
			return "\t다음 조건을 순서대로 충족 :";
		return "\t다음 중 하나를 충족 :";
	}
}
