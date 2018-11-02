using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;
using Camerawork;
using UtilityMethods;
using Language = UtilityMethods.Language;

public class Parser : MonoBehaviour{
	public static List<string[]> GetMatrixTableFrom(string resourceAddress){
		string input = Resources.Load<TextAsset>(resourceAddress).text;
		List<string> rowDataList = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		return rowDataList.ConvertAll(row => row.Split('\t'));
	}
	
	//각 행 중에 첫번째 항목 searchigWord로 시작하는 행을 찾아서 return
	public static string[] FindRowDataOf(string text, string searchingWord, char separator = '\t'){
		string[] RowDataStrings = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		foreach(string row in RowDataStrings){
			string[] tempRowData = row.Split(separator);
			if(tempRowData[0] == searchingWord){
				return tempRowData;
			}
		}

		//Debug.Log("RowData Not Found : " + searchingWord + "in " + text);
		return null;
	}
	
	public static string[] FindRowOf(List<string[]> table, string searchingWord){
		return table.Find(item => item[0] == searchingWord);
	}

	public static List<T> GetParsedData<T>(){
		var dataList = new List<T>();
		var textAsset = GetDataAddress<T>();
		if(textAsset == null)
			return null;
		string[] rowDataList = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for(var i = 1; i < rowDataList.Length; i++){
			var data = CreateParsedObject<T>(rowDataList[i]);
			dataList.Add(data);
		}
		return dataList;
	}

	public static T CreateParsedObject<T>(string rowData){
		if (typeof(T) == typeof(DialogueData))
			return (T)(object)new DialogueData(rowData);
		if (typeof(T) == typeof(TutorialScenario))
			return (T)(object)new TutorialScenario(rowData);
		if (typeof(T) == typeof(AIScenario))
			return (T)(object)new AIScenario(rowData);
		if (typeof(T) == typeof(UnitStatusEffectInfo))
			return (T)(object)new UnitStatusEffectInfo(rowData);
		if (typeof(T) == typeof(TileStatusEffectInfo))
			return (T)(object)new TileStatusEffectInfo(rowData);
		if (typeof(T) == typeof(ActiveSkill))
			return (T)(object)new ActiveSkill(rowData);
		if (typeof(T) == typeof(PassiveSkill))
			return (T)(object)new PassiveSkill(rowData);
		if (typeof(T) == typeof(BattleTrigger))
			return (T)(object)BattleTriggerFactory.Get(rowData);
		if (typeof(T) == typeof(AIInfo))
			return (T)(object)new AIInfo(rowData);
		if (typeof(T) == typeof(UnitInfo))
			return (T)(object)new UnitInfo(rowData, false);
		if (typeof(T) == typeof(UnitGenInfo))
			return (T)(object)new UnitGenInfo(rowData);
		if(typeof(T) == typeof(CameraWork))
			return (T)(object)new CameraWork(rowData);
		if (typeof(T) == typeof(StageInfo))
			return (T)(object)new StageInfo(rowData);

		Debug.LogError("Invalid Input");
		return (T) (object) null; //컴파일할 때 뭔가 리턴해야 해서 만듦
	}

	private static TextAsset GetDataAddress<T>(){
		var address = "";
		var stageName = "Stage" + (int)VolatileData.progress.stageNumber; 
        //if(VolatileData.gameMode == GameMode.Test) stageName = "test";
		
        if (typeof(T) == typeof(GlossaryData)) {address = "Data/Glossary";}
		else if (typeof(T) == typeof(DialogueData)){
	        var basePath = "Data/" + VolatileData.progress.dialogueName; 
	        return Resources.Load<TextAsset>(basePath + Language.Select("", "-en")) ?? Resources.Load<TextAsset>(basePath);
        }
		else if(typeof(T) == typeof(TutorialScenario)) {address = "Tutorial/" + SceneManager.GetActiveScene().name + (int)VolatileData.progress.stageNumber;}
		else if(typeof(T) == typeof(AIScenario)) {address = "Tutorial/" + SceneManager.GetActiveScene ().name + (int)VolatileData.progress.stageNumber + "_AI";}
		else if(typeof(T) == typeof(UnitStatusEffectInfo)) {address = "Data/UnitStatusEffectData";}
        else if(typeof(T) == typeof(TileStatusEffectInfo)) {address = "Data/TileStatusEffectData";}
		else if(typeof(T) == typeof(ActiveSkill)) {address = "Data/ActiveSkillData";}
		else if(typeof(T) == typeof(PassiveSkill)) {address = "Data/PassiveSkillData";}
		else if(typeof(T) == typeof(BattleTrigger)) {address = "Data/" + stageName + "_battleTrigger";}
		else if (typeof(T) == typeof(AIInfo)) {address = dataAddressIncludingAB(stageName, "AI");}
        else if(typeof(T) == typeof(UnitGenInfo)) {address = dataAddressIncludingAB(stageName, "position");}
        else if(typeof(T) == typeof(TileInfo)) {address = "Data/" + stageName + "_map";}
		else if(typeof(T) == typeof(CameraWork)) { address = "Data/" + stageName + "_cameraWork";}
        else if(typeof(T) == typeof(StageInfo)) {address = "Data/StageInfo";}
        else if (typeof(T) == typeof(UnitInfo)){
	        return GetAddressWithDifficulty(dataAddressIncludingAB(stageName, "unit"));
        }
		if(address == "") {Debug.LogError("Invalid Input : " + typeof(T));}
		try {
			return Resources.Load<TextAsset>(address);
		} catch {
			Debug.Log(typeof(T) + " of " + stageName + " not found. returning null");
			return null;
		}
	}

	static TextAsset GetAddressWithDifficulty(string input){
		var defaultResult = Resources.Load<TextAsset>(input);
		if (VolatileData.difficulty == Difficulty.Adventurer)
			return defaultResult;
		if (VolatileData.difficulty == Difficulty.Intro)
			return GetTextAssetOfDifficulty(input, "easy", defaultResult);
		if (VolatileData.difficulty == Difficulty.Tactician)
			return GetTextAssetOfDifficulty(input, "hard", defaultResult);
		if (VolatileData.difficulty == Difficulty.Legend)
			return GetTextAssetOfDifficulty(input, "legend", defaultResult);
		Debug.LogError("난이도 설정 오류!");
		return null;
	}

	static TextAsset GetTextAssetOfDifficulty(string input, string difficultyCode, TextAsset defaultResult){
		var result = Resources.Load<TextAsset>(input + "_" + difficultyCode) ?? defaultResult;
		UnitInfo.SetDifficultyFactors(result != defaultResult);
		return result;
	}
	
	public static List<TileInfo> GetParsedTileInfo(){
		List<TileInfo> tileInfoList = new List<TileInfo>();

		TextAsset csvFile = GetDataAddress<TileInfo>();
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int reverseY = unparsedTileInfoStrings.Length -1; reverseY >= 0 ; reverseY--){
			string[] parsedTileInfoStrings = unparsedTileInfoStrings[reverseY].Split('\t');
			for (int x = 1; x <= parsedTileInfoStrings.Length; x++){
				Vector2 tilePosition = new Vector2(x, unparsedTileInfoStrings.Length - reverseY);
				var tileInfo = new TileInfo(tilePosition, parsedTileInfoStrings[x-1]);
				if(!tileInfo.IsEmptyTile())
					tileInfoList.Add(tileInfo);
			}
		}

		return tileInfoList;
	}

	public static List<GlossaryData> GetParsedGlossaryData(){
		var glossaryDataList = new List<GlossaryData>();
		string[] unparsedTileInfoStrings = GetDataAddress<GlossaryData>().text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 1; i < unparsedTileInfoStrings.Length - 1; i += 2){
			glossaryDataList.Add(new GlossaryData(unparsedTileInfoStrings[i], unparsedTileInfoStrings[i+1]));
		}

		return glossaryDataList;
	}

	private static string dataAddressIncludingAB(string stageName, string typeName){
		var address = "Data/" + stageName;
		
		if(VolatileData.stageData.IsTwoSideStage()){
			if (BattleData.selectedFaction == Faction.Pintos) {
				address += "A";
			} else {
				address += "B";
			}
		}

		return address + "_" + typeName;
	}
	
	private static List<Skill> skillInfosCache;
}
