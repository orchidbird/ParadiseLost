using UnityEngine;
using System;
using System.Collections.Generic;
using Enums;
using GameData;
using UtilityMethods;

public class BattleTrigger{
	public bool acquired;
	public TrigResultType result;
	public TrigActionType action;
	public TrigUnitType target;
	public TrigUnitType actor;

	public string subType = "";
	public int reward;
    public int rewardAddedCount = 0;    // 이 trigger를 달성함으로써 BattleTrigger.reward가 추가된 횟수
	public int count;
	public int reqCount;
    public string reqCountString;

    public int period;  // Spawn 트리거일 때만 사용
	
	public bool reverse; //일반적인 경우와 반대로, 달성된 상태로 시작해서 조건부로 해제되는 것들. 예) n페이즈 이내 승리
	public bool repeatable;

	public List<string> targetUnitNames = new List<string>();
	public List<string> actorUnitNames = new List<string>();
	public List<Vector2Int> targetTiles = new List<Vector2Int>();
	public string nextSceneName;
	public string conditionalSceneName = "";  //클리어 후 사용할 대사 파일 이름(예 : Scene#N)
	public string korName;
	public string engName;
	public string GetName{get { return Language.Select(korName, engName); }}
	
	//승리&패배 조건이 전부 만족시켜야 하는지/하나만 만족시켜도 되는지에 대한 정보
	public enum TriggerRelation{One, All, Sequence}
	public TriggerRelation winTriggerRelation;
	public TriggerRelation loseTriggerRelation;
    
	public List<Log> logs = new List<Log>(); // 이 trigger를 count시킨 로그들

	public Action TriggerAction = () => {};

	public BattleTrigger(TrigResultType resultType, StringParser parser){
        // BattleTriggerFactory에서 commaParser를 이용해 ResultType은 파싱해놓은 상태
        if (resultType != TrigResultType.Info) {
            reqCountString = parser.ConsumeString();
            try{
                reqCount = int.Parse(reqCountString);
            }catch{
                reqCount = -1;
            }
            reward = parser.ConsumeInt();
            korName = parser.ConsumeString();
	        engName = korName;
	        string next = parser.ConsumeString();
	        try{
		        action = (TrigActionType)Enum.Parse(typeof(TrigActionType), next);
	        }catch{
		        engName = next;
		        action = parser.ConsumeEnum<TrigActionType>();
	        }
	        
            if (action == TrigActionType.Escape) { //action의 번호의 의미에 대해서는 TrigActionType을 정의한 부분에서 각주 참고.
                int numberOfTiles = parser.ConsumeInt();
                for (int i = 0; i < numberOfTiles; i++) {
                    Vector2Int position = parser.ConsumeVector2();
                    targetTiles.Add(position);
                }
            }else if ((int)action >= 18 && (int)action <= 22)
                subType = parser.ConsumeString();

            if ((int)action > 10) {
                actor = parser.ConsumeEnum<TrigUnitType>();
                if (actor == TrigUnitType.Name) {
                    actorUnitNames = GetNameList(parser);
                }

                if ((int)action > 20) {
                    target = parser.ConsumeEnum<TrigUnitType>();
                    if (target == TrigUnitType.Name) {
                        targetUnitNames = GetNameList(parser);
                    }
                }
            }
	        
            switch (resultType){
	            case TrigResultType.Spawn:
		            period = parser.ConsumeInt();
		            
		            var spawnCandidates = new List<string>();
		            spawnCandidates.Add(parser.ConsumeString());
		            if (spawnCandidates[0] == "random")
			            //생성할 유닛 후보들의 codeName List를 넘겨줌.
			            spawnCandidates = VolatileData.stageData.GetUnitGenInfos().FindAll(info => info.Additive).ConvertAll(info => info.CodeName);    
		            
		            var targetTilesList = new List<List<Vector2Int>>();
		            var directionsList = new List<List<Direction>>();
		            int numberOfTiles = parser.ConsumeInt();
		            for (int i = 0; i < numberOfTiles; i++) {
			            var positionCandidates = parser.ConsumeList<Vector2Int>();
			            targetTilesList.Add (positionCandidates);
			            var directionCandidates = parser.ConsumeList<Direction> ();
			            directionsList.Add (directionCandidates);
		            }
		            TriggerAction = () => {			            
			            var targetTiles = new List<Vector2Int>();
			            var directions = new List<Direction>();
			            for(int i = 0; i < targetTilesList.Count; i++){
				            var positionCandidates = targetTilesList[i];
				            int randomIndex = UnityEngine.Random.Range(0, positionCandidates.Count);
				            targetTiles.Add(positionCandidates[randomIndex]);
				            var directionCandidates = directionsList[i];
				            directions.Add(randomIndex >= directionCandidates.Count
					            ? directionCandidates[0]
					            : directionCandidates[randomIndex]);
			            }
			            //codeName List에서 무작위로 하나 생성.
			            //var spawnUnitName = spawnCandidates[UnityEngine.Random.Range(0, spawnCandidates.Count)];
			            //UnitManager.Instance.GenerateUnitsAtPosition(spawnUnitName, targetTiles, directions);
		            };
		            break;
	            case TrigResultType.MoveCamera:
		            List<CameraMoveLog> cameraMoveActions = parser.ConsumeList<CameraMoveLog>();
		            TriggerAction = () => {
			            foreach(var action_ in cameraMoveActions)
				            LogManager.Instance.Record(action_);
		            };
		            break;
	            case TrigResultType.Zoom:
		            List<ZoomLog> zoomActions = parser.ConsumeList<ZoomLog>();
		            TriggerAction = () => {
			            foreach(var action_ in zoomActions)
				            LogManager.Instance.Record(action_);
		            };
		            break;
	            case TrigResultType.Script:
		            TriggerAction = () => {
					LogManager.Instance.Record(new DisplayScriptLog("stage" + (int)VolatileData.progress.stageNumber + "_" + GetName));
		            };
		            break;
            }

            //Debug.Log("index : " + commaParser.index + " / length : " + commaParser.origin.Length);
            while (parser.Remain > 0) {
                string code = parser.ConsumeString();
                if (code == "") break;
                
                if (code == "Repeat") repeatable = true;
                else if(code == "ChangeScript") conditionalSceneName = parser.ConsumeString();
                else if(code == "Reverse"){
                    reverse = true;
                    acquired = true;
                }else Debug.LogError("잘못된 트리거 속성 이름 : index " + parser.index + " / " + code);
            }
        }else{
            nextSceneName = parser.ConsumeString();
            winTriggerRelation = parser.ConsumeEnum<TriggerRelation>();
            loseTriggerRelation = parser.ConsumeEnum<TriggerRelation>();
        }
	}

	List<string> GetNameList(StringParser commaParser){
		int targetCount = commaParser.ConsumeInt();
		List<string> result = new List<string>();
		for (int i = 0; i < targetCount; i++){
			string unitName = commaParser.ConsumeString();
			result.Add(unitName);
		}

		return result;
	}

    public void Trigger() {
		TriggerAction ();
    }

    // Info인 경우에만 의미가 있음
    public string GetRelationText(TrigResultType winOrLose) {
        if(result != TrigResultType.Info) {
            Debug.LogError("'GetRelationText' 메서드는 Info trigger에 대해서만 불려야 함");
            return "";
        }
	    var relation = winOrLose == TrigResultType.Win ? winTriggerRelation : loseTriggerRelation;
        
        if (relation == TriggerRelation.All)
            return Language.Select("\t다음 조건을 모두 충족 :", "\tComplete all :");
        if (relation == TriggerRelation.Sequence)
            return Language.Select("\t다음 조건을 순서대로 충족 :", "\tComplete in order :");
        return Language.Select("\t다음 중 하나를 충족 :", "\tComplete one :");
    }
}

class BattleTriggerFactory {
    public static BattleTrigger Get(string data) {
        StringParser commaParser = new StringParser(data, '\t');
        var resultType = commaParser.ConsumeEnum<TrigResultType>();
	    var trigger = new BattleTrigger(resultType, commaParser) {result = resultType};
	    return trigger;
    }
}
