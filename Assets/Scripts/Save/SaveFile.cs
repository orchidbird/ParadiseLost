using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameData;
using System;
using Enums;

namespace Save {
	public class SaveFile {
		// 2018.0804   : 이미 읽은 튜토리얼들에 대한 정보 추가
		// 2018.0804.1 : Configuration들에 대한 정보 추가
		// 2018.0930   : steam 업적 진행상황에 대한 정보 추가
		
		delegate void SaveFileParser(StringParser parser);
		delegate string SaveFileComposer();
		const string RECENT_SAVEFILE_VERSION = "2018.1021";
		static Dictionary<string, Delegate> versionComposerDict = new Dictionary<string, Delegate> {
			{"0", new SaveFileComposer(SaveFileComposer_V0_0)},
			{"1", new SaveFileComposer(SaveFileComposer_V1_0)},
			{"2018.0804", new SaveFileComposer(SaveFileComposer_V2018_0804)},
			{"2018.0804.1", new SaveFileComposer(SaveFileComposer_V2018_0804_1) },
			{"2018.0930", new SaveFileComposer(SaveFileComposer_V2018_0930) },
			{"2018.1021", new SaveFileComposer(SaveFileComposer_V2018_1021) }
		};
		static Dictionary<string, Delegate> versionParserDict = new Dictionary<string, Delegate> {
			{"0", new SaveFileParser(SaveFileParser_V0_0)},
			{"1", new SaveFileParser(SaveFileParser_V1_0)},
			{"2018.0804", new SaveFileParser(SaveFileParser_V2018_0804)},
			{"2018.0804.1", new SaveFileParser(SaveFileParser_V2018_0804_1) },
			{"2018.0930", new SaveFileParser(SaveFileParser_V2018_0930) },
			{"2018.1021", new SaveFileParser(SaveFileParser_V2018_1021) }
		};

		// parameter로 version이 명시되지 않았을 시 가장 최근 버전 양식으로 saveFile을 GameData로부터 compose하여 저장함.
		public static string ComposeSaveFile(string name, string version = RECENT_SAVEFILE_VERSION) {
			return name + "\n" + "SaveFile Version:" + version + "\n" + versionComposerDict[version].DynamicInvoke();
		}

		// saveFile의 첫 번째 줄에는 그 saveFile 양식의 버전이 쓰여있으며, 쓰여있지 않을 시 0.0 버전임.
		// 그 버전에 따라 알맞은 parser를 호출하여 saveFile을 GameData로 parsing함.
		public static void ParseSaveFile(string data) {
			StringParser lineParser = new StringParser(data, '\n');

			// 첫 번째 줄은 세이브파일 이름
			lineParser.ConsumeString();

			// 두 번째 줄은 세이브파일 버전
			string versionInfo = lineParser.ConsumeString();
			string version = "0";
			if (versionInfo.StartsWith("SaveFile Version")) {
				version = versionInfo.Split(':')[1];
			} else {
				lineParser.CancelLastParse();
			}
			if (!versionParserDict.ContainsKey(version)) {
				Debug.LogError("세이브파일 버전 " + version + "에 알맞은 Parser가 없음");
				return;
			}
			versionParserDict[version].DynamicInvoke(lineParser);
		}

		static string SaveFileComposer_V2018_1021() {
			string text = SaveFileComposer_V2018_0930() + "\n";
			foreach(Warning.WarningType warningType in Enum.GetValues(typeof(Warning.WarningType))) {
				text += warningType + ":" + Configuration.ignoreWarning[warningType] + ",";
			}
			return text;
		}

		static void SaveFileParser_V2018_1021(StringParser lineParser) {
			SaveFileParser_V2018_0930(lineParser);
			string line = lineParser.ConsumeString();
			StringParser commandStringParser = new StringParser(line, ',');
			char[] seperators = new char[] { ',' };

			foreach (Warning.WarningType warningType in Enum.GetValues(typeof(Warning.WarningType))) {
				if (!Configuration.ignoreWarning.ContainsKey(warningType)) {
					Configuration.ignoreWarning.Add(warningType, false);
				}
			}
			foreach (var token in line.Split(seperators, options: StringSplitOptions.RemoveEmptyEntries)){
				string[] kv = token.Split(':');
				Warning.WarningType warningType = (Warning.WarningType) Enum.Parse(typeof(Warning.WarningType), kv[0]);
				bool value = bool.Parse(kv[1]);
				Configuration.ignoreWarning[warningType] = value;
			}
		}

		static string SaveFileComposer_V2018_0930() {
			string text = SaveFileComposer_V2018_0804_1() + "\n";
			text += RecordData.neutralizeEnemyCount + "," +
				RecordData.critCount;
			return text;
		}
		static void SaveFileParser_V2018_0930(StringParser lineParser) {
			SaveFileParser_V2018_0804_1(lineParser);
			StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			RecordData.neutralizeEnemyCount = commastringParser.ConsumeInt();
			RecordData.critCount = commastringParser.ConsumeInt();
		}
		static string SaveFileComposer_V2018_0804_1() {
			string text = SaveFileComposer_V2018_0804() + "\n";
			text += VolatileData.difficulty + "," +
				Configuration.textObjectDuration + "," +
				Configuration.SEVolume + "," +
				Configuration.BGMVolume + "," + 
				Configuration.NPCBehaviourDuration + "," +
				Configuration.showRealBlood;
			return text;
		}

		static void SaveFileParser_V2018_0804_1(StringParser lineParser) {
			SaveFileParser_V2018_0804(lineParser);
			StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			VolatileData.difficulty = commastringParser.ConsumeEnum<Difficulty>();
			Configuration.textObjectDuration = commastringParser.ConsumeFloat();
			Configuration.SEVolume = commastringParser.ConsumeFloat();
			Configuration.BGMVolume = commastringParser.ConsumeFloat();
			Configuration.NPCBehaviourDuration = commastringParser.ConsumeFloat();
			Configuration.showRealBlood = commastringParser.ConsumeBool();
		}

		static string SaveFileComposer_V2018_0804() {
			string text = SaveFileComposer_V1_0() + "\n";
			text += RecordData.alreadyReadTutorials.Count + ",";
			foreach(var readTutorial in RecordData.alreadyReadTutorials)
				text += readTutorial + ",";
			return text;
		}

		static void SaveFileParser_V2018_0804(StringParser lineParser) {
			SaveFileParser_V1_0(lineParser);
			RecordData.alreadyReadTutorials = new List<string>();
			StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			int num = commastringParser.ConsumeInt();
			for(int i = 0; i < num; i++) {
				RecordData.alreadyReadTutorials.Add(commastringParser.ConsumeString());
			}
		}

		static string SaveFileComposer_V1_0() {
			// 1, 2번째 줄은 세이브파일 이름, 버전(이 메서드 이전에 추가됨)
			string text = "";

			// 세 번째 줄은 세이브 파일의 기본 정보(현재 다이얼로그 등)
			text += RecordData.progress.ToSaveString() + "\n";

			// 네 번째 줄은 playing time
			text += RecordData.totalPlayingTime + "\n";

			// 다섯 번째 줄은 클리어한 스테이지의 개수
			text += RecordData.stageClearRecords.Count + "\n";
			foreach (var kv in RecordData.stageClearRecords) {
				// 여섯 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
				// 한 줄에 스테이지와 도전 횟수, 최고점수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
				text += kv.Key + "," + kv.Value.Count + "," + kv.Value.Max(record => record.score) + "\n";
				foreach (var record in kv.Value)
					text += record.ToSaveString() + "\n";
			}

			// 마지막 줄에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
			text += RecordData.customSkillTrees.Count + ",";
			foreach (var kv in RecordData.customSkillTrees) {
				string unitName = kv.Key;
				List<string> skillNames = kv.Value;

				text += unitName + ",";
				text += skillNames.Count + ",";
				foreach (var skillName in skillNames) {
					text += skillName + ",";
				}
			}
			text += "\n";

			// 마지막 줄은 해금된 각주들에 대한 정보
			// level>0 인 각주들에 대해 (index, level) 쌍들을 나열한다.
			foreach (var kv in RecordData.openedGlossaries) {
				text += kv.Key + "," + kv.Value.Count + ",";
				foreach (var indexLevelPair in kv.Value)
					text += indexLevelPair.Key + "," + indexLevelPair.Value + ",";
			}

			return text;
		}

		static void SaveFileParser_V1_0(StringParser lineParser) {
			RecordData.Reset();
			// 1, 2번째 줄은 세이브파일 이름, 버전(이 메서드 이전에 파싱함)

			// 세 번째 줄은 세이브 파일의 기본 정보(레벨, 현재 스테이지 및 현재 다이얼로그 등)
			StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');

			RecordData.progress = new Progress(commastringParser.ConsumeString());
			VolatileData.progress = new Progress();
			VolatileData.progress.Clone(RecordData.progress);

			// 네 번째 줄은 playing time
			RecordData.totalPlayingTime = float.Parse(lineParser.ConsumeString());

			// 다섯 번째 줄은 클리어한 스테이지의 개수
			StageNum stageNum = (StageNum)(int.Parse(lineParser.ConsumeString()));
			for (int i = 0; i < (int)stageNum; i++) {
				// 여섯 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
				// 한 줄에 스테이지와 도전 횟수, 최고 점수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
				commastringParser = new StringParser(lineParser.ConsumeString(), ',');
				StageNum stage = commastringParser.ConsumeEnum<StageNum> ();
				int clearCount = commastringParser.ConsumeInt();
				commastringParser.ConsumeInt();
				for (int j = 0; j < clearCount; j++) {
					RecordData.RecordStageClear(stage, new StageClearRecord(lineParser.ConsumeString()));
				}
			}

			// 마지막에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
			commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			int unitNum = commastringParser.ConsumeInt();
			for (int i = 0; i < unitNum; i++) {
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
			foreach (GlossaryType i in Enum.GetValues(typeof(GlossaryType))) {
				GlossaryType type = commastringParser.ConsumeEnum<GlossaryType>();
				int num = commastringParser.ConsumeInt();
				for (int j = 0; j < num; j++) {
					int index = commastringParser.ConsumeInt();
					int level = commastringParser.ConsumeInt();
					RecordData.openedGlossaries[type].Add(index, level);
				}
			}
		}


		static string SaveFileComposer_V0_0() {
			// 1, 2번째 줄은 세이브파일 이름, 버전(이 메서드 이전에 추가됨)
			string text = "";

			// 세 번째 줄은 세이브 파일의 기본 정보(현재 다이얼로그 등)
			text += RecordData.progress.ToSaveString() + "\n";

			// 네 번째 줄은 클리어한 스테이지의 개수
			text += RecordData.stageClearRecords.Count + "\n";
			foreach (var kv in RecordData.stageClearRecords) {
				// 다섯 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
				// 한 줄에 스테이지와 도전 횟수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
				text += kv.Key + "," + kv.Value.Count + "\n";
				foreach (var record in kv.Value)
					text += record.ToSaveString() + "\n";
			}

			// 마지막 줄에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
			text += RecordData.customSkillTrees.Count + ",";
			foreach (var kv in RecordData.customSkillTrees) {
				string unitName = kv.Key;
				List<string> skillNames = kv.Value;

				text += unitName + ",";
				text += skillNames.Count + ",";
				foreach (var skillName in skillNames) {
					text += skillName + ",";
				}
			}
			text += "\n";

			// 마지막 줄은 해금된 각주들에 대한 정보
			// level>0 인 각주들에 대해 (index, level) 쌍들을 나열한다.
			foreach (var kv in RecordData.openedGlossaries) {
				text += kv.Key + "," + kv.Value.Count + ",";
				foreach (var indexLevelPair in kv.Value)
					text += indexLevelPair.Key + "," + indexLevelPair.Value + ",";
			}

			return text;
		}

		static void SaveFileParser_V0_0(StringParser lineParser) {
			RecordData.Reset();
			// 1, 2번째 줄은 세이브파일 이름, 버전(이 메서드 이전에 파싱함)

			// 세 번째 줄은 세이브 파일의 기본 정보(레벨, 현재 스테이지 및 현재 다이얼로그 등)
			StringParser commastringParser = new StringParser(lineParser.ConsumeString(), ',');

			RecordData.progress = new Progress(commastringParser.ConsumeString());
			VolatileData.progress = new Progress();
			VolatileData.progress.Clone(RecordData.progress);

			// 네 번째 줄은 클리어한 스테이지의 개수
			int stageNum = int.Parse(lineParser.ConsumeString());
			for (int i = 0; i < stageNum; i++) {
				// 다섯 번째 줄부터는 각 스테이지 별로 도전기록들(실패한 기록도 포함).
				// 한 줄에 스테이지와 도전 횟수가 나오고, 그 다음 줄부터 한 줄마다 각 도전기록들이 나오는 구조
				commastringParser = new StringParser(lineParser.ConsumeString(), ',');
				StageNum stage = commastringParser.ConsumeEnum<StageNum> ();
				int clearCount = commastringParser.ConsumeInt();
				for (int j = 0; j < clearCount; j++) {
					RecordData.RecordStageClear(stage, new StageClearRecord(lineParser.ConsumeString()));
				}
			}

			// 마지막에서 두 번째 줄은 마지막으로 선택한 스킬들 목록
			commastringParser = new StringParser(lineParser.ConsumeString(), ',');
			int unitNum = commastringParser.ConsumeInt();
			for (int i = 0; i < unitNum; i++) {
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
			foreach (GlossaryType i in Enum.GetValues(typeof(GlossaryType))) {
				GlossaryType type = commastringParser.ConsumeEnum<GlossaryType>();
				int num = commastringParser.ConsumeInt();
				for (int j = 0; j < num; j++) {
					int index = commastringParser.ConsumeInt();
					int level = commastringParser.ConsumeInt();
					RecordData.openedGlossaries[type].Add(index, level);
				}
			}
		}
		public static Difficulty GetDifficultyFromSaveFileString(string[] data){
			foreach(Difficulty diff in Enum.GetValues(typeof(Difficulty))){
				var diffName = diff.ToString();
				foreach(var line in data)
					if (line.StartsWith(diffName)) 
						return diff;
			}
			
			Debug.LogError("Difficulty not found from SaveFile");
			return default(Difficulty);
		}
	}
}
