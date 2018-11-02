using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;
using Save;
using Enums;
using UtilityMethods;
using Language = UtilityMethods.Language;

public class ResultPanel : MonoBehaviour{
	public Text TriggerNames;
	public Text ScoreText;
	public Text TotalExpText;
	private bool alreadyClicked;
	public BattleTriggerManager Checker;
	SceneLoader sceneLoader;

	public void Initialize(){
		TriggerNames.text = "";
		ScoreText.gameObject.SetActive(true);
		ScoreText.text = "";
		sceneLoader = Checker.SceneLoader;
		
		if(ReadyManager.Instance != null)
			Destroy(ReadyManager.Instance.gameObject);
	}

	public IEnumerator PrintLoseResult(){
		List<BattleTrigger> trigs = BattleTriggerManager.Instance.triggers.FindAll(trig => trig.result == TrigResultType.Lose);

		foreach(var trigger in trigs){
			BattleData.rewardPoint += trigger.reward;
            trigger.rewardAddedCount++;
			if(trigger.acquired) TriggerNames.text += "<color=red>" + trigger.GetName + " (V)</color>\n";
			else TriggerNames.text += trigger.GetName + "\n";
			TotalExpText.text = "<color=red>" + Language.Select("패배", "Fail") + "</color>";

			yield return new WaitForSeconds(0.5f);
		}

		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));

		StageNum stage = VolatileData.progress.stageNumber;
		StageClearRecord record = new StageClearRecord(UnitManager.Instance.PCSelectedSkillList, new Dictionary<string, List<int>>(), 
				VolatileData.difficulty, 0, -1, BattleData.currentPhase, Time.time - BattleData.startTime);
		RecordData.RecordStageClear(stage, record);
		GameDataManager.RecordPlayLog(new BattleEndPlayLog(stage, BattleEndType.Lose, record));
		SteamAchievementManager.Instance.CheckOnLose();

		yield return LoadNextScene(false);
	}

	public IEnumerator PrintWinResult(){
		List<BattleTrigger> scoreTriggers = BattleTriggerManager.Instance.triggers.FindAll(trig =>
			(trig.result == TrigResultType.Win || trig.result == TrigResultType.Bonus) && trig.acquired && trig.reward != 0);
		Debug.Log("count of scoreTriggers : " + scoreTriggers.Count);
		
		foreach(var trig in scoreTriggers){
			BattleData.rewardPoint += trig.reward;
            trig.rewardAddedCount++;
			TriggerNames.text += trig.GetName + " " + MultiplierText(trig) + "\n";

			yield return new WaitForSeconds(0.1f);
			if (!trig.repeatable || trig.count == 1){
				ScoreText.text += trig.reward;
			}else{
				ScoreText.text += trig.reward * trig.rewardAddedCount;
			}
				
			ScoreText.text += "\n";
			yield return new WaitForSeconds(0.4f);
		}

        int point = BattleData.rewardPoint;
        int star = 0;
        if (point >= 70) star = 3;
        else if (point >= 50) star = 2;
        else if (point >= 30) star = 1;
        TotalExpText.text = Language.Select("성취도 : ", "Achievements: ") + point;
        for(int i = 0; i < star; i++) {
            TotalExpText.text += "★";
        }
		StageNum stage = VolatileData.progress.stageNumber;

		Dictionary<string, List<int>> achievedTriggers = new Dictionary<string, List<int>>();
        foreach(var trigger in scoreTriggers)
            achievedTriggers.Add(trigger.GetName, new List<int> { trigger.rewardAddedCount, trigger.reward });

		StageClearRecord record = new StageClearRecord(UnitManager.Instance.PCSelectedSkillList, achievedTriggers, 
				VolatileData.difficulty, point, star, BattleData.currentPhase, Time.time - BattleData.startTime);
		RecordData.RecordStageClear(stage, record);
		GameDataManager.RecordPlayLog(new BattleEndPlayLog(stage, BattleEndType.Win, record));
		SteamAchievementManager.Instance.CheckOnClear(star);

		if (VolatileData.gameMode == GameMode.AllStageTest) {
			foreach(StageNum stageNum in Enum.GetValues(typeof(StageNum))){
				if (stageNum > VolatileData.progress.stageNumber) {
					VolatileData.progress.stageNumber = stageNum;
					break;
				}
			}
			var sceneLoader = FindObjectOfType<SceneLoader>();
			sceneLoader.LoadNextBattleScene();
		}

		//다 출력된 후 클릭을 해야 넘어감
		yield return new WaitUntil (() => Input.GetMouseButtonDown(0));
		yield return LoadNextScene(true);
	}

	IEnumerator LoadNextScene(bool winBattle){
		if(VolatileData.gameMode != GameMode.Challenge){
			BattleTrigger TriggerForNextScript = Checker.triggers.Find(trig => trig.acquired && !string.IsNullOrEmpty(trig.conditionalSceneName));
			if (TriggerForNextScript != null)
				sceneLoader.LoadNextDialogueScene(TriggerForNextScript.conditionalSceneName);
			else if(winBattle)
				sceneLoader.LoadNextDialogueScene(Checker.triggers.Find(x => x.result == TrigResultType.Info).nextSceneName);
			else //패배한 전투
				sceneLoader.LoadNextDialogueScene ("Scene_Lose" + (int)VolatileData.progress.stageNumber);
		}else
			StartCoroutine(Checker.SceneLoader.LoadArchiveScene());

		// 씬이 바뀔 때까지 다른 함수로 넘어가지 않기 위함
		yield return new WaitForSeconds(10);
	}

	string MultiplierText(BattleTrigger trigger){
		if(!trigger.repeatable || trigger.count == 1)
			return "";
		else
			return " x"+trigger.count;
	}
}
