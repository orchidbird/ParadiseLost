using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameData;
using System;
using Enums;
using Save;

public class GameDataManager
{
	public static void Reset(int i){
		RecordData.Reset();
		VolatileData.Reset();
		SaveAt(i, VolatileData.saveSlotName);
	}

	private static string ConvertGameDataToString()
	{
		// 첫 번째 줄은 세이브 슬롯 이름(이 메서드 이전에 추가됨)
		string text = "";

		// 두 번째 줄은 세이브 파일의 기본 정보(현재 다이얼로그 등)
		text += RecordData.progress.ToSaveString() + "\n";

		// 세 번째 줄은 클리어한 스테이지의 개수
		text += RecordData.stageClearRecords.Count + "\n";
		foreach (var kv in RecordData.stageClearRecords)
		{
			// 네 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
			// 한 줄에 스테이지와 도전 횟수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
			text += kv.Key + "," + kv.Value.Count + "\n";
			foreach (var record in kv.Value)
				text += record.ToSaveString() + "\n";
		}

		// 마지막 줄에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
		text += RecordData.customSkillTrees.Count + ",";
		foreach (var kv in RecordData.customSkillTrees)
		{
			string unitName = kv.Key;
			List<string> skillNames = kv.Value;

			text += unitName + ",";
			text += skillNames.Count + ",";
			foreach (var skillName in skillNames)
			{
				text += skillName + ",";
			}
		}
		text += "\n";

		// 마지막 줄은 해금된 각주들에 대한 정보
		// level>0 인 각주들에 대해 (index, level) 쌍들을 나열한다.
		foreach (var kv in RecordData.openedGlossaries)
		{
			text += kv.Key + "," + kv.Value.Count + ",";
			foreach (var indexLevelPair in kv.Value)
				text += indexLevelPair.Key + "," + indexLevelPair.Value + ",";
		}

		return text;
	}

	private static void ConvertStringToGameData(string str)
	{
		StringParser lineParser = new StringParser(str, '\n');
		RecordData.Reset();

		// 첫 번째 줄은 세이브 슬롯 이름
		VolatileData.saveSlotName = lineParser.ConsumeString();

		// 두 번째 줄은 세이브 파일의 기본 정보(레벨, 현재 스테이지 및 현재 다이얼로그 등)
		StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');

		RecordData.progress = new Progress(commastringParser.ConsumeString());
		VolatileData.progress = new Progress();
		VolatileData.progress.Clone(RecordData.progress);

		// 세 번째 줄은 클리어한 스테이지의 개수
		int stageNum = int.Parse(lineParser.ConsumeString());
		for (int i = 0; i < stageNum; i++)
		{
			// 네 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
			// 한 줄에 스테이지와 도전 횟수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
			commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			StageNum stage = (StageNum)commastringParser.ConsumeInt();
			int clearCount = commastringParser.ConsumeInt();
			for (int j = 0; j < clearCount; j++)
			{
				RecordData.RecordStageClear(stage, new StageClearRecord(lineParser.ConsumeString()));
			}
		}

		// 마지막에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
		commastringParser = new StringParser(lineParser.ConsumeString(), ',');
		int unitNum = commastringParser.ConsumeInt();
		for (int i = 0; i < unitNum; i++)
		{
			string unitName = commastringParser.ConsumeString();
			int skillNum = commastringParser.ConsumeInt();
			for (int j = 0; j < skillNum; j++)
				RecordData.AddSkillTree(unitName, commastringParser.ConsumeString());
		}

		// 마지막 줄은 해금된 각주들에 대한 정보
		// level>0 인 각주들에 대해 (type, index, level) 쌍들을 나열한다.
		commastringParser = new StringParser(lineParser.ConsumeString(), ',');
		if (commastringParser.origin[0] == "")
			return;
		foreach (GlossaryType i in Enum.GetValues(typeof(GlossaryType)))
		{
			GlossaryType type = commastringParser.ConsumeEnum<GlossaryType>();
			int num = commastringParser.ConsumeInt();
			for (int j = 0; j < num; j++)
			{
				int index = commastringParser.ConsumeInt();
				int level = commastringParser.ConsumeInt();
				RecordData.openedGlossaries[type].Add(index, level);
			}
		}
	}

	public static void Save(){
		GameMode gameMode = VolatileData.gameMode;
		if (gameMode == GameMode.Story || gameMode == GameMode.Challenge){
			int i = VolatileData.currentSaveSlotIndex;
			SaveAt(i, RecordData.progress.ToSaveString());
		}else if (gameMode == GameMode.Test)
			SaveAt(5, "Test");
	}

	public static void SaveAt(int i, string name){
		string filePath = SaveDataPath.GetPathWithIndex(i);
		string data = SaveFile.ComposeSaveFile(name);
		File.WriteAllText(filePath, data, Encoding.UTF8);
		VolatileData.currentSaveSlotIndex = i;
	}

	public static void RecordPlayLog(PlayLog playLog, int i = -1) {
		Type type = playLog.GetType();
		if(i == -1)
			i = VolatileData.currentSaveSlotIndex;
		string data = "{}\n";
		if(type == typeof(BattleEndPlayLog))
			data = "BattleEnd : " + JsonUtility.ToJson((BattleEndPlayLog)playLog, true) + "\n";
		if (type == typeof(BattleStartPlayLog))
			data = "BattleStart : " + JsonUtility.ToJson((BattleStartPlayLog) playLog, true) + "\n";
		if (type == typeof(SpentTimePlayLog)) {
			SpentTimePlayLog spentTimePlayLog = ((SpentTimePlayLog)playLog);
			data = "SpentTime : " + JsonUtility.ToJson(spentTimePlayLog, true) + "\n";
			if(spentTimePlayLog.sceneName != "Title")
				RecordData.totalPlayingTime += spentTimePlayLog.spentTime;
		}
		string filePath = SaveDataPath.GetPlayLogPathWithIndex(i);
		File.AppendAllText(filePath, data, Encoding.UTF8);
		VolatileData.currentSaveSlotIndex = i;
	}

	public static void RemoveEntirePlayLog(int i) {
		string filePath = SaveDataPath.GetPlayLogPathWithIndex(i);
		File.Delete(filePath);
	}

	public static void ForkEntirePlayLog(int from, int to) {
		string fromFilePath = SaveDataPath.GetPlayLogPathWithIndex(from);
		string toFilePath = SaveDataPath.GetPlayLogPathWithIndex(to);
		string data = File.ReadAllText(fromFilePath);
		File.WriteAllText(toFilePath, data);
	}

	public static void Load(int i){
		string filePath = SaveDataPath.GetPathWithIndex(i);
		if (!File.Exists(filePath)) {
			Debug.Log("Save does not exist, new save file created at " + filePath);
			Reset(i);
			return;
		}

		Debug.Log("Save is loaded from " + filePath);
		string data = File.ReadAllText(filePath, Encoding.UTF8);
		SaveFile.ParseSaveFile(data);
		
		VolatileData.currentSaveSlotIndex = i;
	}
}
