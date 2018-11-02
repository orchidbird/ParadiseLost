using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Battle.Turn;
using System.Linq;
using UtilityMethods;

public static class PathFinder {
	public static class dataStorage{ //매번 계산하면 시간이 걸리는 것들을 한 번 해서 저장하고 계속 가져다 쓰는 식.
		public static Dictionary<Unit, int> DestroyCostDict = new Dictionary<Unit, int>();
		public static Unit registeredActor;
		public static bool easeMoveHeightConstraint;
		public static bool moveCostChange;
		public static int moveCostAcc;
		public static int plantMoveCostChange;

		public static void CheckAndUpdate(Unit actor){
			if(registeredActor == actor) return;
			
			DestroyCostDict.Clear();
			registeredActor = actor;
			easeMoveHeightConstraint = actor.HasStatusEffect(StatusEffectType.EaseMoveHeightConstraint);
			moveCostChange = actor.HasStatusEffect(StatusEffectType.RequireMoveAPChange);
			moveCostAcc = Setting.moveCostAcc - (actor.HasStatusEffect(StatusEffectType.MoveCostAccDecrease) ? 1 : 0);
			plantMoveCostChange = actor.HasSkillOfKorName("숲의 후예") ? -2 : 0;
		}
	}
    
	public static Dictionary<Vector2Int, TileWithPath> CalculatePaths(Unit mover, bool forPlan, int maxAPUse = 0, Tile startTile = null){
		if (maxAPUse == 0)
			maxAPUse = mover.GetCurrentActivityPoint();
		Dictionary<Vector2, Tile> tiles = TileManager.Instance.GetAllTiles();
		Vector2Int startPos = startTile == null ? mover.Pos : startTile.Pos;

		var tileQueue = new Queue<Vector2Int>();
		var tilesWithPath = new Dictionary<Vector2Int, TileWithPath>();
		TileWithPath startPath = new TileWithPath(tiles[startPos], mover);
		tilesWithPath.Add(startPos, startPath);

		if (!mover.IsMovePossibleState ())
			return tilesWithPath;

		// Queue에 넣음
		tileQueue.Enqueue(startPos);

		while (tileQueue.Count > 0){
			// Queue에 있는 모든 원소에 대해 전후좌우에 있는 타일을 탐색(탐색 과정에서 Queue 추가)
			Vector2Int newPosition = tileQueue.Dequeue();
			foreach (Direction direction in EnumUtil.directions) 
				SearchNearbyTile (tilesWithPath, tileQueue, mover, newPosition, newPosition + Utility.DirToV2I(direction), maxAPUse, forPlan);
		}
		// queue가 비었으면 loop를 탈출.
		return tilesWithPath.Where(kv => !kv.Value.dest.IsUnitOnTile() || kv.Value.dest.GetUnitOnTile() == mover).ToDictionary(pair => pair.Key, pair => pair.Value);
		//return tilesWithPath;
	}
	
	static void SearchNearbyTile(Dictionary<Vector2Int, TileWithPath> tilesWithPath,
		Queue<Vector2Int> tileQueue, Unit actor, Vector2Int tilePos, Vector2Int nearbyPos, int maxAPUse, bool forPlan){
		
		dataStorage.CheckAndUpdate(actor);
		// if) 타일이 존재하지 않거나, 다음타일과의 높이차가 2 이상이거나, ZOC에 걸리거나, 타일까지 드는 ap가 remain ap보다 큰 경우 고려하지 않음.
		var tiles = TileManager.Instance.GetAllTiles();
		if (!tiles.ContainsKey(nearbyPos)) return;
		var prevTile = tiles[tilePos];
		var nearbyTile = tiles[nearbyPos];
		int deltaHeight = Mathf.Abs(tiles[tilePos].GetHeight() - nearbyTile.GetHeight());
		if (deltaHeight >= 2 && !dataStorage.easeMoveHeightConstraint || deltaHeight >= 4) return;
		foreach (var effect in nearbyTile.GetStatusEffectsOfType (StatusEffectType.ZOC))
			if(effect.GetCaster() != null && actor.IsEnemyTo(effect.GetCaster()))
				return;
		TileWithPath prevTileWithPath = tilesWithPath[tilePos];
		TileWithPath nearbyTileWithPath = new TileWithPath(nearbyTile, actor, prevTileWithPath);
		if (nearbyTileWithPath.RequireActivityPoint > maxAPUse) return;
		
		int requireAP = nearbyTileWithPath.RequireActivityPoint;
		var Obstacle = nearbyTile.GetUnitOnTile();
		//실제 이동 가능 영역을 구하는 것이고 동료가 아닌 유닛이 경로에 있을 경우 탐색 종료
		if (Obstacle != null && (!Obstacle.IsAllyTo(actor) || Obstacle.IsObject)){
			if (!forPlan || prevTile.IsUnitOnTile()) return;
			
			var minDestroyCost = int.MaxValue;

			if (!dataStorage.DestroyCostDict.ContainsKey(Obstacle)){
				// 한칸 바로 앞을 공격할 수 없는 AI 스킬이 나오면 수정해야 함
				foreach (var skill in actor.GetActiveSkillList()) {
					if (skill.GetSkillApplyType() != SkillApplyType.DamageHealth) continue;
	            
					var obstaclePos = nearbyTile.Location;

					SkillLocation location;
					if (skill.GetRangeType () == RangeType.Route)
						location = new SkillLocation (prevTile, nearbyTile, _Convert.Vec2ToDir(obstaclePos - tilePos));
					else if (skill.GetRangeType () == RangeType.Auto)
						location = new SkillLocation (prevTile, prevTile, _Convert.Vec2ToDir(obstaclePos - tilePos));
					else
						location = new SkillLocation (prevTile, nearbyTile, Utility.GetDirectionToTarget (tilePos, obstaclePos));
					Casting casting = new Casting (actor, skill, location);
					int destroyCost = Obstacle.CalculateIntKillNeedCount (casting) * actor.GetActualRequireSkillAP (skill);
					if (destroyCost < 10000 && destroyCost < minDestroyCost) {
						minDestroyCost = destroyCost;
						AI.selectedSkill = skill;
					}
				}

				dataStorage.DestroyCostDict.Add(Obstacle, minDestroyCost);
			}

			minDestroyCost = dataStorage.DestroyCostDict[Obstacle];
			if (minDestroyCost == int.MaxValue) return;
			
			requireAP += minDestroyCost;
			nearbyTileWithPath.AddRequireAP (minDestroyCost);
			nearbyTileWithPath.skillForDestroy = AI.selectedSkill;
		}
		
		//if, 새로운 타일이거나, 기존보다 ap가 더 적게 드는 경로일 경우 업데이트하고 해당 타일을 queue에 넣음.
		if (!tilesWithPath.ContainsKey(nearbyPos)){
			tilesWithPath.Add(nearbyPos, nearbyTileWithPath);
			tileQueue.Enqueue(nearbyPos);
			return;
		}
		if (requireAP >= tilesWithPath[nearbyPos].requireActivityPoint) return;
		tilesWithPath.Remove(nearbyPos);
		tilesWithPath.Add(nearbyPos, nearbyTileWithPath);
		tileQueue.Enqueue(nearbyPos);
	}
}
