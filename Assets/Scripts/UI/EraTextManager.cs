using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;
using UtilityMethods;

public class EraTextManager : MonoBehaviour {

	public Text KorEraTitle;
	public Text KorEraNumber;
	public Text KorEraDateAndLocation;
	public Text KorEraStory;

	public Text EngEraTitle;
	public Text EngEraNumber;
	public Text EngEraDateAndLocation;
	public Text EngEraStory;

	public void SetEraText(StageInfo stageInfo){
		Initialize();
		Lang currentLanguage = VolatileData.language;
		if (currentLanguage == Lang.Kor)
			SetKorEraText(stageInfo);
		else
			SetEngEraText(stageInfo);
	}

	void SetKorEraText(StageInfo stageInfo) {
		KorEraTitle.text = stageInfo.korTitle;
		KorEraNumber.text = "제" + (int)stageInfo.stage/10 + "장.";
		KorEraDateAndLocation.text = dateToString(stageInfo.date) + '\n' + "유니티 제국 수도 모나폴리";
		KorEraStory.text = stageInfo.korIntro;
	}

	void SetEngEraText(StageInfo stageInfo) {
		EngEraTitle.text = stageInfo.engTitle;
		EngEraNumber.text = "Chapter " + (int)stageInfo.stage/10 + ".";
		EngEraDateAndLocation.text = dateToString(stageInfo.date) + '\n' + "- Monapoli, The Capital of Unity Empire";
		EngEraStory.text = stageInfo.engIntro;
	}

	string dateToString(int input){
		if (input <= 13)
			return Language.Select("제국력 518년 1월 " + (input + 18) + "일", "- January " + (input+18) + ", 518");
		else
			return Language.Select("제국력 518년 2월 " + (input - 13) + "일", "- February " + (input-13) + ", 518");
	}

	void Initialize()
	{
		KorEraTitle.text = "";
		KorEraNumber.text = "";
		KorEraDateAndLocation.text = "";
		KorEraStory.text = "";

		EngEraTitle.text = "";
		EngEraNumber.text = "";
		EngEraDateAndLocation.text = "";
		EngEraStory.text = "";
	}
}
