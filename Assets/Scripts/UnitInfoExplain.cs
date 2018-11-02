using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;
using TMPro;
using GameData;
using UnityEngine.SceneManagement;
using UtilityMethods;

public class UnitInfoExplain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
	public UnitInfoUI viewer;
	public bool forStat;
	public Stat stat;
	public GameObject ExplainPanel;
	public Image IconImage;
	TextMeshProUGUI textUI;

	void Awake(){
		if (stat == Stat.Will && !VolatileData.OpenCheck(Setting.WillChangeOpenStage))
			gameObject.SetActive(false);
	}
	
	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		if (SceneManager.GetActiveScene().name != "Battle") return;
		
		ExplainPanel.SetActive(true);
		ExplainPanel.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + new Vector3(0, 0.2f, 0);
		textUI = ExplainPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		if(forStat){
			int currentStat = 0;
			try{
				currentStat = int.Parse(GetComponentInChildren<Text>().text);
			}catch{
				currentStat = viewer.unit.GetStat(stat);
			}
			
			var colorCode = "red";
			if(stat != Stat.None)
				textUI.text = Language.Find(stat + "Explain");
			
			if (stat == Stat.Defense){
				colorCode = "green";
				textUI.text += floatToPercent(-(float) currentStat / (currentStat + Setting.defenseHalfLevel), false, 1);
			}else if (stat == Stat.Will){
				colorCode = "#00FFFF";
				textUI.text += floatToPercent((100 - currentStat) / (float) currentStat, false, 1);
			}if (stat == Stat.Agility)
				textUI.text += "턴마다 행동력을 " + currentStat + " 회복 / 턴을 종료할 때 " + (currentStat/2) + "을(를) 넘는 행동력은 소멸";
			
			textUI.text += "\n기본값: " + viewer.unit.GetBaseStat (stat);
			if (viewer != null){
				foreach (var statChange in viewer.unit.GetStatChangeList(stat))
					textUI.text += "\n<color=" + colorCode + ">" +
					               (stat == Stat.Power ? floatToPercent(statChange.value, true) : ValueChangeText ((int)statChange.value)) 
					               + "</color> (" + statChange.reason + ")";
			}

			textUI.text = _String.ColorExplainText(textUI.text);
		}else{
			if (IconImage.sprite == VolatileData.GetIcon(IconSprites.Transparent)) return;
			
			textUI.text = Language.Find(IconImage.sprite.name + "Explain");
			if(IconImage.sprite == VolatileData.GetIcon(Element.Metal))
				textUI.text += new Formula(FormulaVarType.Level, 0.7f, 53).Result(unitInfo: null) + Language.Select("", "on Metal tile");			
			textUI.text = _String.ColorExplainText(textUI.text);
		}
	}

	string floatToPercent(float input, bool color, int roundDigit = 0){
		input = (float)Math.Round(input*100, roundDigit);
		if (input == 0) return "불변";
		
		var result = input + "%";
		if(input > 0) result = "+" + result;

		return color ? ColorText(result, input > 0 ? "green" : "red") : result;
	}

	string ValueChangeText(int input){
		if (input == 0) return "불변";
		return input > 0 ? ColorText("+" + input, "green") : ColorText(input+"", "red");
	}

	string ColorText(string input, string colorCode){
		return "<color=" + colorCode + ">" + input + "</color>";
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		ExplainPanel.SetActive(false);
	}
}
