using System;
using UnityEngine.UI;
using UnityEngine;
using GameData;
using UnityEngine.SceneManagement;
using Enums;

public class Test : MonoBehaviour {
    public GameObject panel;
    public GameObject stageTextField;
    public InputField stageInputField;

    void Start() {
        panel.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Title");

        if (Input.GetKeyDown(KeyCode.Return) && panel.activeSelf) {
            GameDataManager.Load(5);
            string stageNumber = stageInputField.text;
			VolatileData.progress.stageNumber = (StageNum)(Convert.ToInt32(stageNumber));

            var sceneLoader = FindObjectOfType<SceneLoader>();
            sceneLoader.LoadNextBattleScene();
        }
    }
    
    public void OnStageButtonClicked() {
        VolatileData.gameMode = GameMode.Test;
        panel.SetActive(true);
        stageInputField.gameObject.SetActive(true);
        stageTextField.SetActive(true);
    }

    public void OnEachStageButtonClicked(StageNum stageNumber){
        VolatileData.progress.stageNumber = stageNumber;
        var sceneLoader = FindObjectOfType<SceneLoader>();
        sceneLoader.LoadNextBattleScene();
    }

	public void TestAllStages(){
		VolatileData.gameMode = GameMode.AllStageTest;
		if (stageInputField.text == "") {
			VolatileData.progress.stageNumber = StageNum.S1_1;
		} else {
			string stageNumber = stageInputField.text;
			VolatileData.progress.stageNumber = (StageNum)(Convert.ToInt32 (stageNumber));
		}
		var sceneLoader = FindObjectOfType<SceneLoader>();
		sceneLoader.LoadNextBattleScene();
	}

	public void ChangeDifficulty(int difficulty){
		VolatileData.SetDifficulty(difficulty);
	}
}
