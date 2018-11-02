using Enums;
using GameData;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Save;
using UtilityMethods;

public class BattleWarning : MonoBehaviour {
	public enum WarningType{Restart, DifficultyChange, Title}
	public WarningType warnType;
	public Difficulty Difficulty;
	public Text Text;

	public void SetDifficulty(int input){
		Difficulty = (Difficulty) input;
	}
	public void Initialize(int input){
		warnType = (WarningType) input;
		Difficulty = VolatileData.difficulty;
		Text.text = Language.Find("Warning_" + warnType);
	}

	public void YesPushed(){
		if (warnType == WarningType.Title)
			FindObjectOfType<SceneLoader>().GoToTitle();
		else if(warnType == WarningType.Restart)
			RestartBattle(VolatileData.difficulty);
		else if(warnType == WarningType.DifficultyChange){
			RestartBattle(Difficulty);
		}
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
