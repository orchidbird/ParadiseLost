using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Enums;
using UnityEngine.UI;
using GameData;
using UnityEngine.SceneManagement;
using UtilityMethods;

public class Candidate{
	ReadyManager Manager;
	public string CodeName;
	public List<Skill> selectedSkills = new List<Skill>();
    public bool picked;

	public Candidate(string codeName){
		Manager = ReadyManager.Instance;
		CodeName = codeName;
		ReadyManager.Instance.LoadSkillTree(this);
	}

	public void ResetSkills(){			
		Manager.ReadyPanel.skillButtonList.ForEach(button => button.selected = button == Manager.ReadyPanel.skillButtonList[0]);
		selectedSkills.Clear();
		selectedSkills.Add(VolatileData.SkillsOf(CodeName, true).Find(skill => skill.RequireLevel == 0));
		ReadyManager.Instance.UpdateEtherTexts();
	}
	
	public bool IsThisPickable(Skill skill){
		return Manager.RemainEther >= skill.ether 
		       && !selectedSkills.Exists(_skill => _skill.korName == skill.korName)
		       && (string.IsNullOrEmpty(skill.required) || selectedSkills.Exists(item => item.address == skill.required));
	}
	public int SumOfCost{get{return selectedSkills.Count == 0 ? 0 : selectedSkills.Sum(skill => skill.ether);}}
}

public class ReadyManager : MonoBehaviour{
	public static ReadyManager Instance;
	public SelectableUnitCounter selectableUnitCounter;
	public BattleReadyPanel ReadyPanel;
    public RightScreen_BattleReady rightPanel;
	public AvailableUnitButton RecentUnitButton;
	List<AvailableUnitButton> unitButtons = new List<AvailableUnitButton>();
	public List<Candidate> candidates = new List<Candidate>();
    public List<Candidate> pickedList{get{return candidates.FindAll(cand => cand.picked);}}
	public DetailInfoPanel detailPanel;

    public GameObject CharacterButtons;
    public GameObject characterButtonPrefab;
    public GameObject arrowIcon;

	public Button startButton;

	public Image secret;

	public void ResetSkillOfThisUnit(){ //에디터 버튼으로 호출
		RecentUnitButton.candidate.ResetSkills();
	}
    public void PickUnit(AvailableUnitButton button, bool pickIn){
        if(button.isPC){
			if(!pickIn && button.isFixed
			    || pickIn && selectableUnitCounter.IsPartyFull()){
				SoundManager.Instance.PlaySE ("Cannot");
				return;
			}
            
            button.candidate.picked = pickIn;
            button.ActiveHighlight(pickIn);
            UpdateEtherText(button);
        }else if (ReadyPanel.panelType == BattleReadyPanel.PanelType.Briefing){
            ReadyPanel.mainPanel.SetActive(pickIn);
	        ReadyPanel.MBM.ToggleAllImages(pickIn);
        }

	    button.UpdateGray();
    }

	public int MaxEther{get{
		int token = RecordData.tokens == null ? 0 : RecordData.tokens.Count;
		return (RecordData.level*3 + 45 + token) * selectableUnitCounter.maxSelectableUnitNumber / 3;
	}}
	public int CurrentEther{get{return pickedList.Count == 0 ? 0 : pickedList.Sum(cand => cand.SumOfCost);}}
	public int RemainEther{get { return MaxEther - CurrentEther; }}
	bool IsEfficient{get{ //효율적: "더 이상 선택할 수 있는 능력이 없음"
		return pickedList.Count == FindObjectOfType<SelectableUnitCounter>().maxSelectableUnitNumber
		       && pickedList.All(cand => !VolatileData.SkillsOf(cand.CodeName, true).Any(cand.IsThisPickable));
	}}

	public void LoadSkillTree(Candidate unit){
		if (!RecordData.customSkillTrees.ContainsKey(unit.CodeName)){
			unit.ResetSkills();
			return;
		}
	    
	    foreach (var skillName in RecordData.customSkillTrees[unit.CodeName]){
		    var selectedSkill = TableData.AllSkills.Find(skill => _String.Match(skill.ownerName, unit.CodeName) && skill.korName == skillName);
		    if (selectedSkill != null && selectedSkill.RequireLevel <= RecordData.level && 
		        unit.SumOfCost + selectedSkill.ether <= MaxEther && !unit.selectedSkills.Contains(selectedSkill)) {
			    unit.selectedSkills.Add(selectedSkill);
		    }
	    }
    }

	private void Awake(){
		Instance = this;
	}

	void Start(){
		GeneratePCUnitButtons();
		
		DontDestroyOnLoad(gameObject);
		RecentUnitButton = unitButtons.First();
		ReadyPanel.Initialize(unitButtons);
		if(VolatileData.progress.stageNumber == Setting.readySceneOpenStage)
			TutorialManager.Instance.Activate("BattleReady");
	}

	List<string[]> stageAvailablePCTable;
	public List<string[]> StageAvailablePCTable{get{return stageAvailablePCTable ?? (stageAvailablePCTable = Parser.GetMatrixTableFrom("Data/StageAvailablePC"));}}
	
    void GeneratePCUnitButtons(){
		string[] stageData = Parser.FindRowOf(StageAvailablePCTable, ((int)VolatileData.progress.stageNumber).ToString()); 
		int selectMax = int.Parse (stageData [1]);
		int availableNum = int.Parse (stageData [2]);
		int fixedMemberNum = int.Parse (stageData [3]);
		bool isAllPickStage = (selectMax == availableNum);
		FindObjectOfType<SelectableUnitCounter>().SetMaxSelectableUnitNumber (selectMax);

        var selectableUnitNames = (VolatileData.gameMode == GameMode.Challenge)?
	        RecordData.GetUnlockedCharacters() : StageData.CandidatesOfStage(VolatileData.progress.stageNumber); 

		for (int i = 0; i < selectableUnitNames.Count; i++) {
			var unitButton = Instantiate(characterButtonPrefab, CharacterButtons.transform).GetComponent<AvailableUnitButton>();
			var candidate = new Candidate (selectableUnitNames [i]);
			candidates.Add(candidate);
			//unitButton.Initialize (true, candidate, isFixed: isAllPickStage || (i < fixedMemberNum));
			unitButtons.Add(unitButton);
			LoadSkillTree(unitButton.candidate);
			if(RecordData.recentPicks.Contains(unitButton.codeName) && !VolatileData.stageData.IsTwoSideStage())
				PickUnit(unitButton, true);
		}
    }

	/*private void GenerateNPCUnitButtons(){
		Debug.Log("Generate NPC Button");
        VolatileData.stageData.Load(true);  // 재도전했을 시 Battle 씬에서 unitInfo가 오염되었으므로 다시 로드한다. 
        var unitInfos = VolatileData.stageData.GetUnitInfos();
        var NPCNameDict = new Dictionary<string, UnitInfo>();
        foreach(var info in unitInfos){
	        if (info.nameKor == "selected" || info.isObject) continue;
	        var generalCodeName = _String.GeneralName(info.codeName);
	        if(!NPCNameDict.ContainsKey(generalCodeName))
		        NPCNameDict.Add(generalCodeName, info);
        }
		
        foreach (var kv in NPCNameDict) {
            var unitButton = Instantiate(characterButtonPrefab, CharacterButtons.transform).GetComponent<AvailableUnitButton>();
            unitButton.Initialize(false, info : kv.Value);
            unitButtons.Add(unitButton);
        }
    }*/
	
    public void ActivateUnitButtons(bool PC, bool activate) {
        unitButtons.FindAll(button => button.isPC == PC).ForEach(button => button.gameObject.SetActive(activate));
    }

	void Update(){
		if (SceneManager.GetActiveScene().name != "BattleReady") return;
		if (ReadyPanel == null) ReadyPanel = FindObjectOfType<BattleReadyPanel>();
		
		if(ReadyPanel.panelType == BattleReadyPanel.PanelType.Ether && Input.GetKeyDown(KeyCode.R))
			PickRandomCharactersAndSkills();
		
		if(!CanStart) 
			startButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
		else
			startButton.GetComponent<Image>().color = selectableUnitCounter.IsPartyFull() && IsEfficient ? Color.white : Color.red;
		
        if (RecentUnitButton != null && RecentUnitButton.gameObject.activeSelf)
            arrowIcon.transform.position = RecentUnitButton.transform.position;
        else {
            List<AvailableUnitButton> activeUnitButtons = unitButtons.FindAll(button => button.gameObject.activeSelf);
            if(activeUnitButtons.Count > 0)
                arrowIcon.transform.position = activeUnitButtons[0].transform.position;
        }

		if (!Input.GetMouseButtonDown(1) || ReadyPanel.panelType != BattleReadyPanel.PanelType.Briefing) return;
		ReadyPanel.mainPanel.SetActive(false);
		ReadyPanel.MBM.ToggleAllImages(false);
		ReadyPanel.conditionPanel.gameObject.SetActive(true);
	}

	public void PickRandomCharactersAndSkills(){
		foreach (var button in unitButtons)
			PickUnit(button, false);

		var unpicked = unitButtons.FindAll(button => button.isPC && !button.candidate.picked);
		while (!selectableUnitCounter.IsPartyFull()){
			var candToPick = Generic.PickRandom(unpicked);
			candToPick.OnClicked();
			candToPick.candidate.ResetSkills();
			unpicked.Remove(candToPick);
		}
		
		ResetAllPickedSkills();
		PickRandomSkills();
		ReadyPanel.SetAllSkillSelectButtons();
	}

	void ResetAllPickedSkills(){
		foreach (var cand in pickedList)
			cand.ResetSkills();
	}
	void PickRandomSkills(){
		int count = 0;
		while(pickedList.Any(cand => !cand.selectedSkills.Any(skill => skill is ActiveSkill)) && count < 100){
			count++;
			ResetAllPickedSkills();
			
			while (!IsEfficient){
				var pickableSkills = new List<Skill>();
				foreach (var cand in pickedList)
					pickableSkills.AddRange(VolatileData.SkillsOf(cand.CodeName, true).FindAll(skill => cand.IsThisPickable(skill)));
				
				if(pickableSkills.Count == 0) break;
				var skillToPick = Generic.PickRandom(pickableSkills);
				var skillOwner = pickedList.Find(cand => cand.CodeName == skillToPick.ownerName);
				Debug.Assert(skillOwner != null, skillToPick.Name + "의 주인이 없음!");
				skillOwner.selectedSkills.Add(skillToPick);
				pickableSkills.Remove(skillToPick);
			}
		}
		
		foreach (var unitButton in unitButtons)
			UpdateEtherText(unitButton);
	}

	//에디터상 UI 버튼으로 구현(Ref없어도 지우지 말 것)
	public void CheckToStartBattle(){
		if(!CanStart)
			SoundManager.Instance.PlaySE("Cannot");
		else if(IsEfficient)
			SaveSkillTreesAndStartBattle();
		else
			UIManager.Instance.Warn(SaveSkillTreesAndStartBattle, Language.Select("능력을 더 선택할 수 있습니다.\n\n정말 시작하시겠습니까?", "Can select more skills.\n\nAre you sure to start?"));
	}
	public bool CanStart{get { return RemainEther >= 0; }}

    void SaveSkillTreesAndStartBattle(){
	    RecordData.recentPicks.Clear();
	    foreach (var unit in pickedList){
		    RecordData.AddSkillTree(unit.CodeName, unit.selectedSkills);
		    RecordData.recentPicks.Add(unit.CodeName);
	    }
        FindObjectOfType<SceneLoader>().LoadBattleSceneFromBattleReadyScene();
    }

    public void OnMouseOverUnit(string unitName) {
        AvailableUnitButton button = unitButtons.Find(bu => bu.codeName == unitName);
	    if (button == null) return;
	    
	    RecentUnitButton.SetGray(true);
	    button.SetGray(false);
    }

    public void OnMouseOverUnitClicked(string unitName) {
        AvailableUnitButton button = unitButtons.Find(bu => bu.codeName == unitName);
        if (button != null)
            button.OnClicked();
    }

	public void PickFaction(Faction faction){
		Debug.Log(faction + " 선택");
		UnPickAll ();
		foreach (var button in unitButtons) {
			if (Utility.PCNameToFaction (button.codeName) == faction) {
				PickUnit(button, true);
			}
		}

		BattleData.selectedFaction = faction;
	}
	public void UnPickAll(){
		foreach (var button in unitButtons)
            PickUnit(button, false);
	}

	public void UpdateEtherTexts(){
		foreach (var unitButton in unitButtons)
			UpdateEtherText(unitButton);
	}
    public void UpdateEtherText(AvailableUnitButton unitButton){
	    if (!unitButton.isPC) return;
			unitButton.NameText.text = UnitInfo.ConvertName(unitButton.codeName) + "\n(" + unitButton.candidate.SumOfCost + ")";
    }
}
