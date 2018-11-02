using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Battle.Turn;
using GameData;
using Save;
using DG.Tweening;
using Camerawork;
using UtilityMethods;

public class BattleManager : MonoBehaviour{
    public CameraMover cameraMover;
	public TutorialController TutorialController;
	static BattleManager instance;
	public static BattleManager Instance{ get { return instance; } }
	public BattleData.Triggers triggers;
    public Image bloodyScreen;
    public ClickEffect clickEffect;

    void Awake (){
		if (instance != null && instance != this) {
			Destroy (gameObject);
			return;
		}
	    instance = this;
		BattleData.Initialize ();
		triggers = BattleData.triggers;

	    VolatileData.stageData.Load(true);
	    LogManager.SetInstance();
	    TileManager.SetInstance();
		TileManager.Instance.GenerateMap();
		SkillLocation.tileManager = TileManager.Instance;
	}

	public IEnumerator Start(){
		var UM = UnitManager.Instance;
		SoundManager.Instance.PlayBGM ("BGM_Urgent");
		bloodyScreen.material.SetFloat("_HP_Percent", 1);
		
		yield return MoveCameraToTileAveragePosAndZoomOut();
		UM.AutomaticGeneration();
		BattleUIInitialize();
		
		UM.ReadAfterGeneration();
		StartCoroutine (TurnManager ());
    }

	IEnumerator WaitForClickingConditionPanel() {
		// condition panel이 사라진 이후 유닛 배치 UI가 뜨고, 그 이후 유닛 배치를 해야 하므로 일시정지.
		while (true) {
			if ((Input.GetMouseButtonDown(0) || VolatileData.gameMode == GameMode.AllStageTest)
			    && FindObjectOfType<SceneLoader>().loaded) break;
			yield return null;
		}
		// 승리/패배조건을 움직임
		yield return BattleUIManager.Instance.MoveConditionText();
	}

	IEnumerator MoveCameraToTileAveragePosAndZoomOut() {
		//모든 타일 위치의 평균값으로 카메라를 이동
		var values = TileManager.Instance.GetAllTiles().Values.ToList();
		cameraMover.MoveCameraToAveragePosition(values);
		yield return cameraMover.ZoomOutCameraToViewMap(0);
	}

	void RecordBattleStartPlayLog() {
		UnitManager UM = UnitManager.Instance;
		StageNum stage = VolatileData.progress.stageNumber;
		int trial = 1;
		Dictionary<string, List<string>> selectedSkills = new Dictionary<string, List<string>>();
		if (RecordData.stageClearRecords.ContainsKey(stage))
			trial = RecordData.stageClearRecords[stage].Count + 1;
		if(VolatileData.OpenCheck(Setting.readySceneOpenStage)) {
			selectedSkills = UM.PCSelectedSkillList;
		}
		GameDataManager.RecordPlayLog(new BattleStartPlayLog(stage, trial, VolatileData.difficulty, selectedSkills));
	}

	IEnumerator ShowDialogueScript(string scriptName) {
		if (RecordData.alreadyReadDialogues.Contains(scriptName)) yield break;

		VolatileData.progress.dialogueName = scriptName;
		yield return DialogueManager.Instance.Initialize();
		DialogueManager.Instance.gameObject.SetActive(false);
		RecordData.alreadyReadDialogues.Add(scriptName);
	}

    bool startFinished;

	//유닛 배치가 끝난 후에 호출된다(18.04.02)
	void BattleUIInitialize(){
		BattleData.SetTurnUnit();
        BattleUIManager.Instance.ActivateAPBarUI(true);
		BattleUIManager.Instance.UpdateApBarUI();
        BattleUIManager.Instance.SetSelectedUnitViewerUI(BattleData.turnUnit);
        startFinished = true;
		BattleUIManager.Instance.UpdateSmallConditionTexts();
        BattleUIManager.Instance.SlideUIsIn(Setting.slideUISlowDuration);
		StartCoroutine(BattleUIManager.Instance.battleUICanvas.transform.Find("UpperUI").GetComponentInChildren<TogglePosition>().Move());
	}

	IEnumerator MoveCameraToPCAveragePos() {
		//PC 평균 위치로 카메라 이동
		var Allies = UnitManager.GetAllUnits().FindAll(unit => unit.IsPC).ConvertAll(unit => (MonoBehaviour) unit);
		Vector3 pos = Utility.AveragePos(Allies);
		cameraMover.SetFixedPosition(pos);
		yield return cameraMover.ZoomInBack(1);
	}

	void CalculateBattleTriggerRequireCounts() {
		//reqCount가 -1인 모든 BattleTrigger에 대해
		BattleTriggerManager.Instance.triggers.FindAll(trig => trig.reqCount == -1).ForEach(trig => {
			//target이 Any이면 actor의 UnitType에 해당하는 유닛 수만큼, 아니면 target에 해당하는 유닛 수만큼으로 reqCount를 변경
			trig.reqCount = UnitManager.GetAllUnits().FindAll(unit => {
				TrigUnitCheckType checkType = (TrigUnitCheckType) Enum.Parse(typeof(TrigUnitCheckType), trig.reqCountString);
				return BattleTriggerManager.Instance.CheckUnitType(trig, unit, checkType);
			}).Count;
		});
	}

	IEnumerator TurnManager(){
		var UM = UnitManager.Instance;
		var UIM = BattleUIManager.Instance;
		var LM = LogManager.Instance;
        while (!(startFinished && UIM.startFinished))
            yield return null;
		LM.Record(new BattleStartLog());
		UM.AllPassiveSkillsTriggerOnStageStart ();
		yield return LM.ExecuteLastEventLogAndConsequences(); //Record true로 합치지 말 것!
		UnitManager.Instance.UpdateStatusEffectsAtActionEnd();
		bool stageTutoStarted = false;
        while (true) {
            yield return StartPhaseOnGameManager();
		        
			while(UM.unitsActThisPhase.Count != 0){
				//전투에 승리해서 결과창이 나오면 진행 정지
				if (UIM.resultPanel.gameObject.activeSelf) {yield break;}
				
				BattleData.SetTurnUnit();
				var unit = BattleData.turnUnit;
				LM.Record(new TurnStartLog(unit));
				yield return LM.ExecuteLastEventLogAndConsequences(unit.CodeName + "의 TurnStartLog");
				UM.DeactivateAllDamageTextObjects();
				UIM.UpdateApBarUI();

				if (BattleData.turnUnit.IsAI){
					BattleData.currentState = CurrentState.AITurn;
					yield return unit.GetAI().UnitTurn();
                }else{
					if (!stageTutoStarted){
						TutorialController.gameObject.SetActive(true);
						stageTutoStarted = true;
					}
					yield return ActionAtTurn(unit);
				}

				UM.unitsActThisPhase.Remove(unit);
                if (unitDestroyedDuringOwnTurn == null)
	                UM.unitsActNextPhase.Add(unit);
	            unitDestroyedDuringOwnTurn = null;
				
				LM.Record(new TurnEndLog(unit));
				yield return LM.ExecuteLastEventLogAndConsequences();
			}

			yield return EndPhaseOnGameManager();
		}
	}

	void UpdateAPBarAndMoveCameraToSelectedUnit(Unit unit){
		BattleUIManager.Instance.UpdateApBarUI();
		if (unit == null)
			return;
		cameraMover.SetFixedPosition(unit.realPosition);
	}
	public void ShowUnitTurnStart(Unit unit){
		UpdateAPBarAndMoveCameraToSelectedUnit (unit);
        RenewBloodyScreen();
        BattleData.move = new BattleData.Move();

		UnitManager.Instance.AllPassiveSkillsTriggerOnTurnStart(unit);
		BattleUIManager.Instance.SetSelectedUnitViewerUI(unit);
		unit.ShowArrow();
	}

    public Unit unitDestroyedDuringOwnTurn;
	public static bool IsSelectedUnitRetreatOrDie(){
		var actor = BattleData.turnUnit;
		return actor.GetHP < actor.RetreatHP || actor.CheckEscape();
	}

	public IEnumerator CheckWinOrLoseTriggers() {
        //승리 조건이 충족되었는지 확인
        yield return CheckTriggers(TrigResultType.Win);
        yield return CheckTriggers(TrigResultType.Lose);
    }
    IEnumerator CheckTriggers(TrigResultType type) {
        //type은 Win 또는 Lose라고 가정
        BattleTriggerManager BTM = BattleTriggerManager.Instance;

        List<BattleTrigger> triggers = BTM.triggers.FindAll(trig => trig.result == type);
        BattleTrigger info = BTM.triggers.Find(trig => trig.result == TrigResultType.Info);
        BattleTrigger.TriggerRelation trigRelation = (type == TrigResultType.Win) ? info.winTriggerRelation : info.loseTriggerRelation;

        //All이나 Sequence이면 전부 달성했을 때, One이면 하나라도 달성했을 때 승리
        if (((trigRelation == BattleTrigger.TriggerRelation.All || trigRelation == BattleTrigger.TriggerRelation.Sequence) &&
            triggers.All(trig => trig.acquired)) ||
            (trigRelation == BattleTrigger.TriggerRelation.One && triggers.Any(trig => trig.acquired))){
			
			yield return BTM.EndBattle(type == TrigResultType.Win);
        }
    }

	public UnityEvent readyCommandEvent;

	private IEnumerator ActionAtTurn(Unit unit){
		var LM = LogManager.Instance;
		var TM = TileManager.Instance;
		var UIM = BattleUIManager.Instance;

		BattleData.currentState = CurrentState.FocusToUnit;
		if(unit.HasStatusEffect(StatusEffectType.Faint)){
			yield return unit.ShowFaint();
			BattleData.currentState = CurrentState.AfterAction;
			yield break;
		}
		
		UnitStatusEffect collectSE = unit.statusEffectList.Find (item => item.IsTypeOf (StatusEffectType.Collect));
		if(collectSE != null && collectSE.Duration() >= 2){
			BattleData.currentState = CurrentState.AfterAction;
			yield break;
		}
		
		while (BattleData.currentState == CurrentState.FocusToUnit){
            RenewBloodyScreen();
			
			if (BattleData.unitInSelectedUnitViewer == null && BattleData.tileInTileViewer == null) {
				yield return cameraMover.Slide (unit.transform.position, Setting.basicCameraMoveDuration);
			} else if (BattleData.unitInSelectedUnitViewer != null) {
				yield return cameraMover.Slide (BattleData.unitInSelectedUnitViewer.transform.position, Setting.basicCameraMoveDuration);
			} else if (BattleData.tileInTileViewer != null) {
				yield return cameraMover.Slide (BattleData.tileInTileViewer.transform.position, Setting.basicCameraMoveDuration);
			}

            if (IsSelectedUnitRetreatOrDie()) {
				BattleData.currentState = CurrentState.Destroyed;
				yield break;
			}

			ReadyForUnitAction ();

			TM.PreselectTiles(TM.GetTilesInGlobalRange(onlyVisible : true));
			BattleData.isWaitingUserInput = true;

			var movableTilesWithPath = new Dictionary<Vector2Int, TileWithPath>();
			var movableTiles = new List<Tile>();
			IEnumerator update = null;

			if (unit.HasStatusEffect(StatusEffectType.Taunt)){
				unit.GetComponent<AIData>().isActive = true;
				unit.AddAI();
				yield return unit.GetAI().UnitTurn();
				Destroy(unit.GetAI());
				yield break;
			}
			
			//이동 가능한 범위 표시
			if(unit.IsMovePossibleState()){
				movableTilesWithPath = unit.MovableTilesWithPath;
				movableTiles = movableTilesWithPath.Keys.ToList().ConvertAll(vec2 => TM.GetTile(vec2));
				TM.PaintTiles(movableTiles, TM.TileColorMaterialForMove);
				update = UpdatePreviewPathAndAP(movableTilesWithPath);
				StartCoroutine(update);
			}

			if (VolatileData.gameMode == GameMode.AllStageTest)
				StartCoroutine (BattleTriggerManager.Instance.EndBattle(true));

			//튜토리얼이 켜져있으면 다음 단계로 넘어가는 기능을 실행. 나중에 더 용도를 찾을 수도 있다
			readyCommandEvent.Invoke();
			if(TutorialController != null && !TutorialController.gameObject.activeSelf)
				TutorialController.gameObject.SetActive(true);

			yield return EventTrigger.WaitOr(triggers.actionCommand, triggers.rightClicked, triggers.skillSelected,
											 triggers.tileSelected, triggers.tileLongSelected, triggers.cheatKeyPressed);

            if (update != null)
				StopCoroutine(update);
			
			unit.HideAfterImage ();
			TM.ClearAllTileColors ();
			Tile tileUnderMouse = TM.preSelectedMouseOverTile;
			if (tileUnderMouse != null)
				tileUnderMouse.CostAP.gameObject.SetActive(false);

			BattleUIManager.Instance.selectedUnitViewerUI.OffPreviewAp();
			Log lastLog = LM.GetLastEventLog();
			if (triggers.rightClicked.Triggered && lastLog is MoveLog) {
				if (!((MoveLog) lastLog).sightChanged) {
					SoundManager.Instance.PlaySE("Cancel");
					LM.RewindLastEventLog();
				}
			}else if (triggers.tileSelected.Triggered){
				var clickedTile = BattleData.SelectedTile;
				if (movableTiles.Contains(clickedTile)){
					SoundManager.Instance.PlaySE ("Click");
					TM.DepreselectAllTiles ();
					BattleData.currentState = CurrentState.AfterAction;
					var destPos = BattleData.move.selectedTilePosition;
					MoveToTile (destPos, movableTilesWithPath);
				}else if(clickedTile.IsUnitOnTile() && VolatileData.OpenCheck(Setting.detailInfoOpenStage)){
					SoundManager.Instance.PlaySE ("Click");
					UIM.ActivateDetailInfoUI (clickedTile.GetUnitOnTile ());
				}else
					SoundManager.Instance.PlaySE ("Cannot");
			}else if(triggers.skillSelected.Triggered){
				yield return SkillAndChainStates.SelectSkillApplyArea();
				UIM.PreviewAp(null);				
            }else if (triggers.actionCommand.Data == ActionCommand.Standby){
				BattleData.currentState = CurrentState.AfterAction;
                WaitTimeForStandby(unit);
			}else if (triggers.actionCommand.Data == ActionCommand.Collect) {
                BattleData.currentState = CurrentState.AfterAction;
                LM.Record(new CollectStartLog(unit, BattleData.NearestCollectable.unit));
                BattleData.turnUnit.CollectNearestCollectable();
            }
            yield return LM.ExecuteLastEventLogAndConsequences(unit.CodeName + "의 행동 완료");
		}
		UIM.HideActionButtons();
	}
	public void MoveToTile(Vector2Int destPos, Dictionary<Vector2Int, TileWithPath> pathDict) {
		Unit unit = BattleData.turnUnit;
		List<Tile> realDestPath = new List<Tile> ();
		bool trapOperated = false;
		foreach(var tile in pathDict[destPos].fullPath){
			realDestPath.Add (tile);
			List<TileStatusEffect> traps = TileManager.Instance.FindTrapsOnThisTile(tile);
			foreach (var trap in traps) 
				if (unit.GetListPassiveSkillLogic().TriggerOnSteppingTrap(unit, trap)) 
					trapOperated = true;
                
			if (trapOperated)
				break;
		}

		unit.ApplyMove(pathDict[realDestPath.Last().Pos]);
	}

	public void MoveCameraToUnitAndDisplayUnitInfoViewer(Unit unit) {
		BattleUIManager.Instance.SetMovedUICanvasOnUnitAsCenter(unit);
		BattleUIManager.Instance.SetSelectedUnitViewerUI(unit);
	}

    float flickerStartTime;
    public IEnumerator FadeBloodyScreen(float endValue, float duration) {
        Tweener tw = bloodyScreen.material.DOFloat(endValue, "_HP_Percent", duration);
        yield return tw.WaitForCompletion();
    }

    public IEnumerator FlickerBloodyScreen(float maxBloodyness, float duration, bool leaveBloody = false) {
        if (!leaveBloody) {
            float originalHPPercent = bloodyScreen.material.GetFloat("_HP_Percent");
            yield return FadeBloodyScreen(1 - maxBloodyness, duration * 0.25f);
            yield return FadeBloodyScreen(originalHPPercent, duration * 0.75f);
        }
        else yield return FadeBloodyScreen(1 - maxBloodyness, duration);
    }

    public void RenewBloodyScreen() {
        if (BattleData.IsUserTurn){
	        float HPPercent = BattleData.turnUnit.GetHpRatio();
            if (HPPercent < 0.5f) {
                bloodyScreen.material.SetFloat("_HP_Percent", HPPercent);
                return;
            }
        }
        bloodyScreen.material.SetFloat("_HP_Percent", 1);
    }

	public void ReadyForUnitAction(){
		MoveCameraToUnitAndDisplayUnitInfoViewer(BattleData.turnUnit);
		UpdateAPBarAndMoveCameraToSelectedUnit (BattleData.turnUnit);
		
		if(BattleData.IsUserTurn)
			BattleUIManager.Instance.SetActionButtons();
	}

	public void CallbackMoveCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Move);
	}

	public void CallbackSkillCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Skill);
	}

	public void CallbackStandbyCommand(){
		triggers.actionCommand.Trigger(ActionCommand.Standby);
	}

    public void CallbackCollectCommand() {
        triggers.actionCommand.Trigger(ActionCommand.Collect);
    }

	public void CallbackOnPointerEnterRestCommand(){
		BattleUIManager.Instance.PreviewAp(new APAction(APAction.Action.Rest, BattleData.turnUnit.ApOverflow));
	}

	public void CallbackOnPointerExitRestCommand(){
		BattleUIManager.Instance.PreviewAp(null);
	}

	public static void WaitTimeForStandby(Unit unit){
        LogManager.Instance.Record(new WaitForSecondsLog(0.1f));
	}

	public void CallbackSkillSelect(){
		triggers.skillSelected.Trigger();
	}

	void CallbackRightClick(){
		triggers.rightClicked.Trigger();
	}

	public void CallbackDirection(){
		if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !BattleData.longClickLock)
			triggers.directionLongSelected.Trigger();
		else
			triggers.directionSelected.Trigger();
		BattleUIManager.Instance.SetDirectionUIActive(false);
	}
	public void CallbackDirectionLong(Direction direction){
		triggers.directionLongSelected.Trigger();
		BattleUIManager.Instance.SetDirectionUIActive(false);
	}

    void Update(){
	    var UIM = BattleUIManager.Instance;
		if(Input.GetMouseButtonDown(1)){
		    if(!BattleData.rightClickLock && BattleUIManager.Instance.DeactivateTopUI() == null)
		    	CallbackRightClick();// 가장 위에 있는 UI가 있다면 deactivate하고, 아니라면 명령 취소 등을 한다.
	    }

        if (BattleData.currentState != CurrentState.FocusToUnit)
			UIM.ReleaseViewerHold (); // 행동을 선택하면 홀드가 자동으로 풀림.

	    if (Input.GetKeyDown(KeyCode.Tab)){
		    int order = 1;
		    foreach (var unit in UnitManager.Instance.unitsActThisPhase){
			    if(!unit.IsActive || unit.IsHiddenUnderFogOfWar()) continue;
			    unit.OrderText.gameObject.SetActive(true);
			    unit.OrderText.text = order.ToString();
			    order += 1;
		    }
		    foreach (var unit in UnitManager.Instance.unitsActNextPhase){
			    if(!unit.IsActive || unit.IsHiddenUnderFogOfWar()) continue;
			    unit.OrderText.gameObject.SetActive(true);
			    unit.OrderText.text = order.ToString();
			    order += 1;
		    }
	    }if (Input.GetKeyUp(KeyCode.Tab))
		    foreach (var unit in UnitManager.GetAllUnits())
			    unit.OrderText.gameObject.SetActive(false);
	    
		if (Input.GetKeyDown(KeyCode.N))
			StartCoroutine (BattleTriggerManager.Instance.EndBattle(true));
		if (Input.GetKeyDown(KeyCode.Delete))
			StartCoroutine(BattleTriggerManager.Instance.EndBattle(false));
		if(Input.GetKeyDown(KeyCode.X)) {
			Unit mouseOverUnit = TileManager.Instance.mouseOverUnit;
			if (mouseOverUnit != null) {
				LogManager.Instance.Record(new UnitDestroyLog(mouseOverUnit, TrigActionType.Kill));
				triggers.cheatKeyPressed.Trigger();
			}
		}
        if(Input.GetKeyDown(KeyCode.Space) && startFinished && AllowCameraChange){
            if (!cameraMover.zoomedOut) {
                UIM.SlideUIsOut(Setting.slideUIFastDuration);
                StartCoroutine(cameraMover.ZoomOutCameraToViewMap(Setting.cameraZoomDuration));
            } else {
                UIM.SlideUIsIn(Setting.slideUIFastDuration);
                StartCoroutine(cameraMover.ZoomInBack(Setting.cameraZoomDuration));
            }
        }

	    if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift))
		    UnitManager.Instance.ShowAllChains();
	    if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
		    UnitManager.Instance.HideAllChains();
        if(!UIM.logDisplayPanel.gameObject.activeSelf){
            float size = 1;
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                size = (float)Math.Pow(3, 0.25f);
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                size = 1 / (float)Math.Pow(3, 0.25f);
            if (size != 1)
                StartCoroutine(cameraMover.Zoom(size, Setting.cameraZoomDuration, relative : true));
        }

	    if (Input.GetKeyDown(KeyCode.A))
		    ChangeAspect(1);
		if (Input.GetKeyDown(KeyCode.D))
		    ChangeAspect(-1);

	    if(Input.GetKeyDown(KeyCode.P))
			Debug.LogError("Input P -> Pause Game");
	}

	public bool AllowCameraChange{get{
			//Debug.Log(BattleData.isWaitingUserInput +"/"+ cameraMover.Movable +"/"+ (BattleData.TutoState == TutorialState.None) +"/"+ VolatileData.OpenCheck(Setting.cameraMoveOpenStage));
			return BattleData.isWaitingUserInput && cameraMover.Movable && BattleData.TutoState == TutorialState.None
			       && VolatileData.OpenCheck(Setting.cameraMoveOpenStage);
		}
	}
	private void ChangeAspect(int direction) { // direction = 1 : 반시계 방향, direction = -1 : 시계방향
		if(!AllowCameraChange) return;
		
        var tileManager = TileManager.Instance;
        var unitManager = UnitManager.Instance;
        int aspectBefore = (int)BattleData.aspect;
        int aspectAfter = (aspectBefore + direction + 4) % 4;
        BattleData.aspect = (Aspect)aspectAfter;
        tileManager.UpdateRealTilePositions();
        unitManager.UpdateRealUnitPositions();
        
		if(BattleData.turnUnit == null){
			var tiles = TileManager.Instance.GetAllTiles().Values.ToList();
			cameraMover.MoveCameraToAveragePosition(tiles);
		}else{
			cameraMover.SetFixedPosition(BattleData.turnUnit.realPosition);
			cameraMover.MoveCameraToPosition(cameraMover.fixedPosition);
			BattleUIManager.Instance.SetMovedUICanvasOnUnitAsCenter(BattleData.turnUnit);
		}
        cameraMover.CalculateBoundary();

		if(BattleData.TutoState == TutorialState.Active && FindObjectOfType<TutorialController>().CurrentScenario != null){
			FindObjectOfType<TutorialController>().CurrentScenario.UpdateAspect();
		}
    }

	public void OnMouseDownHandlerFromTile(Vector2Int position){
		//Debug.Log("타일 클릭됨");
		if (!BattleData.isWaitingUserInput) return;
		
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			triggers.tileLongSelected.Trigger();
		else
			triggers.tileSelected.Trigger();
		BattleData.move.selectedTilePosition = position;
	}
	public void OnLongMouseDownHandlerFromTile(Vector2Int position){
		if (!BattleData.isWaitingUserInput) return;
		
		triggers.tileLongSelected.Trigger();
		BattleData.move.selectedTilePosition = position;
	}

	IEnumerator EndPhaseOnGameManager(){
        int phase = BattleData.currentPhase;
        LogManager.Instance.Record(new PhaseEndLog(phase));
        UnitManager.Instance.EndPhase();
        TileManager.Instance.EndPhase(phase);
        yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
        yield return new WaitForSeconds(Setting.phasePassTime);
	}

	IEnumerator StartPhaseOnGameManager(){
		BattleData.currentPhase++;
        int phase = BattleData.currentPhase;

		yield return BattleUIManager.Instance.MovePhaseUI(BattleData.currentPhase);
        LogManager.Instance.Record(new PhaseStartLog(phase));
        yield return UnitManager.Instance.StartPhase(BattleData.currentPhase);

		yield return new WaitForSeconds(Setting.phasePassTime);
		if(phase == 2)
			TutorialManager.subjectQueue.Enqueue("Phase");
	}

	public List<Tile> escapeSpotTiles = new List<Tile>();
	// 승/패 조건과 관련된 타일을 하이라이트 처리

	void SetEscapeSpotTiles() {
		var tileTriggers = BattleTriggerManager.Instance.triggers.FindAll(
			bt => (bt.action == TrigActionType.Escape || bt.action == TrigActionType.StepOnTile)
			&& (bt.result == TrigResultType.Win || bt.result == TrigResultType.Lose)
		);

		tileTriggers.ForEach(trigger => {
			escapeSpotTiles.AddRange(trigger.targetTiles.ConvertAll(tilePos => TileManager.Instance.GetTile(tilePos)));
		});
	}

	private static IEnumerator UpdatePreviewPathAndAP(Dictionary<Vector2Int, TileWithPath> movableTilesWithPath){
		var TM = TileManager.Instance;
		Tile prevMouseOverTile = null;
		var unit = BattleData.turnUnit;
		while (true) {
			var newOverTile = TM.preSelectedMouseOverTile;
			TM.ShowProperTileColors();

			if (newOverTile == null || newOverTile == unit.TileUnderUnit || !movableTilesWithPath.ContainsKey(newOverTile.Location)){
				BattleUIManager.Instance.PreviewAp (null);
				unit.HideAfterImage ();
			} else {
				Vector2Int overTilePos = newOverTile.Location;
				int requiredAP = movableTilesWithPath [overTilePos].RequireActivityPoint;
				BattleUIManager.Instance.PreviewAp (new APAction (APAction.Action.Move, requiredAP));
					
				newOverTile.CostAP.gameObject.SetActive (true);
				newOverTile.CostAP.text = requiredAP.ToString ();
					
				List<Tile> path = movableTilesWithPath [overTilePos].fullPath;
				unit.SetAfterImageAt (overTilePos, Calculate.FinalDirOfPath(path, unit.GetDir()));
				foreach (var tile in path) {
					tile.ClearTileColor ();
					tile.PaintTile (TM.TileColorMaterialForRange2);
				}
			}
			yield return null;
		}
	}

	//PC 차례에만 사용할 것!
	public void SetProperVisualState(){
		TileManager.Instance.ShowProperTileColors();
		BattleUIManager.Instance.ShowProperPreviewAp();
		BattleUIManager.Instance.ShowOrHideDirectionUI();
	}
}
