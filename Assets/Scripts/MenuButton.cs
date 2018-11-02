using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameData;
using Save;
using Enums;

public class MenuButton : MonoBehaviour{
	//유니티 버튼 함수는 parameter를 하나밖에 줄 수 없어서 함수를 쪼개놓음
	public void GoToTitleScene(){
		FindObjectOfType<SceneLoader>().GoToTitle();
	}

    public void RestartBattle() {
		StageNum stage = VolatileData.progress.stageNumber;
		StageClearRecord record = new StageClearRecord(UnitManager.Instance.PCSelectedSkillList, new Dictionary<string, List<int>>(), 
				VolatileData.difficulty, 0, -1, BattleData.currentPhase, Time.time - BattleData.startTime);
		RecordData.RecordStageClear(stage, record);
		GameDataManager.RecordPlayLog(new BattleEndPlayLog(stage, BattleEndType.Restart, record));
		FindObjectOfType<SceneLoader>().LoadNextBattleScene();
    }
}
