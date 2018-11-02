using System.Collections.Generic;
using GameData;
using Enums;
using System;
using System.Linq;

namespace Save {
	public class PlayLog {

	}
	[Serializable]
	public class BattleStartPlayLog : PlayLog {
		public StageNum stage;
		public int trial;
		public Difficulty difficulty;
		public List<SelectedSkillEntry> selectedSkills = new List<SelectedSkillEntry>();

		public BattleStartPlayLog(StageNum stage, int trial, Difficulty difficulty, Dictionary<string, List<string>> selectedSkills) {
			this.stage = stage;
			this.trial = trial;
			this.difficulty = difficulty;
			foreach(var kv in selectedSkills) {
				this.selectedSkills.Add(new SelectedSkillEntry(kv.Key, kv.Value));
			}
			this.selectedSkills = this.selectedSkills.OrderBy(entry => entry.PC).ToList();
		}
	}
	[Serializable]
	public class BattleEndPlayLog : PlayLog {
		// 이 스테이지 도전기록에서, 각 트리거에서 얻은 점수들을 담고 있는 딕셔너리
		// 트리거 이름, 점수를 얻은 횟수, 한 번당 얻은 점수 순으로 기록함
		public StageNum stage;
		public Difficulty difficulty;
		public BattleEndType endType;
		public List<AchievedTriggersEntry> achievedTriggers = new List<AchievedTriggersEntry>();
		public int score;
		public int phase;   // 소요된 페이즈(클리어 당시의 페이즈)
		public float spentTime;    // 소요된 시간(초)
		
		public BattleEndPlayLog(StageNum stage, BattleEndType endType, StageClearRecord stageClearRecord) {
			this.stage = stage;
			this.endType = endType;
			this.difficulty = stageClearRecord.difficulty;
			this.score = stageClearRecord.score;
			this.phase = stageClearRecord.phase;
			this.spentTime = stageClearRecord.time;
			foreach (var kv in stageClearRecord.achievedTriggers) {
				this.achievedTriggers.Add(new AchievedTriggersEntry(kv.Key, kv.Value[0], kv.Value[1]));
			}
		}
	}
	[Serializable]
	public class SpentTimePlayLog : PlayLog {
		public string sceneName;
		public float spentTime;

		public SpentTimePlayLog(string sceneName, float spentTime) {
			this.sceneName = sceneName;
			this.spentTime = spentTime;
		}
	}

	[Serializable]
	public class SelectedSkillEntry {
		public string PC;
		public List<string> skills;

		public SelectedSkillEntry(string PC, List<string> skills) {
			this.PC = PC;
			this.skills = skills;
		}
	}
	[Serializable]
	public class AchievedTriggersEntry {
		public string trigger;
		public int achievedTimes;
		public int scorePerOnce;

		public AchievedTriggersEntry(string trigger, int achievedTimes, int scorePerOnce) {
			this.trigger = trigger;
			this.achievedTimes = achievedTimes;
			this.scorePerOnce = scorePerOnce;
		}
	}
	/*
	[System.Serializable]
	public class CharacterPickrate {
		public string name;
		public List<int> pickedStages;
		public List<int> unpickedStages;
		public float score;
		public int pickableStageNum;
		public float pickRate;

		public CharacterPickrate(string name, List<int> pickedStages, List<int> unpickedStages, float score, int pickableStageNum, float pickRate) {
			this.name = name;
			this.pickedStages = pickedStages;
			this.unpickedStages = unpickedStages;
			this.score = score;
			this.pickableStageNum = pickableStageNum;
			this.pickRate = pickRate;
		}
	}*/
}
