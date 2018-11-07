using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Battle.Skills;
using System.Linq;
using GameData;
using Battle.Damage;
using UtilityMethods;

public class UnitManager : MonoBehaviour{
	private static UnitManager instance;
	public static UnitManager Instance{ get { return instance; } }
    public GameObject playButton;

	void Awake(){
		if (instance != null && instance != this){
			Destroy (gameObject);
		}
		else instance = this;
	}

    public GameObject unitPrefab;
	List<Unit> allUnits = new List<Unit>();
	public List<Unit> unitsActThisPhase = new List<Unit>();
    public List<Unit> unitsActNextPhase = new List<Unit>();

    List<Collectable> Collectables = new List<Collectable>();
    public Dictionary<string, List<string>> PCSelectedSkillList = new Dictionary<string, List<string>>();
    int generatedPC = 0;
    List<UnitGenInfo> genInfos = new List<UnitGenInfo>();
    
	public static List<Unit> GetAllUnits() {return Instance.allUnits;}

	public Unit GetAnUnit(string engName){
		Unit wantedUnit = null;
		foreach(Unit unit in GetAllUnits()){
			if (unit.CodeName == engName) {
				wantedUnit = unit;
			}
		}
		return wantedUnit;
	}

	public void ResetLatelyHitUnits(){
		foreach (var unit in GetAllUnits()){
			unit.GetLatelyHitInfos().Clear();
		}
	}

    public void UpdateBasicStatusEffects(){
        foreach(var unit in GetAllUnits())
	        if(!unit.IsObject)
	        	unit.UpdateFearDebuff();
    }

    public void CheckCollectableObjects() {
        foreach(var name in Collectables){
            int distance = Calculate.Distance(BattleData.turnUnit.Pos, name.unit.Pos);
	        if (distance > name.range)
		        continue;
	        BattleUIManager.Instance.AddCollectableActionButton();
	        BattleData.NearestCollectable = name;
	        return;
        }
        BattleData.NearestCollectable = null;
    }

    public void TriggerPassiveSkillsAtActionEnd(){
		foreach(var unit in GetAllUnits()) {
            unit.GetListPassiveSkillLogic().TriggerOnActionEnd(unit);
        }
    }

    public void TriggerPassiveSkillsAfterCast(CastLog castLog) {
        castLog.casting.Caster.GetListPassiveSkillLogic().TriggerAfterCast(castLog);
    }

    public void TriggerStatusEffectsAtActionEnd(){
        foreach(var unit in GetAllUnits()) {
            List<UnitStatusEffect> statusEffectList = unit.statusEffectList;
            foreach (UnitStatusEffect statusEffect in statusEffectList) {
                Skill skill = statusEffect.GetOriginSkill();
                if (skill is ActiveSkill)
                    ((ActiveSkill)skill).SkillLogic.TriggerStatusEffectAtActionEnd(unit, statusEffect);
                else if(skill is PassiveSkill)
                    ((PassiveSkill)skill).SkillLogic.TriggerStatusEffectAtActionEnd(unit, statusEffect);
            }
        }
    }

    public void UpdateStatusEffectsAtActionEnd() {
		Aura.UpdateAllAuraUnitEffects (allUnits);
        
        foreach (var unit in GetAllUnits())
            foreach(UnitStatusEffect statusEffect in unit.statusEffectList)
                if(statusEffect.Stack > 0){
                    for (int i = 0; i < statusEffect.actuals.Count; i++)
                        statusEffect.CalculateAmount(i, true);
                    unit.UpdateTransparency();
                }else
					unit.RemoveStatusEffect(statusEffect);

		// 특수 상태이상(기절, 속박, 침묵)이 갱신되었는지 체크
	    GetAllUnits().ForEach(unit => unit.UpdateCCIcon());
    }

    public void UpdateStatsAtActionEnd(){
        foreach(var unit in GetAllUnits())
            unit.UpdateStats();
    }
    public void UpdateHealthViewersAtActionEnd(){
	    foreach (var unit in GetAllUnits())
			unit.UpdateHealthViewer();
    }
    public void UpdateRealUnitPositions(){
        foreach (var unit in GetAllUnits()) {
            unit.UpdateRealPosition();
        }
    }

	public void ShowAllChains(){
		foreach (var unit in GetAllUnits().FindAll(item => item.GetSide() == Side.Ally))
			ChainList.ShowChainByThisUnit(unit);
	}
	public void HideAllChains(){
		foreach (var unit in GetAllUnits()) {
			ChainList.HideChainTilesDisplay ();
			ChainList.ShowUnitsTargetingThisTile (unit.TileUnderUnit, false);
		}
	}

    public void UpdateFogOfWar(){
		var originalFogTypesDict = new Dictionary<Tile, FogType>();
        foreach (var unit in GetAllUnits())
	        unit.RemoveFogsInSight();
		//위에서 안개 정보가 바뀐 후에 투명도를 맞춰야 하므로, 같은 GetAllUnits() 기반이더라도 합치지 말 것!
        foreach(var unit in GetAllUnits())
	        unit.UpdateTransparency();
		foreach(var kv in originalFogTypesDict) {
			if ((int) kv.Key.fogType >= (int) kv.Value) continue;
			
			Log lastLog = LogManager.Instance.GetLastEventLog();
			if (!(lastLog is MoveLog)) continue;
			((MoveLog)lastLog).sightChanged = true;
			break;
		}
    }

    public void DeactivateAllDamageTextObjects(){
        foreach(var unit in allUnits)
			unit.DeactivateDamageTextObject();
    }
    
    public void CheckDestroyedUnits(Casting casting = null){
        LogManager logManager = LogManager.Instance;
        List<Unit> allUnits_Clone = new List<Unit>();
        foreach(var unit in GetAllUnits())
            allUnits_Clone.Add(unit);

        foreach(var unit in allUnits_Clone) {
            var type = unit.GetDestroyReason(casting: casting);
	        if (type == null) continue;
	        logManager.Record(new UnitDestroyLog(unit, (TrigActionType)type));
	        TriggerOnUnitDestroy(unit, (TrigActionType)type);
        }

		foreach(var unit in allUnits_Clone)
			if (unit.GetDestroyReason(casting: casting) == null)
				SkillLogicFactory.Get(unit.GetPassiveSkillList()).TriggerAfterUnitsDestroyed (unit);
    }

    public void TriggerOnUnitDestroy(Unit destroyedUnit, TrigActionType actionType) {
		destroyedUnit.BreakChain ();
		foreach (var se in destroyedUnit.statusEffectList)
			if (se.IsAura ()) 
				Aura.TriggerOnAuraRemoved (destroyedUnit, se);

	    var killedUnit = new List<Unit>();
		foreach (var hitInfo in destroyedUnit.GetLatelyHitInfos()){
			Unit destroyer = hitInfo.caster;
			//destroyedUnit.GetListPassiveSkillLogic ().TriggerOnDestroyed (destroyedUnit, destroyer);
			List<PassiveSkill> destroyerPassiveSkills = destroyer.GetPassiveSkillList();
			if(actionType == TrigActionType.Kill){
                SkillLogicFactory.Get(destroyerPassiveSkills).TriggerOnNeutralizeByMyHand(hitInfo, destroyedUnit, TrigActionType.Kill);
				if (hitInfo.skill != null)
					SkillLogicFactory.Get(hitInfo.skill).OnKill(hitInfo);
				
				if (!killedUnit.Contains(destroyer)){
					killedUnit.Add(destroyer);
					destroyer.ChangeWill (WillChangeType.Shocked, BigSmall.None);
				}
			}else if(actionType == TrigActionType.Retreat)
                SkillLogicFactory.Get(destroyerPassiveSkills).TriggerOnNeutralizeByMyHand(hitInfo, destroyedUnit, TrigActionType.Retreat);

			if (actionType == TrigActionType.Escape) continue;
			SkillLogicFactory.Get (destroyerPassiveSkills).TriggerOnNeutralizeByMyHand (hitInfo, destroyedUnit, TrigActionType.Neutralize);
			if (destroyedUnit.IsEnemyTo (destroyer) && Utility.UnitsInRange (Utility.TilesInDiamondRange (destroyedUnit.Pos, 1, 2, 0)).Any (unit => unit != destroyer && unit.IsAllyTo (destroyer))) {
				destroyer.ChangeWill (WillChangeType.Guardian, BigSmall.None);
			}
		}

        foreach (var unit in allUnits) {
			UnitStatusEffect taunt = unit.statusEffectList.Find (se => se.GetCaster () == destroyedUnit && se.IsTypeOf (StatusEffectType.Taunt));
			if (taunt != null)
				unit.RemoveStatusEffect (taunt);
	        
            foreach (var passive in unit.GetPassiveSkillList())
                passive.SkillLogic.TriggerOnUnitDestroy(unit, destroyedUnit, actionType);

	        if (actionType == TrigActionType.Escape) continue;
	        
	        if (destroyedUnit.IsEnemyTo (unit)){
		        unit.ChangeWill(WillChangeType.Cheer, BigSmall.Small);
		        if(unit == BattleData.turnUnit)
			        unit.ChangeWill(WillChangeType.DirectNeutralize, BigSmall.None);
	        } else if (!destroyedUnit.IsObject && destroyedUnit.IsAllyTo(unit)){
		        if (destroyedUnit.IsPC) {
			        unit.ChangeWill (WillChangeType.Disturbed, BigSmall.Big);
			        unit.ChangeWill (WillChangeType.Angry, BigSmall.None);
		        }else 
			        unit.ChangeWill (WillChangeType.Disturbed, BigSmall.Small);
	        }
        }

		foreach (var kv in TileManager.Instance.GetAllTiles()) {
			Tile tile = kv.Value;
			TileStatusEffect zoc = tile.StatusEffectList.Find (se => se.GetCaster () == destroyedUnit && se.IsTypeOf (StatusEffectType.ZOC));
			if (zoc != null)
				tile.RemoveStatusEffect (zoc);
		}
    }
    public void DeleteDestroyedUnit(Unit target){
	    foreach (var tile in target.TilesUnderUnit)
		    tile.SetUnitOnTile(null);
	    
	    GetAllUnits().Remove(target);
        unitsActThisPhase.Remove(target);
        unitsActNextPhase.Remove(target);
	    Collectables.Remove(Collectables.Find(item => item.unit == target));
	    
	    if (target == BattleData.turnUnit){
		    BattleData.currentState = CurrentState.Destroyed;
		    BattleManager.Instance.unitDestroyedDuringOwnTurn = target;
	    }
	    
	    foreach (var guard in GetAllUnits().FindAll(item => item.CodeName == target.connectedName))
		    LogManager.Instance.Record(new UnitDestroyLog(guard, TrigActionType.Retreat));
	    foreach (var holdee in GetAllUnits().FindAll(item => item.CodeName == target.holdName)){
			holdee.GetAI()._AIData.SetActive(true);
		    holdee.RemoveStatusEffect(holdee.statusEffectList.Find(se => se.GetOriginSkillName() == "붙잡힘"));
	    }
    }

	public void ActivateGuard(List<Tile> tiles){
		foreach (var tile in tiles) {
			if (tile == null || !BattleData.guardDict.ContainsKey(tile))
				continue;
			foreach (var unit in BattleData.guardDict[tile])
				if (unit != null && unit.IsAI && !unit.GetComponent<AIData>().isActive && unit.GetComponent<AIData>().info.acute) 
					unit.GetComponent<AIData>().SetActive(true);
		}
	}

	void ApplyAIInfo(Unit unit){
		var aiData = unit.GetComponent<AIData>();
		aiData.isActive = true; //게임 시작 전이므로 Log를 통해 활성화할 수 없음!
		
		unit.GetActiveSkillList().RemoveAll (skill => skill.korName == "언령" || skill.korName == "광휘" || skill.korName == "집중" || skill.korName == "암살 표식" || skill.korName == "셔플 불릿");
		unit.AddAI();
	}

	public void CheckVigilAreaForAI(){
		foreach (var unit in GetAllUnits().FindAll(unit => unit.IsAI && !unit.GetAI().IsActive))
			unit.GetAI().CheckVigilArea();
	}

	public void GenerateUnitAutomatically(){
		var infoListToSpawn = new List<UnitInfo>();
		do{
			for (int i = 0; i < 3; i++){
				UnitInfo info;
				do
					info = Generic.PickRandom(RecordData.units); while (infoListToSpawn.Contains(info));
				infoListToSpawn.Add(info);
			}
		} while (infoListToSpawn.Count(info => info.HasOffensiveSkill) <= 2);

		GenerateUnit(new UnitInfo(false));
		foreach (var unitInfo in infoListToSpawn)
			GenerateUnit(unitInfo);
        UpdateFogOfWar();
    }

	void GenerateUnit(UnitInfo info){
        var unit = Instantiate(unitPrefab).GetComponent<Unit>();
		unit.ApplyInfo(info);
        unit.transform.SetParent(transform);
		unit.LoadSprites();
        unit.SetDirection(Generic.PickRandom(EnumUtil.directions));

		//유닛이 생성되자마자 fogOfWar 아래에 있으면 숨겨야 하는데,
		//pivot 세팅은 다음 프레임에 이루어지므로 미리 하지 않으면 숨겨지지 않는다.
		//빈 타일 중 무작위로 골라서 유닛을 생성
		unit.SetInitialLocation();
	
        //AIInfo가 allUnits를 참조하고 unitsActThisPhase.Add 여부가 AI를 참조하므로, 아래 문단 내 순서를 함부로 바꾸지 말 것!
        allUnits.Add(unit);
		if (!info.isAlly){
			unit.side = Side.Enemy;
			ApplyAIInfo(unit);
		}

        if(unit.HasAction) unitsActThisPhase.Add(unit);
		unit.healthViewer.SetInitHealth(unit.actualStats[Stat.MaxHealth], unit);

        //평균 위치 계산하고, 소속 타일 중 가장 앞에 있는 것보다 position.z를 0.05f 당김
        var tiles = unit.TilesUnderUnit;
        float zValue = tiles[0].transform.position.z;
        tiles.ForEach(tile => {
            tile.SetUnitOnTile(unit);
            if(tile.transform.position.z < zValue) {zValue = tile.transform.position.z;}
        });
        var averagePos = Utility.AveragePos(tiles);
		unit.transform.position = new Vector3(averagePos.x, averagePos.y, zValue - 0.05f);
    }

    /*public void GenerateUnitsAtPosition(string codeName, List<Vector2Int> positions, List<Direction> directions){
        //codeName인 유닛을 positions에, directions의 방향으로 생성
        for (int i = 0; i < positions.Count; i++) {
            UnitInfo unitInfo = VolatileData.stageData.GetUnitInfos().Find(info => info.codeName == codeName);

            int range = 0;
            Vector2Int? position = null;
            do {
                foreach (var tile in Utility.TilesInDiamondRange(positions[i], 0, range, 0))
					if(!tile.IsUnitOnTile()) {
                        position = tile.Location;
                        break;
                    }
	            
                range++;
            } while (position == null);

	        var genInfo = new UnitGenInfo((Vector2Int) position, directions[i]) {CodeName = unitInfo.codeName};
	        var unit = GenerateUnit(genInfo, false);

	        if (TileManager.Instance.GetTile(genInfo.Position).fogType != FogType.None) continue;
			if(regenEffect == null) regenEffect = Resources.Load("VisualEffect/Prefab/ControllerActive") as GameObject;
			LogManager.Instance.Record (new VisualEffectLog (unit, regenEffect, (Setting.basicCameraMoveDuration + 0.5f) * 2));
	        LogManager.Instance.Record(new CameraMoveLog(unit.transform.position, Setting.basicCameraMoveDuration));
	        LogManager.Instance.Record(new CameraMoveLog(null, 0.5f));
        }
    }*/
	static GameObject regenEffect;

	public void AllPassiveSkillsTriggerOnStageStart(){
		foreach (Unit caster in GetAllUnits())
			caster.ApplyTriggerOnStageStart ();
	}

    public void AllPassiveSkillsTriggerOnTurnStart(Unit turnStarter) {
        foreach (Unit caster in GetAllUnits())
            caster.GetListPassiveSkillLogic().TriggerOnAnyTurnStart(caster, turnStarter);
    }
    public void AllPassiveSkillsTriggerOnTurnEnd(Unit turnEnder){
	    foreach (Unit caster in GetAllUnits())
		    caster.GetListPassiveSkillLogic().TriggerOnTurnEnd(caster, turnEnder);
    }

    public IEnumerator StartPhase(int phase) {
        foreach (var unit in allUnits) {
            unit.movedTileCount = 0;
			unit.previousMoveCost = 0;
			unit.UpdateStartPosition();
			unit.ApplyTriggerOnPhaseStart(phase);
        }
        TileManager.Instance.TriggerTileStatusEffectsAtPhaseStart();
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
    }

	public void EndPhase(){
		foreach (var unit in allUnits) {unit.RegenerateActionPoint();}
		//행동력 회복시킨 후 순서 정렬하는 역할
        unitsActThisPhase.AddRange(unitsActNextPhase);
        unitsActNextPhase.Clear();

		foreach (var unit in allUnits) {unit.ApplyTriggerOnPhaseEnd();}
	}

    public void ReadTileBuffInfos() {
        if(BattleData.tileBuffInfos.Count != 0)
            return;
        foreach (var statusEffectInfo in StatusEffector.USEInfoList) {
            UnitStatusEffectInfo statusEffectToAdd = statusEffectInfo;
            if (statusEffectInfo.GetOwnerOfSkill() == "tile") {
                switch (statusEffectInfo.actuals[0].statusEffectType) {
                case StatusEffectType.PowerChange:
                    BattleData.tileBuffInfos.Add(Element.Fire, statusEffectToAdd);
                    break;
                case StatusEffectType.DefenseChange:
                    BattleData.tileBuffInfos.Add(Element.Metal, statusEffectToAdd);
                    break;
                case StatusEffectType.WillChange:
                    BattleData.tileBuffInfos.Add(Element.Water, statusEffectToAdd);
                    break;
                case StatusEffectType.HealOverPhase:
                    BattleData.tileBuffInfos.Add(Element.Plant, statusEffectToAdd);
                    break;
                default:
                    Debug.Log("fail reading tile buff infos");
                    break;
                }
            }
        }
    }

    void ReadOtherStatusEffectInfos(){
        foreach (var statusEffectInfo in StatusEffector.USEInfoList)
            if (statusEffectInfo.GetOwnerOfSkill() == "collecting")
                BattleData.collectingStatusEffectInfo = statusEffectInfo;
    }

	static List<string[]> collectTable;
    void ReadCollectableObjects(){
	    if (collectTable == null)
		    collectTable = Parser.GetMatrixTableFrom("Data/CollectableObjects");

	    foreach (var row in collectTable){
		    var units = GetAllUnits().FindAll(unit => _String.Match(unit.CodeName, row[0]));
		    foreach (var unit in units){
			    Collectables.Add(new Collectable(unit, row));
			    unit.objectTag = ObjectTag.Collectable;
			    unit.SetSpecialObjectIcon();
		    }
	    }
    }

	public void ReadAfterGeneration(){
        ReadTileBuffInfos();
        ReadOtherStatusEffectInfos();
        ReadCollectableObjects();
	}

    public void ApplyUSEsAtBattleStart(){
	    //var POVBuff = StatusEffector.FindUSE("주인공");
	    var HoldedDebuff = StatusEffector.FindUSE("붙잡힘");
	    var POVName = VolatileData.GetStageData(VolatileData.progress.stageNumber, StageInfoType.POV);
        foreach(var unit in GetAllUnits()){
	        //if (unit.CodeName == POVName)
		    //    StatusEffector.AttachAndReturnUSE(unit, new List<UnitStatusEffect> { new UnitStatusEffect(POVBuff, unit) }, unit, false);
	        if (unit.holdName == "") continue;
	        var target = GetAllUnits().Find(_unit => _unit.CodeName == unit.holdName);
	        if(target != null)
		        StatusEffector.AttachAndReturnUSE(unit, new List<UnitStatusEffect> { new UnitStatusEffect(HoldedDebuff, unit, target) }, target, false);
        }
    }
}
