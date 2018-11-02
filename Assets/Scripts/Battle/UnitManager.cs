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

    /*public void UpdateFogOfWar(){
        var TM = TileManager.Instance;
	    if (!TM.fogUsed)
		    return;

		Dictionary<Tile, FogType> originalFogTypesDict = new Dictionary<Tile, FogType>();
		foreach (var tile in TM.GetAllTiles().Values) {
			originalFogTypesDict.Add(tile, tile.fogType);
			tile.SetFogType(tile.GetInitialFogType());
		}
        foreach (var unit in GetAllUnits())
	        unit.RemoveFogsInSight();
		//위에서 안개 정보가 바뀐 후에 투명도를 맞춰야 하므로, 같은 GetAllUnits() 기반이더라도 합치지 말 것!
        foreach(var unit in GetAllUnits())
	        unit.UpdateTransparency();
		foreach(var kv in originalFogTypesDict) {
			if((int)kv.Key.fogType < (int)kv.Value) {
				Log lastLog = LogManager.Instance.GetLastEventLog();
				if (!(lastLog is MoveLog)) continue;
				((MoveLog)lastLog).sightChanged = true;
				break;
			}
		}
    }*/

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

			if (actionType != TrigActionType.Escape) {
				if (destroyedUnit.IsEnemyTo (unit)){
					unit.ChangeWill(WillChangeType.Cheer, destroyedUnit.IsNamed ? BigSmall.Big : BigSmall.Small);
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
        }

		foreach (var kv in TileManager.Instance.GetAllTiles()) {
			Tile tile = kv.Value;
			TileStatusEffect zoc = tile.StatusEffectList.Find (se => se.GetCaster () == destroyedUnit && se.IsTypeOf (StatusEffectType.ZOC));
			if (zoc != null) {
				tile.RemoveStatusEffect (zoc);
			}
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
	    
	    foreach (var guard in GetAllUnits().FindAll(item => item.CodeName == target.myInfo.connectedName))
		    LogManager.Instance.Record(new UnitDestroyLog(guard, TrigActionType.Retreat));
	    foreach (var holdee in GetAllUnits().FindAll(item => item.CodeName == target.myInfo.holdName)){
			holdee.GetAI()._AIData.SetActive(true);
		    holdee.RemoveStatusEffect(holdee.statusEffectList.Find(se => se.GetOriginSkillName() == "붙잡힘"));
	    }
    }

	public void ActivateGuard(List<Tile> tiles){
		foreach (var tile in tiles) {
			if (tile == null) {
				continue;
			}
			if(!BattleData.guardDict.ContainsKey(tile))
				continue;
			foreach (var unit in BattleData.guardDict[tile]){
				if (unit != null && unit.IsAI && !unit.GetComponent<AIData>().isActive && unit.GetComponent<AIData>().info.acute) 
					unit.GetComponent<AIData>().SetActive(true);
			}
		}
	}

	void ApplyAIInfo(Unit unit){
		var originalAiInfo = VolatileData.stageData.GetAIInfos().Find(info => _String.Match(info.codeName, unit.CodeName));
		if (originalAiInfo == null) return;
		
		var aiInfo = new AIInfo(originalAiInfo.cloneData);
		var aiData = unit.GetComponent<AIData>();
		
		if (aiInfo.actOnStart)
			aiData.isActive = true; //게임 시작 전이므로 Log를 통해 활성화할 수 없음!
		else{
			aiInfo.InstantiateVigilAreas(unit);
			foreach(var vigilArea in aiInfo.vigilAreas)
				foreach(var tile in vigilArea.area)
					if (BattleData.guardDict.ContainsKey(tile))
						BattleData.guardDict[tile].Add(unit);
					else
						BattleData.guardDict.Add(tile, new List<Unit> { unit });
		}
		
		aiData.info = aiInfo;
		aiData.SetGoalArea(unit);
		
		unit.GetActiveSkillList().RemoveAll (skill => skill.korName == "언령" || skill.korName == "광휘" || skill.korName == "집중" || skill.korName == "암살 표식" || skill.korName == "셔플 불릿" || skill.korName == "한여름");
		unit.AddAI();
	}

	public void CheckVigilAreaForAI(){
		foreach (var unit in GetAllUnits().FindAll(unit => unit.IsAI && !unit.GetAI().IsActive))
			unit.GetAI().CheckVigilArea();
	}

	public void AutomaticGeneration(bool onlyPC = false){
        genInfos = VolatileData.stageData.GetUnitGenInfos();

		foreach (var genInfo in genInfos){
			if (genInfo.IsNonFixedPosPC) continue;
			if(onlyPC && !genInfo.IsFixedPosPC) continue;
			if(genInfo.Additive) continue;
			if(genInfo.alreadyGenerated) continue;
			GenerateUnitWith(genInfo, false);
			if (genInfo.IsFixedPosPC && VolatileData.OpenCheck(Setting.readySceneOpenStage))
				ReadyManager.Instance.candidates.RemoveAll (_candi => "PC" + _candi.CodeName == genInfo.CodeName);
		}

        //UpdateFogOfWar();
    }

	private Unit GenerateUnitWith(UnitGenInfo genInfo, bool isSelectedFromReadyScene){
        var unit = Instantiate(unitPrefab).GetComponent<Unit>();
		genInfo.alreadyGenerated = true;
		var unitInfo = genInfo.CodeName.StartsWith("PC")
			? new UnitInfo(genInfo.CodeName.Substring(2), true)
			: VolatileData.stageData.GetUnitInfos().Find(info => info.codeName == genInfo.CodeName);
		Debug.Assert(unitInfo != null, genInfo.CodeName + "에 해당하는 unitInfo 없음!");
		unit.SetInfo(unitInfo);
		unit.genInfo = genInfo;
		
        unit.transform.SetParent(transform);
		unit.LoadSprites();
        unit.SetDirection(genInfo.Direction);
        unit.SetPivot(genInfo.Position);    // 유닛이 생성되자마자 fogOfWar 아래에 있으면 숨겨야 하는데,
                                            // pivot 세팅은 다음 프레임에 이루어지므로 미리 하지 않으면 숨겨지지 않는다.
	
        //AIInfo가 allUnits를 참조하고 unitsActThisPhase.Add 여부가 AI를 참조하므로, 아래 문단 내 순서를 함부로 바꾸지 말 것!
        allUnits.Add(unit);
		ApplyAIInfo(unit);
		
		var skills = new List<Skill>();
		//Debug.Log(isSelectedFromReadyScene + " / " + unit.IsPC + " / " + VolatileData.OpenCheck(Setting.readySceneOpenStage));
		if(!isSelectedFromReadyScene && (!unit.IsPC || !VolatileData.OpenCheck(Setting.readySceneOpenStage))){
			var mySkills = VolatileData.SkillsOf(unitInfo.codeName, unit.IsPC);
			if(VolatileData.OpenCheck(Setting.passiveOpenStage))
				skills.AddRange(mySkills);
			else
				foreach (var skill in mySkills)
					if(skill is ActiveSkill)
						skills.Add(skill);
		}else
			skills = ReadyManager.Instance.pickedList.Find(cand => cand.CodeName == unitInfo.codeName).selectedSkills;
		
        unit.ApplySkillList(skills, StatusEffector.USEInfoList, StatusEffector.TSEInfoList);
        if(unit.HasAction) unitsActThisPhase.Add(unit);

		unit.healthViewer.SetInitHealth(unit.myInfo.baseStats[Stat.MaxHealth], unit);
        if(unit.IsPC){
            var skillNameList = unit.GetPassiveSkillList().FindAll(skill => skill.RequireLevel > 0).Select(skill => skill.Name).ToList();
            skillNameList.AddRange(unit.GetActiveSkillList().Select(skill => skill.GetName()));
            PCSelectedSkillList.Add(unit.CodeName, skillNameList);
        }

        //평균 위치 계산하고, 소속 타일 중 가장 앞에 있는 것보다 position.z를 0.05f 당김
        var tiles = unit.TilesUnderUnit;
        float zValue = tiles[0].transform.position.z;
        tiles.ForEach(tile => {
            tile.SetUnitOnTile(unit);
            if(tile.transform.position.z < zValue) {zValue = tile.transform.position.z;}
        });
        var averagePos = Utility.AveragePos(tiles);
		unit.transform.position = new Vector3(averagePos.x, averagePos.y, zValue - 0.05f);
        
        return unit;
    }

	Unit GenerateVirtualUnitWith(UnitGenInfo genInfo){
		var unit = Instantiate(unitPrefab).GetComponent<Unit>();
		unit.SetInfo(genInfo.CodeName.StartsWith("PC") ? new UnitInfo(genInfo.CodeName.Substring(2), true) : VolatileData.stageData.GetUnitInfos().Find(info => info.codeName == genInfo.CodeName));
		unit.genInfo = genInfo;

		unit.transform.SetParent(transform);
		unit.SetDirection(genInfo.Direction);
		unit.SetPivot(genInfo.Position);

		var tiles = unit.TilesUnderUnit;
		float zValue = tiles[0].transform.position.z;
		tiles.ForEach(tile => {
			if(tile.transform.position.z < zValue) {zValue = tile.transform.position.z;}
		});
		var averagePos = Utility.AveragePos(tiles);
		unit.transform.position = new Vector3(averagePos.x, averagePos.y, zValue - 0.05f);

		return unit;
	}

    public IEnumerator ManualGeneration(){
	    var RM = FindObjectOfType<ReadyManager>();
		if(!Utility.needsManualGeneration || RM == null) yield break;
			
		BattleUIManager.Instance.EnablePlacedUnitCheckUI();
		
        if (Utility.needsManualGeneration){
            ReadyForManualGeneration();
            yield return GenerateUnitsManually();
        }

	    TileManager.Instance.ClearAllTileColors();
	    Destroy(RM.gameObject);
        yield return null;
	    for (int i = 0; i < GetAllUnits().Count; i++)
		    GetAllUnits()[i].RemoveFogsInSight();
    }

	private IEnumerator GenerateUnitsManually(){
        var triggers = BattleManager.Instance.triggers;
		var pickedList = ReadyManager.Instance.pickedList;
		var TM = TileManager.Instance;
		
		playButton.SetActive (false);

        while(generatedPC < pickedList.Count){
			var presentUnit = pickedList [generatedPC];
			FindObjectOfType<PlacedUnitCheckPanel>().HighlightPortrait(presentUnit.CodeName);
			BattleData.isWaitingUserInput = true;
			BattleData.unitToGenerate = presentUnit;

	        if (VolatileData.gameMode == GameMode.AllStageTest)
		        BattleManager.Instance.OnMouseDownHandlerFromTile(Generic.PickRandom(TileManager.Instance.GetTilesInGlobalRange().FindAll(tile => tile.IsPreSelected())).Pos);
	        else
				yield return EventTrigger.WaitOr(triggers.resetUnitInput, triggers.tileSelected);
	        
            if (triggers.resetUnitInput.Triggered) {
                PCSelectedSkillList = new Dictionary<string, List<string>>();
				BattleData.unitToGenerate = null;
				DestroyAfterImageOfGeneration ();
                yield return GenerateUnitsManually();
                yield break;
            }

			BattleData.isWaitingUserInput = false;

			BattleData.unitToGenerate = null;
			DestroyAfterImageOfGeneration ();
		
			Vector2Int triggeredPos = BattleData.move.selectedTilePosition;
	        UnitGenInfo genInfo = genInfos.Find(item => item.Position == triggeredPos);
			genInfo.CodeName = "PC" + presentUnit.CodeName;

			GenerateUnitWith(genInfo, true);

	        var triggeredTiles = new List<Tile> {TileManager.Instance.GetTile(triggeredPos)};
	        TM.DepaintTiles(triggeredTiles, TM.TileColorMaterialForMove);
			TM.DepreselectTiles(triggeredTiles);
            generatedPC += 1;

			SoundManager.Instance.PlaySE ("Click");
        }

		playButton.SetActive (true);

        // 배치 가능 위치 지우고 턴 시작
		var placingPanel = FindObjectOfType<PlacedUnitCheckPanel>();
		if (placingPanel != null){
			placingPanel.text.text = Language.Select("배치를 이대로 확정할까요?", "Confirm the locations?");
			if (VolatileData.gameMode == GameMode.AllStageTest){
				Destroy(placingPanel.gameObject);
				yield break;
			}
		}
		
		yield return EventTrigger.WaitOr(triggers.resetUnitInput, triggers.finishUnitInput);
		if (!triggers.resetUnitInput.Triggered) yield break;
		
		PCSelectedSkillList = new Dictionary<string, List<string>>();
		BattleData.unitToGenerate = null;
		DestroyAfterImageOfGeneration ();
		yield return GenerateUnitsManually();
	}

	Unit virtualUnit;
	public void ShowAfterImageOfGeneration(Vector2 pos){
		if (virtualUnit != null)
			return;
		UnitGenInfo genInfo = genInfos.Find(item => item.Position == pos);
		genInfo.CodeName = "PC" + BattleData.unitToGenerate.CodeName;
		virtualUnit = GenerateVirtualUnitWith (genInfo);
		virtualUnit.SetAlpha (0.5f);
	}
	public void DestroyAfterImageOfGeneration(){
		if (virtualUnit == null) return;
		genInfos.Find(item => item.Position == virtualUnit.Pos).CodeName = "selected";
		Destroy (virtualUnit.gameObject);
		virtualUnit = null;
	}

    //초기화 버튼 눌렀을 때 호출
	public void ResetManualGeneration(){
		SoundManager.Instance.PlaySE ("Cancel");
        generatedPC = 0;
        FindObjectOfType<PlacedUnitCheckPanel>().ResetHighlight();
		foreach (var image in FindObjectOfType<PlacedUnitCheckPanel>().unitPortraitList){
			var generatedUnit = GetAllUnits().Find(unit => unit.IsPC && image.sprite.name.EndsWith(unit.CodeName));
			if(generatedUnit == null) break;
			
			generatedUnit.genInfo.CodeName = "selected";
			allUnits.Remove(generatedUnit);
			Destroy(generatedUnit.gameObject);
		}

        TileManager.Instance.ClearAllTileColors();
        TileManager.Instance.DepreselectAllTiles();
        ReadyForManualGeneration();
        BattleData.triggers.resetUnitInput.Trigger();
    }
    
    void ReadyForManualGeneration(){
		var selectableTileList = new List<Tile>();
	    var TM = TileManager.Instance;
		genInfos.FindAll(genInfo => genInfo.IsNonFixedPosPC).ForEach(genInfo => selectableTileList.Add(TileManager.Instance.GetTile(genInfo.Position)));
		TM.PaintTiles(selectableTileList, TM.TileColorMaterialForMove);
		TM.PreselectTiles(selectableTileList);
    }

    public void FinishManualGeneration(){
        BattleData.triggers.finishUnitInput.Trigger();
    }

    public void GenerateUnitsAtPosition(string codeName, List<Vector2Int> positions, List<Direction> directions){
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
	        var unit = GenerateUnitWith(genInfo, false);

	        if (TileManager.Instance.GetTile(genInfo.Position).fogType != FogType.None) continue;
			if(regenEffect == null) regenEffect = Resources.Load("VisualEffect/Prefab/ControllerActive") as GameObject;
			LogManager.Instance.Record (new VisualEffectLog (unit, regenEffect, (Setting.basicCameraMoveDuration + 0.5f) * 2));
	        LogManager.Instance.Record(new CameraMoveLog(unit.transform.position, Setting.basicCameraMoveDuration));
	        LogManager.Instance.Record(new CameraMoveLog(null, 0.5f));
        }
    }
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
			    unit.myInfo.objectTag = ObjectTag.Collectable;
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
	    var POVBuff = StatusEffector.FindUSE("주인공");
	    var HoldedDebuff = StatusEffector.FindUSE("붙잡힘");
	    var POVName = VolatileData.GetStageData(VolatileData.progress.stageNumber, StageInfoType.POV);
        foreach(var unit in GetAllUnits()){
	        if (unit.CodeName == POVName)
		        StatusEffector.AttachAndReturnUSE(unit, new List<UnitStatusEffect> { new UnitStatusEffect(POVBuff, unit) }, unit, false);
	        if (unit.myInfo.holdName == "") continue;
	        var target = GetAllUnits().Find(_unit => _unit.CodeName == unit.myInfo.holdName);
	        if(target != null)
		        StatusEffector.AttachAndReturnUSE(unit, new List<UnitStatusEffect> { new UnitStatusEffect(HoldedDebuff, unit, target) }, target, false);
        }
    }
}
