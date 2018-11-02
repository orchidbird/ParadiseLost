using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;
using TMPro;
using System.Text.RegularExpressions;
using DG.DemiLib;
using GameData;
using UtilityMethods;

public class SkillUI : MonoBehaviour, IPointerEnterHandler{
	public Image iconSlot;
	public Skill mySkill;
	public SkillViewer skillViewer;
	public Unit owner;
    public UnitInfo ownerInfo; // BattleReady 씬 등 owner가 null일 때 사용

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		SetViewer (owner);
	}

	public void SetViewer(Unit owner){
		if (mySkill == null || skillViewer == null) return;
		
		skillViewer.UpdateSkillViewer (mySkill, owner, ownerInfo);
		EventSystem.current.SetSelectedGameObject(gameObject);
	}
}

public class SkillViewer : SkillUI{
	public Text viewerNameText;
	public Text costText;
	public Text cooldownText;
	public Image rangeType;
	public Text rangeText;
	public TextMeshProUGUI explainText;
	public GameObject FixText_1stRange;
	public GameObject FixText_2ndRange;
	public GameObject CellGrid;

	public void Initialize(){
		viewerNameText.text = "";
		costText.text = "";
		cooldownText.text = "";
		rangeType.sprite = VolatileData.GetIcon(IconSprites.Transparent);
		rangeText.text = "";
		explainText.text = "";
	}

	void OnDisable(){
		ShowKeywordExplain.Instance.Clear();
	}

	public void UpdateSkillViewer(Skill skill, Unit owner, UnitInfo ownerInfo = null){
        this.ownerInfo = ownerInfo;
        this.owner = owner;
		mySkill = skill;

		if (!skill.IsOpened){
			Initialize();
			var displayChapter = Math.Max(skill.RequireLevel, (int)Setting.passiveOpenStage / 10);
			explainText.text = "??? (" + Language.Select(displayChapter + "장에서 개방됩니다" , "Opened At Chapter " + displayChapter) + ")";
			return;
		}
		
		cooldownText.text = "";

		if(mySkill is ActiveSkill){
			var activeSkill = (ActiveSkill)mySkill;
			costText.text = "<color=#BE81F7>" + Language.Select("행동력", "AP") + " </color>";
			int originalCost = activeSkill.GetRequireAP ();
			if (owner != null) {
				int currentCost = owner.GetActualRequireSkillAP (activeSkill);
				if (currentCost < originalCost) {
					costText.text += "<color=#00FF00>" + currentCost + "</color>";
				} else if (currentCost > originalCost) {
					costText.text += "<color=red>" + currentCost + "</color>";
				} else {
					costText.text += "<color=#BE81F7>" + currentCost + "</color>";
				}
			}else
				costText.text += "<color=#BE81F7>" + originalCost + "</color>";

			int cooldown = activeSkill.GetCooldown();
			if(cooldown > 0)
				cooldownText.text = Language.Select("재사용 대기 " + cooldown + "턴", "Cooldown " + cooldown + " Turn");

			if (activeSkill.GetRangeType() == RangeType.Auto){
				rangeType.sprite = VolatileData.GetIcon(IconSprites.AutoSkill);
				rangeText.text = "";
			}else{
				rangeType.sprite = VolatileData.GetIcon(IconSprites.PointSkill);
				rangeText.text = GetFirstRangeText(activeSkill);
			}

			DisplaySecondRange ((ActiveSkill)mySkill);
			if (FixText_1stRange != null) {
				FixText_1stRange.SetActive(true);
				FixText_2ndRange.SetActive(true);
			}
		} else{
			Initialize();
			costText.text = "<color=#BE81F7>" + Language.Select("특성(자동 적용)", "Passive Skill") + " </color>";
			if (FixText_1stRange != null) {
				FixText_1stRange.SetActive(false);
				FixText_2ndRange.SetActive(false);
			}
		}

		viewerNameText.text = mySkill.Name;
        string text = mySkill.Explaination;
		explainText.text = AddSkillValueTexts(text, owner, ownerInfo);
		//explainText.text = AddSkillValueTexts(_String.ColorExplainText(text), owner, ownerInfo);
		FindObjectOfType<ShowKeywordExplain>().Show(text);
	}

	string GetFirstRangeText(ActiveSkill skill){
		string result = "";
		if(skill.firstRange.min > 1)
			result = skill.firstRange.min+"~";
		return result + skill.firstRange.max;
	}

    string AddSkillValueTextByPower(Match match) {
        float coef = 1;
        float basic = 0;
        coef = float.Parse(match.Groups[1].ToString());
        string basicString = match.Groups[3].ToString();
	    try{basic = float.Parse(basicString);} catch{}
	    
	    string result;
        if(owner != null) 
            result = ((int)new Formula(FormulaVarType.Power, coef, basic).Result(owner)).ToString();
        else if(ownerInfo != null)
            result = new Formula(FormulaVarType.Power, coef, basic).Result(ownerInfo);
        else result = new Formula(FormulaVarType.Power, coef, basic).Result(mySkill.owner);
	    return match + "</color>(<color=red>" + result + "</color>)";
    }
	
    string ReplaceSkillValueTextByLevel(Match match) {
        float coef = 1;
        float basic = 0;
		coef = float.Parse(match.Groups[3].ToString());
        string basicString = match.Groups[2].ToString();
        if (basicString != "")
            basic = float.Parse(basicString);

        string result;
        if (owner != null)
            result = ((int)(new Formula(FormulaVarType.Level, coef, basic).Result(owner))).ToString();
        else if (ownerInfo != null)
            result = new Formula(FormulaVarType.Level, coef, basic).Result(ownerInfo);
        else
	        result = new Formula(FormulaVarType.Level, coef, basic).Result(mySkill.owner);
        return /*match.Groups[1]*/ "<color=green>" + result + "</color>";
    }

    string AddSkillValueTexts(string text, Unit owner, UnitInfo ownerInfo){
		for (int i = 0; i < mySkill.textValueFormulas.Count; i++) {
			if(owner != null)
				text = text.Replace("VALUE" + (i + 1), ((int)mySkill.textValueFormulas[i].Result(owner)).ToString());
			else if(ownerInfo != null)
				text = text.Replace("VALUE" + (i + 1), mySkill.textValueFormulas[i].Result(ownerInfo));
			else
				text = text.Replace("VALUE" + (i + 1), mySkill.textValueFormulas[i].Result(mySkill.owner));
		}

        text = Regex.Replace(text, @"([\-0-9.]+)×(<sprite=[0-9]+>)?공격력\s*\+?([\-0-9.]*)", AddSkillValueTextByPower); // ex) "0.8x공격력"
	    text = Regex.Replace(text, @"([\-0-9.]+)×(<sprite=[0-9]+>)?Power\s*\+?([\-0-9.]*)", AddSkillValueTextByPower);
        text = Regex.Replace(text, @"(<color=[a-z]+>)?([\-0-9.]*)\s*\+?\s*레벨당\s*([\-0-9.]+)", ReplaceSkillValueTextByLevel);
	    text = Regex.Replace(text, @"(<color=[a-z]+>)?([\-0-9.]*)\s*\+?\s*Level×\s*([\-0-9.]+)", ReplaceSkillValueTextByLevel);
	    
        return text;
    }

	void DisplaySecondRange(ActiveSkill skill){
		int rowNum = 11;
		Dictionary<Vector2, Color> rangeColors = skill.RangeColorsForSecondRangeDisplay (rowNum);
		var cells = CellGrid.GetComponentsInChildren<Image>();
		foreach (var kv in rangeColors)
			cells[(int) (kv.Key.x * rowNum + kv.Key.y)].color = kv.Value;
	}
}
