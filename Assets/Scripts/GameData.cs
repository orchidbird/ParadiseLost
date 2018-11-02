using Enums;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Camerawork;
using UtilityMethods;

namespace GameData{
	// 세션별로 달라져야 하는 데이터 - 즉, 로드와 세이브, 리셋을 할 때 달라져야 하는 데이터는 RecordData쪽에(세션의 진행상황, 마지막으로 선택한 기술들 등), 
	// 달라지지 않는 데이터 - 즉, 로드와 세이브, 리셋을 해도 달라지지 않을 데이터는 VolatileData 쪽에(언어 설정, 캐싱 정보 등).
	public class RecordData{
        public static Progress progress = new Progress();
		public static float totalPlayingTime = 0;
		public static int level{get{return (int)VolatileData.progress.stageNumber / 10;}} 
        public static Dictionary<StageNum, List<StageClearRecord>> stageClearRecords = new Dictionary<StageNum, List<StageClearRecord>>();
		public static List<StageNum> tokens = new List<StageNum>();  // 증표(가칭)를 얻은 스테이지들의 list
        public static Dictionary<string, List<string>> customSkillTrees = new Dictionary<string, List<string>>(); // 유닛 별로, 마지막으로 선택한 기술들을 저장
        public static Dictionary<GlossaryType, Dictionary<int, int>> openedGlossaries = new Dictionary<GlossaryType, Dictionary<int, int>>
        {
            {GlossaryType.Person, new Dictionary<int, int>() },
            {GlossaryType.Group, new Dictionary<int, int>() },
            {GlossaryType.Location, new Dictionary<int, int>() },
            {GlossaryType.Etc, new Dictionary<int, int>()}
        };

		static List<string> unlockedCharacters;
        static bool newRecordsAdded;

		public static List<string> alreadyReadDialogues = new List<string>();
		public static List<string> alreadyReadTutorials = new List<string>();
		public static List<string> recentPicks = new List<string>(); //마지막으로 고른 유닛을 저장했다가 Ready씬에 들어갈 때 자동으로 골라줌.

		// 스팀 업적 진행상황을 저장하기 위한 데이터
		public static int neutralizeEnemyCount; // 적을 이탈시킨 횟수
		public static int critCount; // 치명타를 터뜨린 횟수 

		public static void Reset(){
            progress.Reset();
			totalPlayingTime = 0;
			neutralizeEnemyCount = 0;
			critCount = 0;
			alreadyReadTutorials.Clear();
			stageClearRecords = new Dictionary<StageNum, List<StageClearRecord>>();
			alreadyReadDialogues.Clear();
			tokens = new List<StageNum>();
            customSkillTrees = new Dictionary<string, List<string>>();
            openedGlossaries = new Dictionary<GlossaryType, Dictionary<int, int>>{
                {GlossaryType.Person, new Dictionary<int, int>() },
                {GlossaryType.Group, new Dictionary<int, int>() },
                {GlossaryType.Location, new Dictionary<int, int>() },
                {GlossaryType.Etc, new Dictionary<int, int>() }
            };
        }
        
		public static void RecordStageClear(StageNum stage, StageClearRecord record) {
            newRecordsAdded = true;
            if(!stageClearRecords.ContainsKey(stage))
                stageClearRecords.Add(stage, new List<StageClearRecord> { record });
            else stageClearRecords[stage].Add(record);

            if(record.star >=2 && !tokens.Contains(stage))
                tokens.Add(stage);
        }
        public static void AddSkillTree(string unitName, string skillName){
            if(!customSkillTrees.ContainsKey(unitName))
                customSkillTrees.Add(unitName, new List<string> { skillName });
            else if(!customSkillTrees[unitName].Contains(skillName)) 
                customSkillTrees[unitName].Add(skillName);
        }

		public static void AddSkillTree(string unitName, List<Skill> skills){
			customSkillTrees.Remove(unitName);
			customSkillTrees.Add(unitName, new List<string>());
			foreach (var skill in skills)
				customSkillTrees[unitName].Add(skill.korName);
		}

        public static List<string> GetUnlockedCharacters() {
            // 사용한 적이 있는 캐릭터들을 리턴
            if (newRecordsAdded || unlockedCharacters == null) {
                unlockedCharacters = new List<string>();
	            foreach (var record in stageClearRecords){
		            foreach (var unit in StageData.CandidatesOfStage(record.Key)){
			            if(!unlockedCharacters.Contains(unit) && unit != "tien")
				            unlockedCharacters.Add(unit);
		            }
	            }
            }
            newRecordsAdded = false;
            return unlockedCharacters;
        }

        // stage에서 한 번이라도 클리어된 적 있는 battleTrigger를 리턴함. 참조를 쉽게 하기 위해 딕셔너리로 만듬.
		public static Dictionary<string, bool> GetTriggersAchievedAtLeastOnce(StageNum stage) {
            Dictionary<string, bool> achievedTriggers = new Dictionary<string, bool>();
            List<StageClearRecord> records = stageClearRecords[stage];
            foreach(var record in records)
                foreach(var trigger in record.achievedTriggers.Keys) 
                    if (!achievedTriggers.ContainsKey(trigger))
                        achievedTriggers.Add(trigger, true);
            return achievedTriggers;
        }
    }

    public class StageClearRecord {
		public BattleEndType endType;
        // 이 스테이지 도전기록에서, 유닛별로 선택한 스킬들의 목록을 담고 있는 딕셔너리
        public Dictionary<string, List<string>> skillTrees = new Dictionary<string, List<string>>();
        // 이 스테이지 도전기록에서, 각 트리거에서 얻은 점수들을 담고 있는 딕셔너리
        // 트리거 이름, 점수를 얻은 횟수, 한 번당 얻은 점수 순으로 기록함 
        public Dictionary<string, List<int>> achievedTriggers = new Dictionary<string, List<int>>();
		public Difficulty difficulty;
        public int score;
        public int star;
        public int phase;   // 소요된 페이즈(클리어 당시의 페이즈)
        public float time;    // 소요된 시간(초)
        public StageClearRecord(Dictionary<string, List<string>> skillTrees, Dictionary<string, List<int>> achievedTriggers, 
				Difficulty difficulty, int score, int star, int phase, float time) {
            this.skillTrees = skillTrees;
            this.achievedTriggers = achievedTriggers;
			this.difficulty = difficulty;
            this.score = score;
            this.star = star;
            this.phase = phase;
            this.time = time;
        }
        public StageClearRecord(string str) {   // 세이브파일에 기록된 문자열을 스테이지 도전기록으로 파싱
            StringParser commastringParser = new StringParser(str, ',');
            int unitNum = commastringParser.ConsumeInt();
            for(int i = 0; i < unitNum; i++) {
                string unitName = commastringParser.ConsumeString();
                int skillNum = commastringParser.ConsumeInt();
                List<string> skillTree = new List<string>();
                for (int j = 0; j < skillNum; j++)
                    skillTree.Add(commastringParser.ConsumeString());
                skillTrees.Add(unitName, skillTree);
            }
            int achievedTriggerNum = commastringParser.ConsumeInt();
            for(int i = 0; i < achievedTriggerNum; i++) {
                achievedTriggers.Add(commastringParser.ConsumeString(), 
                        new List<int> { commastringParser.ConsumeInt(), commastringParser.ConsumeInt() });
            }
			difficulty = commastringParser.ConsumeEnum<Difficulty>();
            score = commastringParser.ConsumeInt();
            star = commastringParser.ConsumeInt();
            phase = commastringParser.ConsumeInt();
            time = commastringParser.ConsumeFloat();
        }
        public string ToSaveString() {  // 세이브파일에 기록될 문자열
            string text = skillTrees.Count + ",";
            foreach (var kv in skillTrees) {
                text += kv.Key + "," + kv.Value.Count + ",";
                foreach (var skillName in kv.Value)
                    text += skillName + ",";
            }
            text += achievedTriggers.Count + ",";
            foreach(var kv in achievedTriggers)
                text += kv.Key + "," + kv.Value[0] + "," + kv.Value[1] + ",";
            text += difficulty + "," + score + "," + star + "," + phase + "," + time;
            return text;
        }
        public string ToLogString() {   // 로그에 표시될 문자열
            string charactersString = "";
            foreach (var kv in skillTrees)
                charactersString += kv.Key + ",";
            return "출전:" + charactersString + "난이도: " + difficulty + " 점수:" + score + " 별:" + star +
                    " 페이즈:" + phase + " 시간:" + time;
        }

        public bool IsBetterThan(StageClearRecord record) {
            if(score > record.score)    return true;        // 점수에 대해 비교
            if(score < record.score)    return false;
            if(phase < record.phase)    return true;        // 페이즈 수에 대해 비교
            if(phase > record.phase)    return false;
            if(time < record.time)      return true;        // 시간에 대해 비교
            if(time > record.time)      return false;
            return false;
        }
    }

    public class VolatileData {
        public static Progress progress = new Progress();
        public static GameMode gameMode;
	    public static Difficulty difficulty;
	    public static Lang language;

	    public static void SetDifficulty(int input){
		    difficulty = (Difficulty) input;
	    }
		public static bool OpenCheck(StageNum openStageNum){
			return progress.stageNumber >= openStageNum;
		}
        public static int currentSaveSlotIndex = 0;
        public static string saveSlotName;

        public static StageData stageData = new StageData(); // 현재 스테이지(progress.currentStage)에 대한 정보들
        
        public static void Reset() {
            progress.Reset();
            saveSlotName = progress.dialogueName;
        }

        // 리소스 로딩 시간을 최소화하기 위해 캐싱해놓는 범용 에셋 및 데이터들
        static Material grayScale;
	    static Material glowBorder;
        static Dictionary<string, Sprite> portraitsDict = new Dictionary<string, Sprite>();
        static Dictionary<string, Sprite> standingImageDict = new Dictionary<string, Sprite>();
        static Dictionary<string, Sprite[]> unitImageDict = new Dictionary<string, Sprite[]>();
        static Dictionary<string, Sprite> tileImageDict = new Dictionary<string, Sprite>();
        static Dictionary<UnitClass, Sprite> classImageDict = new Dictionary<UnitClass, Sprite>();
        static Dictionary<Element, Sprite> elementImageDict = new Dictionary<Element, Sprite>();
		static Dictionary<Stat, Sprite> statImageDict = new Dictionary<Stat, Sprite>();
        static Dictionary<int, Sprite> stageBackgroundDict = new Dictionary<int, Sprite>();
        static Dictionary<string, GameObject> visualEffectPrefabDict = new Dictionary<string, GameObject>();
        static Dictionary<IconSprites, Sprite> iconSpriteDict = new Dictionary<IconSprites, Sprite>();
        static Dictionary<string, Font> fontDict = new Dictionary<string, Font>();

        public static Material GetGrayScale() {
            if(grayScale == null)
                grayScale = Resources.Load<Material>("Shader/grayscale");
            return grayScale;
        }

	    public static Material GetGlowBorder(){
		    if(glowBorder == null)
			    glowBorder = Resources.Load<Material>("Shader/GlowBorder");
		    return glowBorder;
	    }

        public static Sprite[] GetUnitSprite(string name, bool isAlly){
	        name = _String.GeneralName(name);
            if(!unitImageDict.ContainsKey(name)) {
                Sprite[] sprites = Resources.LoadAll<Sprite>("UnitImage/" + name);
                if (sprites.Length == 0)
	                sprites = Resources.LoadAll<Sprite>(isAlly ? "UnitImage/notFound" : "UnitImage/notFound_enemy");
                if(unitImageDict.Count >= 100) { unitImageDict = new Dictionary<string, Sprite[]>();}
                unitImageDict.Add(name, sprites);
            }
            return unitImageDict[name];
        }

        public static Sprite GetSpriteOf(SpriteType spType, string name){
            Dictionary<string, Sprite> dict = null;
	        if (spType == SpriteType.Illust){
		        dict = standingImageDict;
		        name = _String.GeneralName(name);
	        }else if (spType == SpriteType.Portrait){
		        dict = portraitsDict;
		        name = _String.GeneralName(name);
	        }else if(spType == SpriteType.TileImage) {dict = tileImageDict;}

	        if (dict.ContainsKey(name)) return dict[name];

	        var result = Resources.Load<Sprite>(spType + "/" + name) ?? Resources.Load<Sprite>(spType + "/notfound");
	        if(dict.Count >= 100) {dict = new Dictionary<string, Sprite>();}
	        dict.Add(name, result);
	        return dict[name];
        }

        //Dict Key가 string으로 되어 있는 아이콘을 호출(=달리 Enum화가 되지 않은 것들)
        public static Sprite GetIcon(IconSprites input){
            if(!iconSpriteDict.ContainsKey(input))
                iconSpriteDict[input] = Resources.Load<Sprite>("Icon/" + input);
            return iconSpriteDict[input];
        }
        //Dict Key가 해당 클래스의 enum으로 되어 있는 아이콘을 호출.
        public static Sprite GetIcon<T>(T enumInput){
            if(typeof(T) == typeof(UnitClass)){
                return GetIconOf(classImageDict, (UnitClass)(object)enumInput);
            }
	        if(typeof(T) == typeof(Element)){
		        return GetIconOf(elementImageDict, (Element)(object)enumInput);
	        }
	        if(typeof(T) == typeof(Stat)) {
		        return GetIconOf(statImageDict, (Stat)(object)enumInput);
	        }
	        return GetIcon(IconSprites.Transparent);
        }
        //위의 GetIcon<T>에서 클래스에 따라 다른 Input으로 호출됨.
        //사실상 GetIcon<T>와 한쌍인데 Generic Key는 인자로만 사용 가능해서 두 개의 메소드로 분리.
        static Sprite GetIconOf<T>(Dictionary<T, Sprite> dict, T enumInput){
	        if (dict.ContainsKey(enumInput)) return dict[enumInput];
	        
	        var result = typeof(T) != typeof(Stat) ? Resources.Load<Sprite>("Icon/" + enumInput) : Resources.Load<Sprite>("Icon/Stat/" + enumInput);
	        if(result == null) {return Resources.Load<Sprite>("Icon/Transparent");}
	        if(dict.Count >= 100) {dict.Clear();}
	        dict.Add(enumInput, result);
	        return dict[enumInput];
        }

        public static Font GetFont(string name) {
            if(!fontDict.ContainsKey(name)) {
                if (fontDict.Count >= 100) { fontDict.Clear(); }
                fontDict.Add(name, Resources.Load<Font>("Fonts/" + name));
            }
            return fontDict[name];
        }
		
	    static string stageInfo;
        static string GetStageInfo(){
	        return stageInfo ?? (stageInfo = Resources.Load<TextAsset>("Data/StageInfo").text);
        }

		public static int GetStageDate(StageNum stage){
		    Debug.Log("Get Date of stage " + stage);
			return int.Parse(Parser.FindRowDataOf(GetStageInfo(), ((int)stage).ToString())[1]);
	    }

		public static string GetStageData(StageNum stage, StageInfoType infoType){
		    var column = (int) infoType + (infoType != StageInfoType.POV && language == Lang.Eng ? 1 : 0);
			return Parser.FindRowDataOf(GetStageInfo(), ((int)stage).ToString())[column];
	    }

		public static Sprite GetStageBackground(StageNum stage) {
			if (!stageBackgroundDict.ContainsKey((int)stage)) {
				string bgImageName = Parser.FindRowDataOf(GetStageInfo(), ((int)stage).ToString())[3];
				stageBackgroundDict.Add((int)stage, Resources.Load<Sprite>("Background/" + bgImageName));
			}
			return stageBackgroundDict[(int)stage];
		}

        public static GameObject GetVisualEffectPrefab(string visualEffectName){
	        if (visualEffectName == "-") return null;
	        if (visualEffectPrefabDict.ContainsKey(visualEffectName)) return visualEffectPrefabDict[visualEffectName];
	        
	        var prefab = Resources.Load("VisualEffect/Prefab/" + visualEffectName) as GameObject;
	        if (prefab == null)
		        Debug.Log("Cannot load particle " + visualEffectName);
	        if (visualEffectPrefabDict.Count >= 100)
		        visualEffectPrefabDict = new Dictionary<string, GameObject>(); // 잘못된 파라미터 호출들로 인한 메모리 낭비 방지
	        visualEffectPrefabDict.Add(visualEffectName, prefab);
	        return visualEffectPrefabDict[visualEffectName];
        }

	    static Dictionary<string, List<Skill>> unitAllSkillDict;
	    public static List<Skill> GetAllSkillsByUnit(string unitName){
		    if (unitAllSkillDict == null)
			    unitAllSkillDict = new Dictionary<string, List<Skill>>();
		    if (!unitAllSkillDict.ContainsKey(unitName))
			    unitAllSkillDict.Add(unitName, TableData.AllSkills.FindAll(skill => _String.Match(skill.owner, unitName)));
		    return unitAllSkillDict[unitName];
	    }
	    
	    public static List<Skill> SkillsOf(string ownerName, bool isPC, bool openedOnly = true){
		    var chapter = RecordData.level;
		    var result = GetAllSkillsByUnit(ownerName).FindAll(skill => (!isPC || skill.address[0] != 'L')
		                                                                && (skill.address[0] != 'T' || !isPC && skill.RequireLevel == chapter));
		    if (openedOnly)
			    result = result.FindAll(skill => skill.RequireLevel <= chapter);
		    return result;
	    }
    }

	public static class TableData{
		static List<Skill> allSkills;
		public static List<Skill> AllSkills{get{
			if (allSkills != null) return allSkills;
			var actives = Parser.GetParsedData<ActiveSkill>().Cast<Skill>();
			var passives = Parser.GetParsedData<PassiveSkill>().Cast<Skill>();
			return allSkills = actives.Union(passives).ToList();
		}}
	}
	public class StageInfo{
		public static List<StageInfo> stages;
		public static List<StageInfo> Stages{get { return stages ?? (stages = Parser.GetParsedData<StageInfo>()); }}

		public StageNum stage;
		public int date;
		public string POV;
		private string background;
		public string korTitle;
		public string engTitle;
		public string Title{
			get { return Language.Select(korTitle, engTitle); }
		}
		public string korIntro;
		public string engIntro;
		public string Intro{
			get { return Language.Select(korIntro, engIntro); }
		}
		
		public StageInfo(string line){
			var parser = new StringParser(line, '\t');
			stage = (StageNum)(parser.ConsumeInt ());
			date = parser.ConsumeInt();
			POV = parser.ConsumeString();
			background = parser.ConsumeString();
			korTitle = parser.ConsumeString();
			engTitle = parser.ConsumeString();
			korIntro = parser.ConsumeString();
			engIntro = parser.ConsumeString();
		}
	}

    public class StageData {
	    private StageNum loadedStage;
	    private List<BattleTrigger> _battleTriggers;
	    private List<UnitInfo> _unitInfos;
	    private List<TileInfo> _tileInfos;
	    private List<AIInfo> _aiInfos;
	    private List<UnitGenInfo> _genInfos;
		private List<CameraWork> _cameraWorkInfos;
        
        public void Load(bool forced = false){
	        if(!forced && loadedStage == VolatileData.progress.stageNumber) return;
			
	        //BattleTrigger.Spawn은 _genInfo를 참조하므로, 반드시 후자가 전자보다 먼저 읽혀야 한다!
		    loadedStage = VolatileData.progress.stageNumber;
	        _genInfos = Parser.GetParsedData<UnitGenInfo>();
            _battleTriggers = Parser.GetParsedData<BattleTrigger>();
            _unitInfos = Parser.GetParsedData<UnitInfo>();
            _tileInfos = Parser.GetParsedTileInfo();
            _aiInfos = Parser.GetParsedData<AIInfo>();
			_cameraWorkInfos = Parser.GetParsedData<CameraWork>();
        }

        public List<BattleTrigger> GetBattleTriggers() {
            Load();
            return _battleTriggers;
        }
        public List<UnitInfo> GetUnitInfos() {
            Load();
            return _unitInfos;
        }
        public List<TileInfo> GetTileInfos() {
            Load();
            return _tileInfos;
        }
        public List<AIInfo> GetAIInfos() {
            Load();
            return _aiInfos;
        }
	    public List<UnitGenInfo> GetUnitGenInfos(){
		    Load();
		    return _genInfos;
	    }
		public List<CameraWork> GetCameraWorkInfos() {
			Load();
			return _cameraWorkInfos;
		}
	    public bool IsTwoSideStage(){
		    Load();
		    return loadedStage == Setting.pintosVSHaskellStage;
	    }
	    public bool IsAgiligyChangingStage{get{
		    Load();
		    return Setting.agilityChangingStage.Contains(loadedStage);
	    }}

	    public static List<string> CandidatesOfStage(string[] stageRow){
		    var result = new List<string>();
		    if (stageRow != null)
			    for (int i = 0; i < int.Parse (stageRow [2]); i++)
				    result.Add (stageRow [i + 4]);
		    
		    return result;
	    }
		public static List<string> CandidatesOfStage(StageNum stage){
			return CandidatesOfStage(Parser.FindRowOf(ReadyManager.Instance.StageAvailablePCTable, ((int) stage).ToString())); 
	    }
    }

    public class Progress {
        public bool isDialogue;
        public string dialogueName;
		public StageNum stageNumber;

        public Progress() {
            Reset();
        }

        public Progress(string str){
	        Debug.Log("new Progress with: " + str);
            if (str.Substring(0, 6) == "Stage ") {
                isDialogue = false;
				stageNumber = (StageNum)(int.Parse (str.Substring (6)));
            }else{
                isDialogue = true;
                dialogueName = str;
            }
        }

        public void Clone(Progress progress) {
            isDialogue = progress.isDialogue;
            dialogueName = progress.dialogueName;
            stageNumber = progress.stageNumber;
        }

        public void Reset() {
            isDialogue = true;
            dialogueName = "Scene#1-1";
            stageNumber = StageNum.S1_1;
        }

        public string ToSaveString()
        {
	        if(isDialogue)  return dialogueName;
			return "Stage " + (int)stageNumber;
        }
    }

	public class GlobalData{
		static List<GlossaryData> glossaryDataList = new List<GlossaryData>();
        public static List<GlossaryData> GlossaryDataList{
            get{
                if(glossaryDataList.Count == 0) {glossaryDataList = Parser.GetParsedGlossaryData();}
                return glossaryDataList;
            }
        }

		public static void SetGlossaryDataList(){
            foreach(var glos in GlossaryDataList){
                if(RecordData.openedGlossaries[glos.Type].ContainsKey(glos.index)){
                    glos.level = RecordData.openedGlossaries[glos.Type][glos.index];
                }
            }
		}
	}
}
