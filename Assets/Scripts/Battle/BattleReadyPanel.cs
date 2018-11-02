using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;
using System.Linq;
using Enums;
using UtilityMethods;
using Language = UtilityMethods.Language;

public class BattleReadyPanel : MonoBehaviour{
	public enum PanelType{Ether, Briefing}
	public PanelType panelType;
    public GameObject mainPanel;
    public SkillViewer skillViewer;
	List<AvailableUnitButton> unitButtons;
	public List<SkillSelectButton> skillButtonList = new List<SkillSelectButton>();
	public SkillSelectButton SkillButtonPrefab;
    public Scrollbar scrollbar;
    public MapBriefingManager MBM;
	public GameObject storyPanel;
	public ConditionPanel conditionPanel;
	public GameObject SkillResetButton;

    public GameObject backgroundPanel;
	int buttonDist = 90;

	//ReadyManager.Start()가 끝난 직후 넘어온다
	public void Initialize(List<AvailableUnitButton> unitButtons){
		var RM = ReadyManager.Instance;
		//CharacterButton Rendering을 초기화하기 위해 무의미한 코드 한 줄
		RM.CharacterButtons.transform.parent.GetComponentInParent<ScrollRect>().verticalScrollbarSpacing = 1;
		
		this.unitButtons = unitButtons;

		for (int i = 0; i < 9; i++){
			var skillButton = Instantiate(SkillButtonPrefab, mainPanel.transform.Find("SkillPart").Find("SkillListPanel"));
			skillButton.skillViewer = skillViewer;
			skillButtonList.Add(skillButton);
		}

		skillViewer.Initialize();

        List<AvailableUnitButton> PCUnitButtons = unitButtons.FindAll(unitButton => unitButton.isPC);
        PCUnitButtons.ForEach(button => button.UpdateGray());
        if (VolatileData.gameMode != GameMode.Challenge) {
			PCUnitButtons.ForEach (button => {
				if (button.isFixed)
					RM.PickUnit(button, true);
			});
		}

		if (VolatileData.gameMode == GameMode.AllStageTest){
			SetPanelType(PanelType.Ether, true);
			RM.PickRandomCharactersAndSkills();
			RM.CheckToStartBattle();
		}else
			SetPanelType(PanelType.Briefing, true);
    }
	
    public void Update(){
        SetScrollbarValues();
    }

	//이 함수는 TopButtons에서 사용한다.
	public void SetPanelType(string typeName){
		SetPanelType((PanelType)Enum.Parse(typeof(PanelType), typeName));
	}

	public void SetPanelType(PanelType type, bool initialize = false){
        if(!initialize && type == panelType)
            return;
        var RM = ReadyManager.Instance;
		bool isBriefing = type == PanelType.Briefing;
		conditionPanel.gameObject.SetActive(isBriefing);
		storyPanel.gameObject.SetActive(isBriefing);
		mainPanel.SetActive(!isBriefing);
		SkillResetButton.SetActive(!isBriefing);
		ReadyManager.Instance.secret.enabled = isBriefing && VolatileData.stageData.IsTwoSideStage();
		
        if (isBriefing) {
            RM.ActivateUnitButtons(PC:true, activate:false);
            RM.ActivateUnitButtons(PC:false, activate:true);
            MBM.GenerateUnitAndTileImages(backgroundPanel.GetComponent<RectTransform>(), backgroundPanel);
	        LayoutRebuilder.ForceRebuildLayoutImmediate(conditionPanel.GetComponent<RectTransform>());
			storyPanel.GetComponentInChildren<Text>().text = VolatileData.GetStageData(VolatileData.progress.stageNumber, StageInfoType.Summary);
	        //0.882를 곱하는 이유는 깃발 이미지파일의 왼쪽 11.8%가 투명 영역으로 비어있기 때문.
	        var newOffset = new Vector2(-conditionPanel.GetComponent<RectTransform>().rect.width * conditionPanel.GetComponent<RectTransform>().localScale.x * 0.882f, storyPanel.GetComponent<RectTransform>().offsetMax.y);
	        storyPanel.GetComponent<RectTransform>().offsetMax = newOffset;
        } else {
            Vector3 position = mainPanel.transform.position;
            position.z = 0;
            mainPanel.transform.position = position;
            RM.ActivateUnitButtons(PC: true, activate: true);
            RM.ActivateUnitButtons(PC: false, activate: false);
            MBM.EraseAllTileImages();
            MBM.EraseAllUnitImages();
            unitButtons.FindAll(button => button.gameObject.activeSelf)[0].UpdateRightPanel();
            
            SetAllSkillSelectButtons();
	        unitButtons.ForEach(button => button.UpdateGray());
        }
		backgroundPanel.SetActive(isBriefing);
        panelType = type;
        unitButtons.ForEach(button => {RM.UpdateEtherText(button);});
    }

	public void SetAllSkillSelectButtons(){
		var mySkills = VolatileData.SkillsOf(ReadyManager.Instance.RecentUnitButton.codeName, true, false).OrderBy(skill => skill.RequireLevel).ToList();
		for (int i = 0; i < skillButtonList.Count; i++){
			if (i < mySkills.Count){
				skillButtonList[i].gameObject.SetActive(true);
				skillButtonList[i].Initialize(mySkills[i]);
				skillButtonList[i].ownerInfo = ReadyManager.Instance.RecentUnitButton.unitInfo;
			}else
				skillButtonList[i].gameObject.SetActive(false);
		}
	}

    public void SetScrollbarValues() {
        scrollbar.size = 0.1f;
        scrollbar.numberOfSteps = 11;
    }
}
