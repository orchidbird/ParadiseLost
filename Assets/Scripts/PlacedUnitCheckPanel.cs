using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using UtilityMethods;

public class PlacedUnitCheckPanel : MonoBehaviour {
	public List<Image> unitPortraitList = new List<Image>();
	List<Candidate> selectedUnitList = new List<Candidate>();
	public Text text;

	public void SetUnitPortraits (List<Candidate> unitList) {
		selectedUnitList = unitList;
		for (int i = 0; i < unitList.Count; i++){
			Sprite unitPortraitSprite = GameData.VolatileData.GetSpriteOf(SpriteType.Portrait, unitList[i].CodeName);
			unitPortraitList[i].sprite = unitPortraitSprite;
		} 
	}

	public void HighlightPortrait (string unitString) {
		unitPortraitList.ForEach(img => img.color = Color.gray);
		var targetPortraitIndex = selectedUnitList.IndexOf(selectedUnitList.Find(unit => unit.CodeName == unitString));
		if (targetPortraitIndex != -1)
			unitPortraitList[targetPortraitIndex].color = Color.white;
	}

	public void ResetHighlight(){
		unitPortraitList.ForEach(img => img.color = Color.gray);
		unitPortraitList[0].color = Color.white;
	}

	public void SetText(string inputText) {
		text.text = inputText;
	}

	// Use this for initialization
	void Start () {
		text.text = Language.Select("시작 위치를 선택해주세요", "Select the starting points.");
	}
}
