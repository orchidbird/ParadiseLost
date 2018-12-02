using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Enums;
using GameData;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UtilityMethods;

public class Utility : MonoBehaviour {
    static float GetDegreeToTarget(Vector2 startPosition, Vector2 targetPosition) {
        float deltaDegree = Mathf.Atan2(targetPosition.y - startPosition.y, targetPosition.x - startPosition.x) * Mathf.Rad2Deg;
        return deltaDegree;
    }

    public static List<Tile> GetGrabPath(Unit caster, Unit target) {    //caster,result[n-1],...,target(result[0])
        var tileManager = TileManager.Instance;
        List<Tile> path = new List<Tile>();
        Vector2Int casterPosition = caster.Pos;
        Vector2Int targetPosition = target.Pos;
        Vector2Int directionVector = Calculate.NormalizeV2I(targetPosition - casterPosition);
        Vector2Int currentPosition = targetPosition;
        path.Add(target.TileUnderUnit);
		if (target.CanBeForcedToMove ()) {
			for (int i = 0; i < (targetPosition - casterPosition).magnitude; i++) {
				currentPosition -= directionVector;
				Tile tile = tileManager.GetTile (currentPosition);
				if (tile == null) return path;
				if (tile.IsBasicallyPassable && !tile.IsUnitOnTile())
					path.Add (tile);
				else
					return path;
			}
		}
        return path;
    }

	public static List<Tile> GetChargePath(Vector2Int casterPos, Vector2Int targetPos) {  //caster(result[0]),...,result[n-1],target
		TileManager TM = TileManager.Instance;
        List<Tile> path = new List<Tile>();
	    Vector2Int directionVector = Calculate.NormalizeV2I(targetPos - casterPos);
		int distance = Calculate.Distance(casterPos, targetPos);

        Vector2Int currentPosition = casterPos;
		path.Add (TM.GetTile (casterPos));
		for (int i = 1; i <= distance - 2; i++) {
			currentPosition += directionVector;
			Tile tile = TM.GetTile (currentPosition);
			if (TM.isTileJumpable (tile)) // 타일이 없는 빈 구멍이거나, 유닛이 없는 빈 타일이면(이동불가 타일이더라도) 통과 가능
				path.Add (tile);
			else
				return path;
		}

		currentPosition += directionVector;
		Tile lastTile = TM.GetTile(currentPosition);
		if (TM.isTilePassable (BattleData.turnUnit, lastTile, false)) // 돌진 마지막 목적지는 밟을 수 있는 타일이어야 함
			path.Add (lastTile);
        return path;
    }

	public static List<Tile> GetPushPath(Direction direction, Unit target, int dist) {  //caster,...,target(result[0]),...,result[n-1]
        TileManager tileManager = TileManager.Instance;
        List<Tile> path = new List<Tile>();
        Vector2Int directionVector = DirToV2I(direction);
        Vector2Int currentPosition = target.Pos;
        path.Add(target.TileUnderUnit);
		if (target.CanBeForcedToMove ()) {
			for (int i = 0; i < dist; i++) {
				currentPosition += directionVector;
				Tile tile = tileManager.GetTile (currentPosition);
				if (tileManager.isTilePassable (BattleData.turnUnit, tile, false))
					path.Add (tile);
				else
					return path;
			}
		}
        return path;
    }

    public static Direction GetDirectionToTarget(Unit unit, List<Tile> selectedTiles) {
        return GetDirectionToTarget(unit, AveragePos(selectedTiles));
    }

    public static Vector3 AveragePos<T>(List<T> input){
	    var newList = input.ConvertAll(item => (MonoBehaviour) (object) item);
        var averagePosition = new Vector3(0, 0, 0);
	    foreach (var item in newList)
		    averagePosition += item.transform.position;
        return averagePosition / input.Count;
    }

	public static bool CheckEqaul<T>(List<T> input1, List<T> input2){
		if (input1.Count != input2.Count) return false;

		for (int i = 0; i < input1.Count; i++)
			if (!input2.Contains(input1[i]))
				return false;

		return true;
	}

    public static Direction GetDirectionToTarget(Unit unit, Vector2 targetPosition) {
        return GetDirectionToTarget(unit.Pos, targetPosition);
    }
    public static Direction GetDirectionToTarget(Vector2 startPosition, Vector2 targetPosition) {
        float deltaDegree = GetDegreeToTarget(startPosition, targetPosition);

        if ((-45 < deltaDegree) && (deltaDegree <= 45)) return Direction.RightDown;
        else if ((45 < deltaDegree) && (deltaDegree <= 135)) return Direction.RightUp;
        else if ((-135 < deltaDegree) && (deltaDegree <= -45)) return Direction.LeftDown;
        else if ((deltaDegree <= -135) || (135 < deltaDegree)) return Direction.LeftUp;

        else {
            Debug.LogWarning("Result degree : " + deltaDegree);
            return Direction.RightUp;
        }
    }

    public static float GetDegreeAtAttack(Unit unit, Unit target) {
        if (unit == target) return 180;

        float deltaDegreeAtLook = GetDegreeToTarget(unit.Pos, target.Pos);

        float targetDegree;
        if (target.GetDir() == Direction.RightDown) targetDegree = 0;
        else if (target.GetDir() == Direction.RightUp) targetDegree = 90;
        else if (target.GetDir() == Direction.LeftUp) targetDegree = -180;
        else targetDegree = -90;

        float deltaDegreeAtAttack = Mathf.Abs(targetDegree - deltaDegreeAtLook);

        return deltaDegreeAtAttack;
    }

    public static float GetDirectionBonus(Unit unit, Unit target) {
		if (!VolatileData.OpenCheck(Setting.directionOpenStage) || target == null || target.IsObject)
            return 1.0f;

        float deltaDegreeAtAttack = GetDegreeAtAttack(unit, target);
        if ((deltaDegreeAtAttack < 45) || (deltaDegreeAtAttack > 315)) return Setting.backAttackBonus;
        else if ((deltaDegreeAtAttack < 135) || (deltaDegreeAtAttack > 225)) return Setting.sideAttackBonus;
        else return 1.0f;
    }

    public static float GetHeightBonus(Unit attacker, Unit defender) {
		// 상대가 낮으면 20% 추가, 상대가 높으면 20% 감소
        int attackerHeight = attacker.GetHeight();
        int defenderHeight = defender.GetHeight();

        if (attackerHeight > defenderHeight)
            return 1.2f;
        if (attackerHeight < defenderHeight)
            return 0.8f;
        return 1;
    }

    public static Tile GetFarthestTileToUnit(List<Vector2Int> range, Dictionary<Vector2Int, TileWithPath> movableTilesWithPath, Unit unit) {
        Tile farthestTile = null;
        int maxDistanceToUnit = -1;
        foreach (var pos in range) {
            if (!movableTilesWithPath.ContainsKey(pos)) continue;
            Tile tile = TileManager.Instance.GetTile(pos);
            int distanceToUnit = Calculate.DistanceToUnit(pos, unit);
            if (distanceToUnit <= maxDistanceToUnit) continue;
            farthestTile = tile;
            maxDistanceToUnit = distanceToUnit;
        }
        return farthestTile;
    }
    public static Tile GetNearestTileToUnit(Dictionary<Vector2, TileWithPath> movableTilesWithPath, Unit unit) {
        Tile nearestTile = null;
        int minDistanceToUnit = 999999;
        foreach (var kv in movableTilesWithPath) {
            Tile tile = kv.Value.dest;
            Vector2Int pos = tile.Location;
            int distanceToUnit = Calculate.DistanceToUnit(pos, unit);
            if (distanceToUnit >= 1 && distanceToUnit < minDistanceToUnit) {
                nearestTile = tile;
                minDistanceToUnit = distanceToUnit;
            }
        }
        return nearestTile;
	}

	public static List<Vector2Int> GetRange(TileRange range, Vector2Int mid, Direction dir, int lowerTileFactor){
		if (range.form == RangeForm.Diamond)
			return GetDiamondRange(mid, range.min, range.max, lowerTileFactor);
        else if (range.form == RangeForm.Square)
            return GetSquareRange(mid, range.min, range.max);
        else if (range.form == RangeForm.Triangle)
            return GetTriangleRange(mid, range.min, range.max, dir);
        else if (range.form == RangeForm.Straight)
            return GetStraightRange(mid, range.min, range.max, dir);
        else if (range.form == RangeForm.Cross)
            return GetCrossRange(mid, range.min, range.max);
        else if (range.form == RangeForm.Diagonal)
            return GetDiagonalCrossRange(mid, range.min, range.max);
        else if (range.form == RangeForm.AllDirection)
            return GetAllDirectionRange(mid, range.min, range.max);
        else if (range.form == RangeForm.Front)
            return GetFrontRange(mid, range.min, range.max, range.width, dir);
        else if (range.form == RangeForm.Sector)
            return GetSectorRange(mid, range.min, range.max, dir);
        else if (range.form == RangeForm.One)
	        return new List<Vector2Int> {mid};
		
        Debug.LogError("Invalid Range Form!: " + range.form);
        return null; // default return value
    }
    public static List<Tile> TilesInDiamondRange(Vector2Int mid, int min, int max, int lowerTileFactor){
	    var midTile = TileManager.Instance.GetTile(mid);
	    Debug.Assert(midTile != null, mid + " 위치에 타일이 없음!");
	    var tilesToFind = max <= 6 ? midTile.TilesInSightDistance : TileManager.Instance.GetTilesInGlobalRange();
	    return tilesToFind.FindAll(tile =>{
			var dist = Calculate.Distance(midTile, tile, lowerTileFactor);
			return dist >= min && dist <= max;
		});
    }
	
	public static List<Vector2Int> GetDiamondRange(Vector2Int mid, int min, int max, int lowerTileFactor){
		//Debug.Log(mid + " / " + min + " / " + max + " / " + lowerTileFactor);
		var range = new List<Vector2Int>();
		for (int x = mid.x - max; x <= mid.x + max; x++) {
			for (int y = mid.y - max; y <= mid.y + max; y++){
				var pos = new Vector2Int(x, y);
				var dist = Calculate.Distance(mid, pos, (LowerTileDistCalcType) lowerTileFactor);
				if(dist <= max && dist >= min)
					range.Add(pos);
			}
		}
		return range;
	}
    public static List<Vector2Int> GetSquareRange(Vector2Int mid, int min, int max) {
        var range = new List<Vector2Int>();
		for(int i = -max; i <= max; i++) {
			for(int j = -max; j<= max; j++) {
				if(Math.Abs(i) + Math.Abs(j) >= min) {
					range.Add(new Vector2Int(mid.x + i, mid.y + j));
				}
			}
		}
        return range;
    }
    public static List<Vector2Int> GetTriangleRange(Vector2Int mid, int min, int max, Direction dir) {
        var range = new List<Vector2Int>();
        Vector2Int frontVector = DirToV2I(dir);
        var perpendicularVector = new Vector2Int(frontVector.y, frontVector.x);
        for (int i = 1; i <= max; i++) {
            int width = max - i;
            for (int j = -width; j <= width; j++) {
                if (Math.Abs(j) < min - i) continue;
                Vector2Int pos = mid + perpendicularVector * j + frontVector * i;
                range.Add(pos);
            }
        }
        return range;
    }
    public static List<Vector2Int> GetStraightRange(Vector2Int mid, int min, int max, Direction dir) {
        return GetFrontRange(mid, min, max, 1, dir);
    }
    public static List<Vector2Int> GetCrossRange(Vector2Int mid, int min, int max) {
        var range = new List<Vector2Int>();
        if (min == 0)
            range.Add(mid);
        int newmin = Math.Max(1, min);
        foreach (var direction in EnumUtil.directions)
            range.AddRange(GetStraightRange(mid, newmin, max, direction));
        return range;
    }
    public static List<Vector2Int> GetDiagonalCrossRange(Vector2Int mid, int min, int max) {
        var range = new List<Vector2Int>();
        if (min == 0)
            range.Add(mid);
        int newmin = Math.Max(1, min);
        foreach (Direction direction in EnumUtil.nonTileDirections)
            range.AddRange(GetStraightRange(mid, newmin, max, direction));
        return range;
    }
    public static List<Vector2Int> GetAllDirectionRange(Vector2Int mid, int min, int max) {
        var range = new List<Vector2Int>();
        range.AddRange(GetCrossRange(mid, min, max));
        range.AddRange(GetDiagonalCrossRange(mid, min, max));
        return range;
    }
    public static List<Vector2Int> GetFrontRange(Vector2Int mid, int min, int max, int width, Direction dir) {
        var range = new List<Vector2Int>();
        Vector2Int frontVector = DirToV2I(dir);
        var perpendicularVector = new Vector2Int(frontVector.y, frontVector.x);
        int sideOffset = (width - 1) / 2;
        for (int side = -sideOffset; side <= sideOffset; side++) {
            for (int front = min; front <= max; front++) {
                Vector2Int pos = mid + perpendicularVector * side + frontVector * front;
                range.Add(pos);
            }
        }
        return range;
    }
    public static List<Vector2Int> GetSectorRange(Vector2Int mid, int min, int max, Direction dir) {
        var range = new List<Vector2Int>();
        Vector2Int frontVector = DirToV2I(dir);
        var perpendicularVector = new Vector2Int(frontVector.y, frontVector.x);
        for (int front = min; front <= max; front++) {
            int sideOffset = front - 1;
            if (min == 0)
                sideOffset++;
            for (int side = -sideOffset; side <= sideOffset; side++) {
                Vector2Int pos = mid + perpendicularVector * side + frontVector * front;
                range.Add(pos);
            }
        }
        return range;
    }

    public static Vector2Int DirToV2I(Direction dir) {
        if (dir == Direction.LeftUp)
            return Vector2Int.left;
        else if (dir == Direction.LeftDown)
            return Vector2Int.down;
        else if (dir == Direction.RightUp)
            return Vector2Int.up;
        else if (dir == Direction.RightDown)
            return Vector2Int.right;

        else if (dir == Direction.Left)
            return Vector2Int.left + Vector2Int.down;
        else if (dir == Direction.Right)
            return Vector2Int.right + Vector2Int.up;
        else if (dir == Direction.Up)
            return Vector2Int.left + Vector2Int.up;
        else
            return Vector2Int.right + Vector2Int.down;
    }

    // 반시계 방향 회전
    public static Vector2 Rotate(Vector2 vec, float angle, bool isRadian = true) {
        if (!isRadian)
            angle = (float)(angle * Math.PI / 180f);
        return new Vector2((float)(vec.x * Math.Cos(angle) - vec.y * Math.Sin(angle)),
                           (float)(vec.x * Math.Sin(angle) + vec.y * Math.Cos(angle)));
    }

    static void SetPropertyImage<T>(Image image, T enumInput){
	    if (image == null) return;
        enumInput = CheckStageAndSetNone(enumInput);
        image.sprite = VolatileData.GetIcon(enumInput);
    }

    public static T CheckStageAndSetNone<T>(T enumInput) {
        if (typeof(T) == typeof(UnitClass) && !VolatileData.OpenCheck(Setting.classOpenStage)) {
            return (T)(object)UnitClass.None;
		}if (typeof(T) == typeof(Element) && !VolatileData.OpenCheck(Setting.elementOpenStage)) {
            return (T)(object)Element.None;
        }
        return enumInput;
    }

    public static IEnumerator WaitForFewFrames(int frameCount) {
        for (int i = 0; i < frameCount; i++) {
            yield return null;
        }
    }

    public static List<T> ArrayToList<T>(T[] array) {
        List<T> newList = new List<T>();
        foreach (T item in array) {
            newList.Add(item);
        }
        return newList;
    }

    public static List<Unit> UnitsInRange(List<Tile> input){
        Debug.Assert(input != null);
        return input.FindAll(tile => tile.IsUnitOnTile()).ConvertAll(tile => tile.GetUnitOnTile());
    }

    public static List<T> DeleteNull<T>(List<T> input) {
        return input.FindAll(item => item != null);
    }

    public static bool CheckShowMotion(){
	    var actor = BattleData.turnUnit;
        if(actor == null) return false;
	    return (actor.IsPC && Setting.showMotionPC) || (!actor.IsPC && !Setting.FastAITurn && !actor.IsHiddenUnderFogOfWar());
    }

    //BattleData.turnUnit의 정보를 기반으로 하므로, Unit이 없는 씬에서는 사용 불가
    public static IEnumerator WaitTime(float seconds) {
        if (CheckShowMotion()) yield return new WaitForSeconds(seconds);
        else yield return new WaitForSeconds(seconds / 4);
    }

    public static int CharToInt(char c) {
        Debug.Assert(c >= '0' && c <= '9');
        return c - '0';
    }

    public static IEnumerator Resize(RectTransform rect, Vector2 endValue, float duration, bool from = false, bool relative = false) {
        if (relative) {
            endValue.x *= rect.sizeDelta.x;
            endValue.y *= rect.sizeDelta.y;
        }
        if (from)
            yield return rect.DOSizeDelta(endValue, duration).From().WaitForCompletion();
        else yield return rect.DOSizeDelta(endValue, duration).WaitForCompletion();
    }
    public static IEnumerator Fade(SpriteRenderer sr = null, Graphic graphic = null, float duration = 0, 
			bool fadeBlack = false, bool fadeIn = false, bool useMaterial = false) {
        Tweener tw = null;
		if (!useMaterial){
			if (graphic != null)
				tw = fadeBlack ? graphic.DOColor(new Color(0, 0, 0, 0), duration) : graphic.DOColor(graphic.color - new Color(0, 0, 0, 1), duration);
			else if (sr != null)
				tw = fadeBlack ? sr.DOColor(new Color(0, 0, 0, 0), duration) : sr.DOColor(sr.color - new Color(0, 0, 0, 1), duration);
		} else
			tw = graphic.material.DOFloat(0, "_Alpha", duration);

	    if (!fadeIn) yield return tw.WaitForCompletion();
        else yield return tw.From().WaitForCompletion();
    }

    public static void ChangeParticleColor(ParticleSystem effect, Color color, bool onlyAlpha = false) {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[effect.particleCount];
        effect.GetParticles(particles);
        for (int i = 0; i < particles.Length; i++) {
            if (onlyAlpha) {
                Color originalColor = particles[i].startColor;
                originalColor.a = color.a;
                particles[i].startColor = originalColor;
            } else particles[i].startColor = color;
        }
        effect.SetParticles(particles, particles.Length);
    }

    public static IEnumerator TweenCoroutine(Tweener tw) {
        yield return tw.WaitForCompletion();
    }
    public static IEnumerator RewindTweenCoroutine(Tweener tw) {
        yield return tw.WaitForRewind();
    }
    public static T Maximizer<T>(List<T> list, Func<T, float> func) {
        T maximizer = default(T);
        float max = -float.MaxValue;
        foreach (var elem in list) {
            if (func(elem) > max) {
                maximizer = elem;
                max = func(maximizer);
            }
        }
        return maximizer;
    }
    public static T Minimizer<T>(List<T> list, Func<T, float> func) {
        T minimizer = default(T);
        float min = float.MaxValue;
        foreach (var elem in list) {
            if (func(elem) < min) {
                minimizer = elem;
                min = func(minimizer);
            }
        }
        return minimizer;
    }
    //LeftDown, LeftUp, RightUp, RightDown
    public static Vector3 GetRectCornerPosition(RectTransform rect, Direction direction) {
        Vector3[] v = new Vector3[4];
        rect.GetWorldCorners(v);
        switch (direction) {
        case Direction.LeftDown:
            return v[0];
        case Direction.LeftUp:
            return v[1];
        case Direction.RightUp:
            return v[2];
        case Direction.RightDown:
            return v[3];
        }
        return new Vector3(0, 0, 0);
    }
    public static Vector3 GetRectCornerPosition(GameObject gameObject, Direction direction){
        return GetRectCornerPosition(gameObject.GetComponent<RectTransform>(), direction);
    }
    public static Vector3 RectLocalPositionToWorldPosition(RectTransform rect, Vector2 localUV){
        Vector3 leftDownPos = GetRectCornerPosition(rect, Direction.LeftDown);
        Vector3 rightUpPos = GetRectCornerPosition(rect, Direction.RightUp);
        //Debug.Log(leftDownPos + " To " + rightUpPos);

        float x = (rightUpPos.x - leftDownPos.x) * localUV.x + leftDownPos.x;
        float y = (rightUpPos.y - leftDownPos.y) * localUV.y + leftDownPos.y;
        return new Vector3(x, y);
    }
    public static Vector3 RectLocalPositionToWorldPosition(GameObject gameObject, Vector2 localPosition) {
        return RectLocalPositionToWorldPosition(gameObject.GetComponent<RectTransform>(), localPosition);
    }
	public static Faction PCNameToFaction(string name){
		switch (name) {
		case "arcadia":
			return Faction.Aster;
		case "bianca":
			return Faction.Aster;
		case "curi":
			return Faction.Haskell;
		case "darkenir":
			return Faction.Aster;
		case "deus":
			return Faction.Pintos;
		case "eren":
			return Faction.Pintos;
		case "eugene":
			return Faction.Haskell;
		case "grenev":
			return Faction.Aster;
		case "json":
			return Faction.Haskell;
		case "karldrich":
			return Faction.Haskell;
		case "kashasty":
			return Faction.Pintos;
		case "lenien":
			return Faction.Pintos;
		case "lucius":
			return Faction.Haskell;
		case "luvericha":
			return Faction.Haskell;
		case "noel":
			return Faction.Aster;
		case "ratice":
			return Faction.Haskell;
		case "reina":
			return Faction.Pintos;
		case "sepia":
			return Faction.Aster;
		case "triana":
			return Faction.Pintos;
		case "yeong":
			return Faction.Pintos;
		default:
			return Faction.Aster;
		}
	}

	public static bool needsManualGeneration{
		get { return VolatileData.OpenCheck(Setting.manualGenerationOpenStage) || VolatileData.gameMode == GameMode.Challenge; }
	}
}
