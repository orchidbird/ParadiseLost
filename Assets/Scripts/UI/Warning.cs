using System;
using System.Collections.Generic;
using Enums;
using GameData;
using Save;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class Warning : MonoBehaviour{
	public enum WarningType{Restart = 0, DifficultyChange = 1, Title = 2, InvalidCasting = 3} //숫자 할당 함부로 바꾸지 말 것!(에디터와 연동됨)
	public WarningType warnType;
	public Difficulty Difficulty;
	public Action ResponseForYes;
	public Action ResponseForNo;
	public Toggle ignoreThis;
	public Text Text;

	public void Initialize(int input){
		warnType = (WarningType) input;
		
		if (warnType == WarningType.InvalidCasting){
			ResponseForYes = () => {BattleManager.Instance.triggers.castCheckYesClicked.Trigger();};
			ResponseForNo = () => { BattleManager.Instance.triggers.castCheckNoClicked.Trigger(); };
		}else{
			ResponseForNo = () => {};
			if (warnType == WarningType.Title)
				ResponseForYes = FindObjectOfType<SceneLoader>().GoToTitle;
			else if(warnType == WarningType.Restart)
				ResponseForYes = () => RestartBattle(VolatileData.difficulty);
			else if(warnType == WarningType.DifficultyChange)
				ResponseForYes = () => RestartBattle(Difficulty);
		}

		ignoreThis.isOn = Configuration.ignoreWarning[warnType];
		Difficulty = VolatileData.difficulty;
		Text.text = Language.Find("Warning_" + warnType);
	}
	public void Initialize(Action action, string text){
		ResponseForYes = action;
		ResponseForNo = () => {};
		Text.text = text;
	}

	public void Act(){
		ResponseForYes();
		ClosePanel();
	}
	public void OnCancel(){
		ResponseForNo();
		ClosePanel();
	}

	void ClosePanel(){
		if(ignoreThis != null)
			Configuration.ignoreWarning[warnType] = ignoreThis.isOn;
		UIManager.Instance.DeactivateTopUI();
	}

	public void SetDifficulty(int input){
		Difficulty = (Difficulty) input;
	}

	void RestartBattle(Difficulty difficulty) {
		StageNum stage = VolatileData.progress.stageNumber;
		StageClearRecord record = new StageClearRecord(UnitManager.Instance.PCSelectedSkillList, new Dictionary<string, List<int>>(), 
			VolatileData.difficulty, 0, -1, BattleData.currentPhase, Time.time - BattleData.startTime);
		RecordData.RecordStageClear(stage, record);

		VolatileData.difficulty = Difficulty;
		GameDataManager.RecordPlayLog(new BattleEndPlayLog(stage, BattleEndType.Restart, record));
		FindObjectOfType<SceneLoader>().LoadNextBattleScene();
	}
}
