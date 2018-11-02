using Enums;
using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UtilityMethods;

public class DetailInfoPanel : UnitInfoUI{
	public Image illust;
    public List<SkillInfoButton> skillButtons = new List<SkillInfoButton>();
	public TextMeshProUGUI WillCharacteristic;

	public void InitializeInBattleScene(){
		//SetIllust(unit.myInfo);
		SetCommonUnitInfoUI();
        SetSkillButtons(unit.GetActiveSkillList(), unit.GetPassiveSkillList());
		
		if (unit.IsPC && VolatileData.OpenCheck(Setting.WillCharacteristicOpenStage)){
			WillCharacteristic.text = "";
			WillCharacteristic.transform.parent.gameObject.SetActive(true);
			var properties = UnitInfo.GetWillCharacteristics(unit.CodeName);
			foreach (var kv in properties){
				if (!kv.Value) continue;
				var row = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/WillCharacteristic_Explain").text, kv.Key.ToString());
				Debug.Assert(row != null, kv.Key + "는 설명이 존재하지 않음!");
				WillCharacteristic.text += "<color=yellow>" + row[1+(int)VolatileData.language] + "</color>: " + row[3+(int)VolatileData.language] + "\n";
			}
		}else
			WillCharacteristic.transform.parent.gameObject.SetActive(false);
	}
	/*public void InitializeInReadyScene(UnitInfo info){
		SetIllust(info);
		SetCommonUnitInfoUIWithUnitInfo(info);
		var skills = VolatileData.SkillsOf(info.codeName, false);
		var activeSkills = skills.FindAll(skill => skill is ActiveSkill).ConvertAll(skill => (ActiveSkill) skill);
		var passiveSkills = skills.FindAll(skill => skill is PassiveSkill).ConvertAll(skill => (PassiveSkill) skill);
		
        SetSkillButtons(activeSkills, passiveSkills, info);
		WillCharacteristic.transform.parent.gameObject.SetActive(false);
	}

	void SetIllust(UnitInfo info){
		illust.sprite = VolatileData.GetSpriteOf(SpriteType.Illust, _String.GeneralName(info.codeName)) ??
		                VolatileData.GetIcon(IconSprites.Transparent);
	}*/

	void SetSkillButtons(List<ActiveSkill> actives, List<PassiveSkill> passives, UnitInfo info = null){
		Debug.Log("Passive : " + passives.Count + ", Active : " + actives.Count);
		skillButtons.ForEach(button => button.gameObject.SetActive(true));

		//고유 특성(여기서는 레벨제한이 가장 낮은 특성으로 정의)은 맨 앞에 표시
		if (passives.Count > 0){
			PassiveSkill uniquePassive = passives.OrderBy(skill => skill.RequireLevel).First();
			skillButtons.First ().Initialize (uniquePassive, unit, info);
		}else
			skillButtons.First().gameObject.SetActive(false);
		
		// 나머지 능력을 기술(Active) -> 특성(Passive) 순서로 표시
		for (int i = 1; i <= 10; i++){
			if (i <= actives.Count){
				skillButtons [i].Initialize (actives[i - 1], unit, info);
			}else if(i < actives.Count + passives.Count){
				skillButtons [i].Initialize (passives[i - actives.Count], unit, info);
			}else{
				skillButtons[i].gameObject.SetActive(false);
			}
		}

		// 스킬 상세설명 초기화
		SkillInfoButton skillButton = skillButtons.Find(button => button.isActiveAndEnabled);
		if (skillButton != null){
			skillButton.ownerInfo = info;
			skillButton.GetComponent<SkillInfoButton> ().SetViewer (unit);
			EventSystem.current.SetSelectedGameObject(skillButton.gameObject);
		}else
			FindObjectOfType<SkillViewer>().Initialize();
	}

	void OnDisable(){
		if(BattleManager.Instance != null)
			BattleUIManager.Instance.deactivateDetailInfoEvent.Invoke();
	}
}

public class UnitInfoUI : MonoBehaviour{
	public Unit unit;
	public Text unitName;
	public Dictionary<Stat, Text> statTextDict = new Dictionary<Stat, Text>();
	public Text HpText;
	public Text ApText;
	public Text powerText;
	public Text defenseText;
	public Text WillText;

	void Awake(){
		if(statTextDict.Count < 5){
			statTextDict.Add(Stat.CurrentHP, HpText);
			statTextDict.Add(Stat.CurrentAP, ApText);
			statTextDict.Add(Stat.Power, powerText);
			statTextDict.Add(Stat.Defense, defenseText);
			statTextDict.Add(Stat.Will, WillText);
		}
		AfterAwake();
	}

	public virtual void AfterAwake(){}

	protected void SetCommonUnitInfoUI(){
		unitName.text = UnitInfo.ConvertName(unit.codeName);
		HpText.text = unit.GetHP + " / " + unit.GetStat(Stat.MaxHealth);
		ApText.text = unit.GetCurrentActivityPoint() + " / " + unit.GetStat(Stat.Agility);
		UpdateStat(Stat.Power);
		UpdateStat(Stat.Defense);
		UpdateStat(Stat.Will);
	}

	/*protected void SetCommonUnitInfoUIWithUnitInfo(UnitInfo info){
		unitName.text = UnitInfo.ConvertName(info.codeName);
		HpText.text = info.baseStats[Stat.MaxHealth].ToString();
		ApText.text = info.baseStats[Stat.Agility].ToString();
		for (int i = 2; i <= 4; i++){
			var statType = (Stat) i;
			statTextDict[statType].text = info.baseStats[statType].ToString();
		}
		WillText.gameObject.SetActive(false);
		Utility.SetPropertyImages(classImage, elementImage, info);
	}*/

	void UpdateStat(Stat statType){
		int actual = unit.GetStat(statType);
		int origin = unit.GetBaseStat(statType);
		if (statType == Stat.Will)
			origin = 100;
		Text targetText = statTextDict[statType];
		
		targetText.text = actual.ToString();
		if(actual > origin) targetText.color = Color.green;
		else if(actual < origin) targetText.color = Color.red;
		else targetText.color = Color.white;
	}
}
