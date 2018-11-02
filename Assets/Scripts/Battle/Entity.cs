using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Damage;
using Battle.Skills;
using Battle.Turn;
using DG.Tweening;
using Enums;
using GameData;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class Entity : MonoBehaviour{
	public Tile GetTile{get { return this is Tile ? (Tile) this : ((Unit) this).TileUnderUnit; }}

	public Vector2Int Pos{get{return GetTile.Location;}}
	public int Height{get{return GetTile.GetHeight();}}

	public int WillDownFormBlood{get{return TilesInSight.FindAll(tile => tile.isBloody).Sum(tile => Math.Max(5 - Calculate.Distance(tile, this, 1), 0));}}

	protected List<Tile> TilesInDistance(int dist, int lowerTileFactor){
		return TileManager.Instance.GetTilesInGlobalRange().FindAll(tile =>
			Calculate.Distance(GetTile, tile, lowerTileFactor) <= dist);
	}
	protected List<Tile> TilesInSight{get{return GetTile.TilesInSightDistance.FindAll(IsVisible);}}
	
	public bool IsVisible(Entity obj){
		var TM = TileManager.Instance;
		Vector2 viewerPos = Pos; Vector2 objPos = obj.Pos; int height = Height;
        int minY = (int)Math.Min(viewerPos.y, objPos.y);
        int maxY = (int)Math.Max(viewerPos.y, objPos.y);
        int minX = (int)Math.Min(viewerPos.x, objPos.x);
        int maxX = (int)Math.Max(viewerPos.x, objPos.x);
        bool checkHorizontally = (maxX - minX) > (maxY - minY);
        bool positiveGradient = (objPos.x - viewerPos.x) * (objPos.y - viewerPos.y) > 0;
        bool obstructed = false;
        if (!checkHorizontally) {
            for (int y = minY + 1; y <= maxY; y++) {
                float x = (viewerPos.x + 0.5f) * (objPos.y + 0.5f - y) / (objPos.y - viewerPos.y) + (objPos.x + 0.5f) * (y - viewerPos.y - 0.5f) / (objPos.y - viewerPos.y);
	            var interTiles = positiveGradient
		            ? new List<Tile> { TM.GetTile(new Vector2Int((int)(x - 0.001f), y - 1)), TM.GetTile(new Vector2Int((int)x, y)) }
		            : new List<Tile> { TM.GetTile(new Vector2Int((int)x, y - 1)), TM.GetTile(new Vector2Int((int)(x - 0.001f), y)) };
                foreach (var interTile in interTiles) {
	                if (interTile == null || !TM.isSightObstacle(interTile, height) || interTile == obj || interTile == GetTile) continue;
	                obstructed = true;
	                break;
                }
                if (obstructed)
                    break;
            }
        }
        else {
            for (int x = minX + 1; x <= maxX; x++) {
                float y = (viewerPos.y + 0.5f) * (objPos.x + 0.5f - x) / (objPos.x - viewerPos.x) + (objPos.y + 0.5f) * (x - viewerPos.x - 0.5f) / (objPos.x - viewerPos.x);
	            var interTiles = positiveGradient
		            ? new List<Tile> { TM.GetTile(new Vector2Int(x - 1, (int)(y - 0.001f))), TM.GetTile(new Vector2Int(x, (int)y)) }
		            : new List<Tile> { TM.GetTile(new Vector2Int(x - 1, (int)y)), TM.GetTile(new Vector2Int(x, (int)(y - 0.001f))) };
                foreach (var interTile in interTiles) {
	                if (interTile == null || interTile == obj || interTile == GetTile || !TM.isSightObstacle(interTile, height)) continue;
	                obstructed = true;
	                break;
                }
                if (obstructed)
                    break;
            }
        }
        return !obstructed;
    }
}
