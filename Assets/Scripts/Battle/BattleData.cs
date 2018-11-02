using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using GameData;
using Util;

public enum CurrentState{
	None, AITurn, Destroyed, FocusToUnit, SelectSkillApplyArea, AfterAction,
}

public enum ActionCommand {Waiting, Move, Skill, Rest, Standby, Collect, Cancel}

public interface IEventTrigger{
	void Begin();
	void End();
	void Revert();
	bool Triggered { get; }
}

public class EventTrigger: IEventTrigger{
	private bool enabled = false;
	private bool triggered = false;

	public bool Triggered
	{
		get { return triggered; }
	}

	public void Trigger(){
		if (enabled)
		{
			triggered = true;
		}
	}

	public void Revert(){
		if(enabled)
			triggered = false;
	}

	public IEnumerator Wait(){
		Begin();

		while (triggered == false)
		{
			yield return null;
		}

		End();
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void Begin()
	{
		enabled = true;
		triggered = false;
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void End()
	{
		enabled = false;
	}

	public static IEnumerator WaitOr(params IEventTrigger[] triggers){
		foreach (var trigger in triggers)
			trigger.Begin();
		
		bool looping = true;
		while (looping){
			BattleManager battleManager = BattleManager.Instance;
			foreach (var trigger in triggers){
				if (trigger.Triggered) {
					looping = false;
                    break;
				}
			}
			yield return null;
		}

		foreach (var trigger in triggers)
		{
			trigger.End();
		}
	}
}

public class EventTrigger<T>: IEventTrigger{
	private bool enabled;
	private bool triggered;
	private T data;

	public bool Triggered
	{
		get { return triggered; }
	}

	public T Data
	{
		get { return data; }
	}

	public void Trigger(T data)
	{
		if (enabled)
		{
			triggered = true;
			this.data = data;
		}
	}

	public IEnumerator Wait(){
		Begin();

		while (triggered == false)
			yield return null;

		End();
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void Begin()
	{
		enabled = true;
		triggered = false;
		this.data = default(T);
	}

	// Wait이나 WaitOr을 쓰면 자동으로 호출됩니다.
	public void End()
	{
		enabled = false;
	}

	public void Revert(){
		if(enabled)
			triggered = false;
	}
}

public static class BattleData{
	public class Triggers{
		public EventTrigger rightClicked = new EventTrigger();
		public EventTrigger tileSelected = new EventTrigger();
		public EventTrigger cheatKeyPressed = new EventTrigger();
		public EventTrigger tileLongSelected = new EventTrigger();
		public EventTrigger directionSelected = new EventTrigger();
		public EventTrigger directionLongSelected = new EventTrigger();
		public EventTrigger skillSelected = new EventTrigger();
		public EventTrigger<ActionCommand> actionCommand = new EventTrigger<ActionCommand>();
		public EventTrigger resetUnitInput = new EventTrigger();
		public EventTrigger finishUnitInput = new EventTrigger();
        public EventTrigger castCheckYesClicked = new EventTrigger();
        public EventTrigger castCheckNoClicked = new EventTrigger();
	}

	public static TutorialState TutoState;
	public static Candidate unitToGenerate;
	public static bool rightClickLock;
	public static bool longClickLock;
	public static bool shortClickLock;
	public static Unit activateDetailInfoUnit = null;
	public static bool deactivateDetailInfoLock = false;
	public static bool detailInfoLock;

	public static Triggers triggers = new Triggers();
	public static CurrentState currentState = CurrentState.None;

	public static ActiveSkill selectedSkill;
	public static int rewardPoint;
	public static bool isWaitingUserInput;
	public static bool IsUserTurn{get { return turnUnit == null || turnUnit.IsPC; }}

	public static Unit unitInSelectedUnitViewer;
    public static Tile tileInTileViewer;
    public static Aspect aspect = Aspect.North;
    public static List<LogDisplay> logDisplayList = new List<LogDisplay>();

    public static Dictionary<Element, UnitStatusEffectInfo> tileBuffInfos = new Dictionary<Element, UnitStatusEffectInfo>();
	public static UnitStatusEffectInfo collectingStatusEffectInfo = null;

	public class Move {
		public Vector2Int selectedTilePosition = Vector2Int.zero;
	}

	public static Move move = new Move();

	public static Unit turnUnit; // 현재 턴의 유닛
    public static Collectable NearestCollectable = null;  // turnUnit으로부터 가장 가까운 Collectable(수집 가능한 오브젝트)

	public static Dictionary<Tile, List<Unit>> guardDict = new Dictionary<Tile, List<Unit>>();

	public static int currentPhase;
    public static float startTime;

	public static APAction previewAPAction;

	public static Faction selectedFaction = Faction.Pintos; // used only during pintos vs haskell stage

	// 중요 - BattleData 클래스 내 모든 변수는 static이라서 Initialize 함수 내에서 초기화를 해야 하므로
	// 변수 하나 추가할 때마다 반드시 여기에 초기화하는 코드도 추가할 것
	// 또 전투 시작할 때 반드시 Initialize() 함수를 불러야 한다(현재는 BattleManager 인스턴스의 Awake()시 호출함)
	public static void Initialize(){
		TutoState = TutorialState.None;
		unitToGenerate = null;
		rightClickLock = false;
		longClickLock = false;
		shortClickLock = false;
		detailInfoLock = false;

		triggers = new Triggers();
		currentState = CurrentState.None;

		rewardPoint = 0;
		isWaitingUserInput = false;
		unitInSelectedUnitViewer = null;
		tileInTileViewer = null;
		aspect = Aspect.North;
        logDisplayList = new List<LogDisplay>();

		move = new Move();

		turnUnit = null;

		currentPhase = 0;
        startTime = Time.time;

		previewAPAction = null;
	}

	public static void SetTurnUnit(){
		var units = UnitManager.Instance.unitsActThisPhase;
		units.RemoveAll(unit => unit == null);
		
		if(units.Count == 0) return;
		units.Sort(SortHelper.Chain(new List<Comparison<Unit>>{
			SortHelper.CompareBy<Unit>(unit => unit.GetCurrentActivityPoint()),
			SortHelper.CompareBy<Unit>(unit => unit.GetStat(Stat.Agility)),
			SortHelper.CompareBy<Unit>(unit => unit.gameObject.GetInstanceID())
		}, reverse:true));

		turnUnit = units[0];
	}

	public static Tile SelectedTile{
		get { return TileManager.Instance.GetTile(move.selectedTilePosition); }
	}
	
	public static List<Unit> GetObjectUnitsList(){
		return UnitManager.GetAllUnits ().FindAll (unit => unit.IsObject == true);
	}
}
