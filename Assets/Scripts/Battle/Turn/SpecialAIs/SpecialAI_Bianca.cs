using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enums;
using UtilityMethods;

namespace Battle.Turn{
	public class SpecialAI_Bianca : AI{
		public override void SetFirstState() {
			state = "SetTrapToProtectTargets";
		}

		public IEnumerator SetTrapToProtectTargets(){
			TileManager TM = TileManager.Instance;
			Unit VIP; //지키려는 유닛

			//누구를 지켜야 하는지 연산(공격기가 있는 동료 중 가장 사거리가 긴 유닛 - 보통은 그레네브)
			var armorTriggers = UnitManager.GetAllUnits().FindAll(target => target.CodeName.Contains("armorTrigger"));
			if (armorTriggers.Count > 0)
				VIP = armorTriggers.OrderBy(armor => Calculate.Distance(armor, unit)).First();
			else if (unit.GetAllies.Count <= 2){
				state = "EndTurn";
				yield break;
			}else
				VIP = unit.GetAllies.OrderByDescending(ally => ally.AttackableDistance).First();

			//누구로부터 지켜야 하는지 연산(공격 기술이 있고 & VIP보다 사거리가 짧은 적들 중 가장 가까운 유닛)
			//단, 이미 이동하지 않고도 VIP를 칠 수 있는 적은 막는 의미가 없으므로 제외
			var enemiesToBlock = unit.GetEnemies.FindAll(enemy =>{
				var attackDist = enemy.AttackableDistance;
				return attackDist > 0 && attackDist < VIP.AttackableDistance && Calculate.Distance(enemy, VIP) > attackDist;
			});
			if (enemiesToBlock.Count == 0){
				state = "EndTurn";
				yield break;
			}
			var threatener = enemiesToBlock.OrderBy(enemy => Calculate.Distance(enemy, VIP)).First();
			
			//적이 접근할 경로 계산
			var enemyPathDict = PathFinder.CalculatePaths(threatener, true, int.MaxValue);
			var validPaths = enemyPathDict.Where(kv => Calculate.Distance(kv.Value.dest, VIP) <= threatener.AttackableDistance);
			Debug.Log(threatener.CodeName + "(" + threatener.AttackableDistance + ")의 경로 수: " + validPaths.Count() + " / " + enemyPathDict.Count);
			var optimizedPath = enemyPathDict.Where(kv => Calculate.Distance(kv.Value.dest, VIP) <= threatener.AttackableDistance)
				.OrderBy(kv => kv.Value.RequireActivityPoint).First();
			
			//덫을 깔아야 할 위치 목록: 적의 접근 경로 중에서 함정 또는 유닛이 있는 타일(=이번 턴에 함정을 깔 수 없음)을 빼고 산출
			//나중에 AI의 경로 계산이 덫을 우회하도록 만들어야 함
			var posToSetTrap = optimizedPath.Value.fullPath.FindAll(tile => !tile.HasTrap() && !tile.IsUnitOnTile()).ConvertAll(tile => tile.Pos);
			
			var posToStepOn = new List<Vector2Int> (); // 덫 까는 동안 밟고 있을 위치 후보 - 덫 깔 위치에 인접한 위치들
			foreach (Vector2Int pos in posToSetTrap){
				var targetTile = TM.GetTile(pos);
				foreach (var nearTile in TileManager.Instance.GetTilesNearby(pos)){
					if (nearTile != null && !(nearTile.IsUnitOnTile () && nearTile.GetUnitOnTile() != unit)
					                     && Calculate.Distance(targetTile, nearTile, 1) <= 1){ // 타일이 존재하고 다른 유닛이 없는가 
						posToStepOn.Add (nearTile.Pos);
					}
				}
			}

			Dictionary<Vector2Int, TileWithPath> allPaths = PathFinder.CalculatePaths(unit, true, int.MaxValue);
			var nearestPos = new Vector2Int (-999, -999);
			int minAPToMove = int.MaxValue;
			foreach (Vector2Int pos in posToStepOn){
				if (!allPaths.ContainsKey(pos) || allPaths[pos].RequireActivityPoint >= minAPToMove) continue;
				nearestPos = pos;
				minAPToMove = allPaths [pos].RequireActivityPoint;
			}

			if (nearestPos == new Vector2 (-999, -999)) {
				state = "EndTurn";
				yield break;
			}

			int currentAP = unit.GetCurrentActivityPoint ();

			TileWithPath nearestTileWithPath = allPaths [nearestPos];
			Vector2Int destPos = (AIUtil.GetTileToGoThisTurn (allPaths, nearestTileWithPath, currentAP) ?? unit.TileUnderUnit).Location;

			yield return MoveToThePosition (destPos);

			ActiveSkill trapSkill = unit.GetActiveSkillList ().Find (_skill => _skill.GetName() == "잘근잘근 덫");
			if (!unit.IsThisSkillUsable (trapSkill)) {
				state = "EndTurn";
				yield break;
			}

			Direction trapDir = Direction.None;
			var trapPos = new Vector2Int(-999, -999);

			foreach (Direction direction in EnumUtil.directions) {
				Vector2Int nearPos = destPos + Utility.DirToV2I (direction);
				if (!posToSetTrap.Contains(nearPos)) continue;
				
				trapPos = nearPos;
				trapDir = direction;
				break;
			}

			if (trapDir == Direction.None) {
				state = "EndTurn";
				yield break;
			}

			Casting trapCasting = new Casting (unit, trapSkill, new SkillLocation (unit.TileUnderUnit, TM.GetTile (trapPos), trapDir));
			yield return UseSkill(trapCasting);
			state = "SetTrapToProtectTargets";
		}
	}
}
