using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LogDisplayPanel : MonoBehaviour {
    public GameObject scrollView;
    public Scrollbar scrollbar;
    public VerticalLayoutGroup logs;
    public GameObject invisibleLogs;
    public readonly int displayHeight = 40;
    int maxNumDisplay;

    public void Awake() {
        CalculateMaxNumDisplay();
    }

    public void CalculateMaxNumDisplay() {
        maxNumDisplay = (int)(scrollView.GetComponent<RectTransform>().rect.height / displayHeight);
    }

    public void AddLogDisplay(LogDisplay logDisplay, int num, bool display = true) {
        logDisplay.name = "Log " + num;
        logDisplay.index = num;
        if(display){
            logDisplay.transform.SetParent(logs.transform, false);
            if(logDisplay.log is EffectLog)
                logDisplay.GetComponent<HorizontalLayoutGroup>().padding.left = 60;
            if(Setting.LogDebuggingForMeaning) Debug.Log(logDisplay.name + " : " + (logDisplay.log.GetText() == "" ? logDisplay.log.GetType().ToString() : logDisplay.log.GetText()));
        }else{
            logDisplay.transform.SetParent(invisibleLogs.transform);
            if(Setting.LogDebuggingForElse)
	            Debug.Log(logDisplay.name + " : " + (logDisplay.log.GetText() == "" ? logDisplay.log.GetType().ToString() : logDisplay.log.GetText()));
        }
    }

	void OnEnable(){
		scrollbar.numberOfSteps = BattleData.logDisplayList.Count - maxNumDisplay + 1;
		scrollbar.value = 0;
	}

	void OnDisable(){
		foreach(var tile in TileManager.Instance.GetAllTiles().Values)
			tile.sprite.color = Color.white;
		foreach(var unit in UnitManager.GetAllUnits())
			unit.GetComponent<SpriteRenderer>().color = Color.white;
		BattleUIManager.Instance.DeactivateStatusEffectDisplayPanel();
		BattleUIManager.Instance.DeactivateChangeDisplayPanel();
		BattleUIManager.Instance.DeactivateSkillUI();
	}
}
