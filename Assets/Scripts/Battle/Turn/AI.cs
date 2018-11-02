using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using GameData;
using UtilityMethods;

namespace Battle.Turn{
	public class AIPlan{
		public TileWithPath path;
		public Casting casting;

		public AIPlan(TileWithPath path, Casting casting){
			this.path = path;
			this.casting = casting;
		}
	}
	
	public class AIUtil{
		//AI 입장에서 다른 유닛이 적으로 인식되는지에 대한 함수이기 때문에 AI 부분에서만 써야 한다(은신 스킬 때문)
		//왼쪽 unit1에는 무조건 AI 유닛이 들어가야 하고(unit1의 입장에서 unit2를 보는 것임) unit2엔 아무 유닛이나 들어감
		public static bool IsSecondUnitEnemyToFirstUnit(Unit unit1,Unit unit2){
			//unit2가 '은신' 효과를 갖고 있으면 AI인 unit1에게 적으로 인식되지 않는다
			return unit2.IsEnemyTo(unit1) && !unit2.HasStatusEffect(StatusEffectType.Stealth);
		}
		static Dictionary<Vector2Int, TileWithPath> GetMovablePathsFromAllPaths(Dictionary<Vector2Int, TileWithPath> allPaths, int maxAP){
			return allPaths.Where(kv => (int) kv.Value.requireActivityPoint <= maxAP).ToDictionary(kv => kv.Key, kv => kv.Value);
		}
		static Dictionary<Vector2Int, TileWithPath> GetGoalPathsFromAllPaths(Dictionary<Vector2Int, TileWithPath> allPaths, List<Vector2Int> goalArea){
			return allPaths.Where(kv => goalArea.Contains(kv.Value.dest.Location)).ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		public static List<AIPlan> availablPlans;
		public static IEnumerator CalcAvailablePlans(Unit actor, bool nextTurn = false){
			var skillDict = new Dictionary<ActiveSkill, int>();
			foreach (var skill in actor.GetActiveSkillList().FindAll(actor.IsThisSkillUsable))
				skillDict.Add(skill, actor.GetActualRequireSkillAP(skill));

			var result = new List<AIPlan>();
			var maxAPUse = actor.GetCurrentActivityPoint()
			               + (nextTurn ? actor.GetStat(Stat.Agility) : 0);
			var paths = PathFinder.CalculatePaths(actor, nextTurn, maxAPUse).Values.ToList();
			
			float lastTime = Time.realtimeSinceStartup;
			float calcstartTime = lastTime;
			foreach (var item in skillDict){
				var castingDict = new Dictionary<Tile, AIPlan>();
				foreach (var path in paths){
					/*if (Time.realtimeSinceStartup - lastTime > Setting.AILagMaxDuration) {
						lastTime = Time.realtimeSinceStartup;
						yield return null;
					}*/
					var movingAP = path.RequireActivityPoint;
					if(item.Value + movingAP > maxAPUse)
						continue;
					foreach (var casting in item.Key.GetPossibleCastings(actor, path.dest)){
						if(item.Key.GetRangeType() != RangeType.Point)
							result.Add(new AIPlan(path, casting));
						else{
							if(!castingDict.ContainsKey(casting.Location.TargetTile))
								castingDict.Add(casting.Location.TargetTile, new AIPlan(path, casting));
							else if (movingAP < castingDict[casting.Location.TargetTile].path.RequireActivityPoint)
								castingDict[casting.Location.TargetTile] = new AIPlan(path, casting);
						}
					}
				}
				result.AddRange(castingDict.Values.ToList());
			}

			//var calcTime = (Time.realtimeSinceStartup - calcstartTime);
			//Debug.Log("계산 시간: " + calcTime + " / 계획 후보 수: " + result.Count);
			
			availablPlans = result;
			yield break;
		}

		//미리 계산된 availablePlans 중 targetTile을 공격하는 경우가 있는지 확인
		public static bool CanAttack(Tile target) {return availablPlans.Any(plan => plan.casting.RealRange.Contains(target));}
		
		// 1턴 내에 이동 후 공격 불가하거나 그럴 만한 가치가 없을 때 가장 가치있는 적을 향해 움직인다
		// 가치에는 거리도 적용되므로 너무 멀면 그쪽으로 가진 않는다
		public static Unit CalculateBestApproachWorthUnit(Unit actor){ 
			//공격 기술이 있을 경우 가장 가치가 높은 적을 향함
			if (!actor.HasOnlyFriendlySkill())
				return UnitManager.GetAllUnits().OrderBy(unit => actor.GetAI().GetTargetValue(unit)).ThenBy(unit => Calculate.Distance(actor, unit)).First();
			//힐러인데 모든 아군이 풀피라면 가장 가까운 딜러를 따라다닌다. 딜러가 없다면 가만히 있는다.
			var allyDealers = UnitManager.GetAllUnits().FindAll(_unit => _unit.IsAllyTo(actor) && !_unit.HasOnlyFriendlySkill());
			return (allyDealers.Count != 0 && allyDealers.All(_unit => _unit.GetMaxHealth() == _unit.GetHP)) ? 
				allyDealers.OrderBy(_unit => Calculate.Distance(actor, _unit)).First() : actor;
		}
		
		public static IEnumerator CalculateBestEscapeRoute(Unit unit, List<Vector2Int> goalArea){
			Dictionary<Vector2Int, TileWithPath> allPaths = PathFinder.CalculatePaths(unit, true, int.MaxValue);
			AI.movablePathDict = GetMovablePathsFromAllPaths(allPaths, unit.GetCurrentActivityPoint());
			Dictionary<Vector2Int, TileWithPath> allGoalPaths = GetGoalPathsFromAllPaths(allPaths, goalArea);
			AI.pathToGo = GetMinRequireAPTileWithPath (allGoalPaths);
			yield break;
		}
		public static Tile GetTileToGoThisTurn(Dictionary<Vector2Int, TileWithPath> allPaths, TileWithPath destTileWithPath, int currentAP){
			Tile tileToGoThisTurn = null;
			foreach (Tile tile in destTileWithPath.fullPath){
				if(!allPaths.ContainsKey(tile.Location)) continue;
				if(allPaths[tile.Location].RequireActivityPoint > currentAP) break;
				tileToGoThisTurn = tile;
			}
			return tileToGoThisTurn;
		}

		static TileWithPath GetMinRequireAPTileWithPath(Dictionary<Vector2Int, TileWithPath> movableTilesWithPath){
			return movableTilesWithPath.Count == 0 ? null : movableTilesWithPath.Values.ToList().OrderBy(path => path.RequireActivityPoint).First();
		}
		
		public static float RangeDistanceOfAttack(ActiveSkill skill){
			return skill.secondRange.ReachDistance +
			       (skill.GetRangeType() == RangeType.Auto ? 0 : skill.firstRange.ReachDistance);
		}
	}

	public class AI : MonoBehaviour{
		public Unit unit;
		string CodeName{get { return unit.CodeName; }}
		public AIData _AIData;
		public static ActiveSkill selectedSkill; // 미리 선택한 스킬 기억용
		public static TileWithPath pathToGo;
		public static Casting bestCasting;
		public static Dictionary<Vector2Int, TileWithPath> movablePathDict = new Dictionary<Vector2Int, TileWithPath>();
		public void Initialize(Unit unit, AIData _AIData){
			this.unit = unit;
			this._AIData = _AIData;
		}
		public string state;
		public enum MoveType{ThisTurn, NextTurn, Least}
		public IEnumerator UnitTurn(){
			BattleData.isWaitingUserInput = false;
			var BM = BattleManager.Instance;
			if (!UnitManager.GetAllUnits().Contains(unit)) yield break;
			if (TutorialController.Instance != null)
				TutorialController.Instance.RemoveSpriteAndMark ();
			
			if (!_AIData.IsActive()){
				_AIData.SetActive(BattleData.currentPhase == _AIData.info.actPhase - 1);	
				yield break;
			}

			BM.ShowUnitTurnStart (unit);

			state = "TurnStart";
			if(unit.HasStatusEffect(StatusEffectType.Faint))
				yield return unit.ShowFaint();
			else if(BattleData.TutoState == TutorialState.Active && !unit.IsObject)
				while (state != "EndTurn")
					yield return ActByScenario (TutorialController.Instance.GetNextAIScenario ());	
			else{
				SetFirstState();
				yield return FSM();
			}
			selectedSkill = null;
		}

		public void CheckVigilArea(){ //Activate되면 true, 아니면 false를 return.
			AIInfo info = _AIData.info;
			if(info.vigilAreas.Count > 0 && info.CheckIfThereIsUnitInVigilAreas(unit))
				_AIData.SetActive(true);
		}

		public virtual void SetFirstState(){state = "InitialState";}
		IEnumerator FSM(){
			while(true){
				if(Utility.CheckShowMotion())
					yield return BattleManager.Instance.cameraMover.Slide(unit.transform.position, Setting.basicCameraMoveDuration);
				
				if (BattleData.currentState == CurrentState.Destroyed || state == "EndTurn") yield break;	
				Debug.Log(unit.GetNameKor() + "의 AI state: " + state);
				yield return StartCoroutine (state);
			}
		}
		public virtual IEnumerator InitialState(){
			if (!unit.HasAnyWayToMove()) {
				state = "CastingLoop";
				yield break;
			}
			if (_AIData.goalArea.Count > 0 && !SpecialTargets.ContainsValue(true)){
				state = "Approach";
				yield break;
			} 
			
			yield return CalcBestPlan();
			if (bestPlan != null){
				//Debug.Log("이번 턴에 기술 시전 계획 있음");
				yield return MoveWithDestroyRoutine(bestPlan.path, MoveType.ThisTurn);
				if(unit.IsSkillUsePossibleState())
					yield return UseSkill(bestPlan.casting);
			}else{
				yield return CalcBestPlan(true);
				if (bestPlan != null){
					//Debug.Log("다음 턴 계획에 따라 행동");
					yield return MoveWithDestroyRoutine (bestPlan.path, MoveType.NextTurn);
					state = "EndTurn";
				}
				else
				{
					//Debug.Log("어쨌든 접근");
					state = "Approach";
				}
			}
		}
		public Dictionary<Unit, bool> SpecialTargets{get{
			var result = new Dictionary<Unit, bool>(); //true는 공격 대상, false는 수호 대상
			var UM = UnitManager.Instance;
		
			var taunts = unit.statusEffectList.FindAll(se => se.IsTypeOf(StatusEffectType.Taunt));
			foreach (var taunt in taunts)
				result.Add(taunt.GetCaster(), true);

			Unit unitToAdd;
			if (unit.CodeName == "spyLeaderSword"){
				unitToAdd = UM.GetAnUnit("jailLever");
				if (unitToAdd != null)
					result.Add(unitToAdd, true);
			}if(unit.CodeName == "stel"){
				unitToAdd = UM.GetAnUnit("darkenir");
				if (unitToAdd != null)
					result.Add(unitToAdd, false);
			}if(unit.CodeName == "schmidt"){
				var targets = UnitManager.GetAllUnits().FindAll(unit => unit.CodeName == "curi" || unit.IsObject);
				foreach (var target in targets)
					result.Add(target, false);
			}
		
			return result;
		}}
		
		public IEnumerator MoveWithDestroyRoutine(TileWithPath path, MoveType type){
			Debug.Log(path.dest.Location + "로 이동 시도(" + type + ") / " + (path.skillForDestroy == null ? "No Destruction" : path.skillForDestroy.Name));
			movablePathDict = PathFinder.CalculatePaths(unit, false);
			Tile prevTile = unit.TileUnderUnit;
			var targettedTiles = ChainList.TargetedTiles;
			var toEscapeTargetArea = targettedTiles.Contains(unit.TileUnderUnit) && path.fullPath.Any(tile => !targettedTiles.Contains(tile));
			
			for(int i = 0; i < path.fullPath.Count; i++){
				var tile = path.fullPath[i];
				//Debug.Log(tile.GetPos() + "를 확인");
				if (tile == unit.TileUnderUnit) continue;
					
				if (!movablePathDict.ContainsKey(tile.Location)){
					if (tile.IsUnitOnTile()){
						if(tile.GetUnitOnTile().IsAllyTo(unit))
							continue;
						if (path.skillForDestroy == null)
							yield break;
						
						yield return MoveToThePosition (prevTile.Location);
						yield return DestroyObstacle (path.skillForDestroy, tile);
					}
				}else if (movablePathDict.ContainsKey(prevTile.Location)
				        && (!toEscapeTargetArea || !targettedTiles.Contains(prevTile))
						&& unit.GetCurrentActivityPoint() - movablePathDict[prevTile.Location].RequireActivityPoint <= unit.ApDeletePoint
						&& (type == MoveType.NextTurn && i * 2 > path.fullPath.Count - 1 || type == MoveType.Least))
					break;
				
				prevTile = tile;
			}
			yield return MoveToThePosition (prevTile.Location);
		}
		public IEnumerator DestroyObstacle(ActiveSkill skill, Tile obstacleTile){
			if (obstacleTile.GetUnitOnTile().IsAllyTo(unit)){
				Debug.Log("자기편을 공격 시도");
				yield break;
			}
			Tile currTile = unit.TileUnderUnit;
			Vector2 currPos = currTile.Location;
			Vector2 obstaclePos = obstacleTile.Location;
			SkillLocation location;
			
			if (skill.GetRangeType () == RangeType.Route) {
				location = new SkillLocation (currTile, obstacleTile, _Convert.Vec2ToDir(obstaclePos - currPos));
			} else if (skill.GetRangeType () == RangeType.Point) {
				location = new SkillLocation (currTile, obstacleTile, Utility.GetDirectionToTarget (currPos, obstaclePos));
			} else {
				location = new SkillLocation (currTile, currTile, _Convert.Vec2ToDir(obstaclePos - currPos));
			}
			Casting casting = new Casting (unit, skill, location);
			while (obstacleTile.IsUnitOnTile ()) {
				if (!unit.IsThisSkillUsable (skill)) {
					Debug.Log ("(" + obstaclePos.x + ", " + obstaclePos.y + ")의 장애물을 파괴하려고 계획했으나 더는 공격 불가해 중단");
					dontRestart = true;
					break;
				}
				Debug.Log ("(" + obstaclePos.x + ", " + obstaclePos.y + ")의 장애물을 파괴 시도");
				yield return UseSkill (casting);
			}
		}
		public IEnumerator MoveToThePosition(Vector2Int destPos){
			//Debug.Log(destPos + "로 이동 시도");
			movablePathDict = PathFinder.CalculatePaths(unit, false);
			if (!movablePathDict.ContainsKey(destPos))
				destPos = movablePathDict.OrderBy(kv => Calculate.Distance(destPos, kv.Key) * 1000 + kv.Value.RequireActivityPoint).First().Key;			
			if(movablePathDict[destPos].fullPath.Count > 1)
				yield return Move (destPos);
		}

		// AI 입장에서 보았을 때 target이 얼마나 가치를 갖는지 반환하는 함수
		public virtual float GetTargetValue(Unit target){
			var specTargets = SpecialTargets;
			if(specTargets.ContainsKey(target))
				return specTargets[target] ? -10000f : 10000f;
			if(specTargets.ContainsValue(true))
				return 0;
		
			float sideFactor; //적대적이면 음수, 우호적이면 양수!
			if(CodeName == "monk")
				sideFactor = target.GetSide() == unit.GetSide() ? 1 : -1;
			else if (CodeName == "fireSpirit" && target.CodeName == "citizen")
				sideFactor = -1;
			else if (target.GetSide() == Side.Neutral || _String.GeneralName(target.CodeName) == "child")
				return 0;
			else if (target.IsAllyTo(unit))
				sideFactor = 1;
			else if (target.IsSeenAsEnemyToThisAIUnit(unit))
				sideFactor = -1;
			else
				return 0;
			
			return sideFactor * target.GeneralTargetValue;
		}
		
		public static AIPlan bestPlan;
		public virtual IEnumerator CalcBestPlan(bool nextTurn = false){
			bestPlan = null;
			if (!unit.HasAnySkillToCast() && !nextTurn)
				yield break;
			
			float bestRecord = 0;
			yield return AIUtil.CalcAvailablePlans(BattleData.turnUnit, nextTurn);
			
			float lastTime = Time.realtimeSinceStartup;
			foreach (var plan in AIUtil.availablPlans){
				if (Time.realtimeSinceStartup - lastTime > Setting.AILagMaxDuration) {
					lastTime = Time.realtimeSinceStartup;
					yield return null;
				}

				var cost = plan.path.RequireActivityPoint + BattleData.turnUnit.GetActualRequireSkillAP(plan.casting.Skill);
				var castingValue = plan.casting.GetReward() / Math.Max(cost, 1);
				
				//Debug.Log(plan.casting.Skill.Name + "의 castingValue: " + castingValue);
				if(bestRecord >= castingValue) continue;
				bestRecord = castingValue;
				bestPlan = plan;
			}
			Debug.Log(AIUtil.availablPlans.Count + "개 계획의 보상 계산 및 비교 시간: " + (Time.realtimeSinceStartup - lastTime));
		}

		bool dontRestart;
		public virtual IEnumerator Approach() {
			if(!unit.HasAnyWayToMove()){
				state = "CastingLoop";
				yield break;
			}

			var usablePathDict = PathFinder.CalculatePaths(unit, true);
			if (usablePathDict.Count == 0){
				state = "CastingLoop";
				yield break;
			}

			state = "EndTurn";
			usablePathDict = PathFinder.CalculatePaths(unit, true, int.MaxValue);
			if(_AIData.goalArea.Count == 0 || SpecialTargets.ContainsValue(true)){
				var destUnit = AIUtil.CalculateBestApproachWorthUnit(unit);
				Debug.Log(unit.CodeName + unit.TileUnderUnit.Location + "은 " + destUnit.CodeName + destUnit.TileUnderUnit.Location + "을 향해 접근");

				TileWithPath closestPath = null;
				foreach (var kv in usablePathDict)					
					if (closestPath == null || Calculate.DistanceToUnit(kv.Value.dest.Location, destUnit) < Calculate.DistanceToUnit(closestPath.dest.Location, destUnit))
						closestPath = kv.Value;
				
				yield return MoveWithDestroyRoutine(closestPath, MoveType.Least);
			}else{
				yield return AIUtil.CalculateBestEscapeRoute (unit, _AIData.goalArea);
				yield return MoveWithDestroyRoutine(pathToGo, pathToGo.RequireActivityPoint <= unit.GetCurrentActivityPoint() ? MoveType.ThisTurn : MoveType.Least);
			}

			if (unit.ApOverflow <= 0 || dontRestart) yield break;
			state = "InitialState";
			dontRestart = true;
		}
		public virtual IEnumerator CastingLoop() {
			if(!unit.HasAnySkillToCast()) {
				state = "EndTurn";
				yield break;
			}
			if(unit.IsObject && unit.CodeName == "controller"){
				yield return ObjectUnitBehaviour.AnObjectUnitBehave(unit);
				state = "EndTurn";
				yield break;
			}
			
			unit.CalculateBestCasting();
			if (bestCasting == null){
				state = unit.HasAnyWayToMove() ? "InitialState" : "EndTurn";
				yield break;
			}
			yield return UseSkill(bestCasting);
			state = "InitialState";
		}
		public IEnumerator NeverMoveCastingLoop() {
			if(!unit.HasAnySkillToCast()) {
				state = "EndTurn";
				yield break;
			}
			unit.CalculateBestCasting();
			if (bestCasting == null) {
				state = "EndTurn";
				yield break;
			}
			state = "NeverMoveCastingLoop";
			yield return UseSkill(bestCasting);
		}
		public IEnumerator ChaseSomeone(string targetName, string afterFailState, string afterSuccessState){
			var target = UnitManager.Instance.GetAnUnit (targetName);
			if (target == null) {
				state = afterFailState;
				yield break;
			}
			
			Dictionary<Vector2Int, TileWithPath> allPaths = PathFinder.CalculatePaths(unit, true, int.MaxValue);
			Vector2Int nearestPos = new Vector2Int (-999, -999);
			int minDistance = 500;
			int minAPToMove = int.MaxValue;
			foreach (var kv in allPaths) {
				Vector2Int pos = kv.Key;
				int distance = Calculate.Distance(pos, target.Pos);
				int APToMove = allPaths [pos].RequireActivityPoint;
				if (distance < minDistance || (distance == minDistance && APToMove < minAPToMove)) {
					minDistance = distance;
					nearestPos = pos;
					minAPToMove = APToMove;
				}
			}
			if (nearestPos == new Vector2 (-999, -999)) {
				state = afterFailState;
				yield break;
			}
			int currentAP = unit.GetCurrentActivityPoint ();
			TileWithPath nearestTileWithPath = allPaths [nearestPos];
			var destTile = AIUtil.GetTileToGoThisTurn (allPaths, nearestTileWithPath, currentAP) ?? unit.TileUnderUnit;
			yield return MoveToThePosition (destTile.Location);
			state = afterSuccessState;
		}
		
		public void PaintMovableTiles(){
			List<Tile> movableTiles = new List<Tile>();
			foreach (KeyValuePair<Vector2Int, TileWithPath> movableTileWithPath in movablePathDict) {
				movableTiles.Add (movableTileWithPath.Value.dest);
			}
			LogManager.Instance.Record (new PaintTilesLog (movableTiles, TileManager.Instance.TileColorMaterialForMove));
		}
		public IEnumerator Move(Vector2Int destPos){
			Debug.Log("Move: " + destPos);
			BattleManager.Instance.ReadyForUnitAction();
			if (Utility.CheckShowMotion()){
				PaintMovableTiles();
				LogManager.Instance.Record(new WaitForSecondsLog(Configuration.NPCBehaviourDuration));
				LogManager.Instance.Record(new DepaintTilesLog(TileManager.Instance.TileColorMaterialForMove));
			}
			BattleManager.Instance.MoveToTile (destPos, movablePathDict);
			yield return LogManager.Instance.ExecuteLastEventLogAndConsequences("AI Move");
		}

		public IEnumerator UseSkill(Casting casting){
			BattleManager.Instance.ReadyForUnitAction();

			bool chain = false;
			if (casting.Skill.IsOffensive() && VolatileData.OpenCheck(Setting.chainOpenStage) && VolatileData.difficulty != Difficulty.Intro
			    && SkillAndChainStates.GetCastingResultPreview(ChainList.GetAllChainTriggered(casting), false).All(kv => GetTargetValue(kv.Key) >= 0 || kv.Value.unitHp <= kv.Key.RetreatHP)
			    && (casting.Skill.GetRangeType() != RangeType.Route || Calculate.Distance(casting.Location.TargetTile, unit) <= 1)){
				for (int i = 0; i < UnitManager.Instance.unitsActThisPhase.Count; i++){
					var actor = UnitManager.Instance.unitsActThisPhase[i];
					if (i == 0 || actor.GetSide() == Side.Neutral || !actor.CanAffectChainPlan(casting.GetTargets())) continue;
					
					yield return AIUtil.CalcAvailablePlans(actor);//여기서 Plan을 미리 계산
					if (actor.IsAllyTo(unit)){
						foreach (var target in casting.GetTargets()){
							if (ChainList.GetUnitsTargetingThisTile(target.TileUnderUnit).Count > 0 && VolatileData.difficulty != Difficulty.Legend
							    || !AIUtil.CanAttack(target.TileUnderUnit)) continue;
							chain = true;
							break;
						}
					}else if(actor.IsEnemyTo(unit) && AIUtil.CanAttack(unit.TileUnderUnit))
						break;
					
					if(chain)
						break;
				}
			}
			
			SkillAndChainStates.HideAfterImages();
			yield return casting.Skill.AIUseSkill (casting, chain);
			yield return LogManager.Instance.ExecuteLastEventLogAndConsequences("AI UseSkill");
			SkillAndChainStates.HideCastingPreview();
			if(chain)
				state = "EndTurn";
		}

		public bool IsActive{get { return _AIData.isActive; }}
		public IEnumerator EndTurn(){
			BattleManager.Instance.ReadyForUnitAction();
			state = "EndTurn";
			dontRestart = false;
			yield break;
		}
		public IEnumerator ActByScenario(AIScenario scenario){
			if (scenario.act == AIScenario.ScenarioAct.UseSkill) {
				SkillLocation location = new SkillLocation (unit.Pos, unit.Pos + scenario.relativeTargetPos, scenario.direction);
				Casting casting = new Casting (unit, unit.GetActiveSkillList () [scenario.skillIndex], location);
				yield return StartCoroutine (scenario.functionName, casting);
			} else if (scenario.act==AIScenario.ScenarioAct.Move) {
				movablePathDict = PathFinder.CalculatePaths(unit, false);
				Vector2 targetPos = unit.Pos + scenario.relativeTargetPos;
				yield return StartCoroutine (scenario.functionName, targetPos);
			} else
				yield return StartCoroutine (scenario.functionName);
		}
	}
}
