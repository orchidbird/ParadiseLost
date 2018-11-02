using UnityEngine;
using Enums;
using System.Collections.Generic;

public class SkillLocation {
	public static TileManager tileManager;
	Tile casterTile;
	Tile targetTile;
	Direction direction;
	public SkillLocation(Tile casterTile, Tile targetTile, Direction direction){
		this.casterTile = casterTile;
		this.targetTile = targetTile;
		this.direction = direction;
	}
	public SkillLocation(Vector2Int casterPos, Vector2Int targetPos, Direction direction){
		casterTile = tileManager.GetTile (casterPos);
		targetTile = tileManager.GetTile (targetPos);
		this.direction = direction;
	}
	public Tile CasterTile{
		get { return casterTile; }
	}
	public Vector2Int CasterPos{
		get { return casterTile.Location; }
	}
	public Tile TargetTile{
		get { return targetTile; }
	}

	public void SetRealTargetTile(ActiveSkill skill){
		if (skill.GetRangeType() == RangeType.Route){
			List<Tile> firstRange = skill.GetTilesInFirstRange (CasterPos, Dir);
			Tile routeEnd = TileManager.GetRouteEndForPC (firstRange);
			targetTile = routeEnd;
		}else if(skill.GetRangeType() == RangeType.Auto){
			targetTile = TileManager.Instance.GetTile(CasterPos);
		}
	}
	
	public Vector2Int TargetPos{
		get { return targetTile.Location; }
	}
	public Direction Dir{
		get { return direction; }
	}
}
