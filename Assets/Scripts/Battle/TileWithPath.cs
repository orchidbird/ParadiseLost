using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UtilityMethods;

public class TileWithPath{
	public Tile dest;
	public List<Tile> fullPath = new List<Tile>();
	public Unit mover;
	public float requireActivityPoint; // '도착지점'까지 소모되는 ap
	int intRequireActivityPoint; // 생성자 호출이 끝난 후에는, requireActivityPoint와 intRequireActivityPoint는 항상 같이 수정되어야 함(한쪽만 수정하면 문제 일어남)
	public ActiveSkill skillForDestroy;
	
	public TileWithPath(Tile destTile, Unit mover, TileWithPath prevTileWithPath = null){
		dest = destTile;
		this.mover = mover;

		if (prevTileWithPath == null){
			fullPath.Add(dest);
			return;
		}
		fullPath.AddRange(prevTileWithPath.fullPath);
		fullPath.Add(dest);
		skillForDestroy = prevTileWithPath.skillForDestroy;

		requireActivityPoint = prevTileWithPath.requireActivityPoint + NewTileMoveCost(dest, prevTileWithPath.dest, prevTileWithPath.fullPath.Count - 1, mover);
		intRequireActivityPoint = GetIntRequireAP (requireActivityPoint);
	}

	public static float NewTileMoveCost(Tile dest, Tile prev, int prevCount, Unit movingUnit){
		PathFinder.dataStorage.CheckAndUpdate(movingUnit);
		
		var climbMultiplier = dest.GetHeight() > prev.GetHeight() ? 3 : 1;
		float requireAP = dest.GetBaseMoveCost() * climbMultiplier;
		// 거리 가중치 감소(유진-여행자의 발걸음) 효과 적용
		requireAP += (prevCount + movingUnit.movedTileCount) * PathFinder.dataStorage.moveCostAcc; 
		requireAP *= 100f / movingUnit.GetStat (Stat.Will);
		// 이동 필요 행동력 증감 효과 적용
		if (PathFinder.dataStorage.moveCostChange)
			requireAP = movingUnit.CalculateActualAmount (requireAP, StatusEffectType.RequireMoveAPChange);
		
		//Debug.Log(dest.Location + "으로 이동하는 비용: " + requireAP);
		return requireAP;
	}

	int GetIntRequireAP(float actualRequireAP){
		return (int)Math.Round (actualRequireAP + mover.previousMoveCost) - (int)Math.Round(mover.previousMoveCost);
	}

	public void AddRequireAP(int ap){
		requireActivityPoint += ap;
		intRequireActivityPoint += ap;
	}
	public void MultiplyRequireAP(float coef){
		requireActivityPoint *= coef;
		intRequireActivityPoint = GetIntRequireAP (requireActivityPoint);
	}

	public int RequireActivityPoint{
		get { return intRequireActivityPoint; }
	}
}
