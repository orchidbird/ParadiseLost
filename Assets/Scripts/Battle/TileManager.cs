using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Enums;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour {
	private static TileManager instance = null;
	public static TileManager Instance { get { return instance; } }
	
	public static void SetInstance() { instance = FindObjectOfType<TileManager>(); }

	public GameObject tilePrefab;
	public GameObject tileHeightPrefab;

	Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
	bool preselectLockOn;
	public Tile preSelectedMouseOverTile;
	public Tile mouseOverTile;
	public Unit mouseOverUnit;

	public Tile GetTile(Vector2Int position){
		try{
			return tiles[position];
		}catch{
			return null;
		}
	}
	public List<Tile> GetTilesNearby(Vector2Int centerPos){
		return new List<Tile>{
			GetTile(centerPos + Vector2Int.down),
			GetTile(centerPos + Vector2Int.left),
			GetTile(centerPos + Vector2Int.right),
			GetTile(centerPos + Vector2Int.up)
		};
	} 

	public static List<Tile> GetRouteTiles(List<Tile> tiles) {
		List<Tile> routeTiles = new List<Tile>();
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 탐색을 종료한다.
			// break;

			// 첫 유닛을 만난 경우
			// 이번 타일을 마지막으로 종료한다.
			routeTiles.Add(tile);
			if (tile.IsUnitOnTile())
				break;
		}
		return routeTiles;
	}
	
	public static Tile GetRouteEndForAI(List<Tile> tiles) {
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 null을 return한다.

			// 첫 유닛을 만난 경우
			// 이번 타일을 return하고 종료한다.
			if (tile.IsUnitOnTile())
				return tile;
		}
		return null;
	}
	
	public static Tile GetRouteEndForPC(List<Tile> tiles) {
		foreach (var tile in tiles) {
			// 타일 단차에 의한 부분(미구현)
			// 즉시 막혀서 더는 진행하지 못하는 마지막 타일을 return한다.

			// 첫 유닛을 만난 경우
			// 이번 타일을 return하고 종료한다.
			if (tile.IsUnitOnTile()) {return tile;}
		}

		return null;
	}

    public bool isSightObstacle(Tile tile, int unitHeight) {
        // 이 tile이 unitHeight의 높이를 가지는 유닛에게 시야 장애물이 되는지 리턴
        return (tile.IsUnitOnTile() && tile.GetUnitOnTile().IsObject) || tile.height >= unitHeight + 1;
    }

    public bool isTilePassable(Unit actor, Tile tile, bool ignoreAlly){
		return tile != null && tile.IsActuallyPassable(actor, ignoreAlly); 
    }
	public bool isTileJumpable(Tile tile) {return tile == null || !tile.IsUnitOnTile ();}
	
	public void RemoveFogs(List<Tile> tiles){
		for (int i = 0; i < tiles.Count; i++)
			tiles[i].SetFogType(FogType.None);
	}
	
	public List<Tile> TilesInRange(TileRange range, Vector2Int mid, Direction dir, int lowerTileFactor, bool onlyVisible = false, bool ignoreHeight = false){
		if(range.form == RangeForm.Global)
			return GetTilesInGlobalRange(onlyVisible);
		if (range.form == RangeForm.One)
			return new List<Tile> {GetTile(mid)};
		if (range.form == RangeForm.Diamond)
			return Utility.TilesInDiamondRange(mid, range.min, range.max, lowerTileFactor);	
		
		var tiles = GetTilesInPositionRange(Utility.GetRange(range, mid, dir, lowerTileFactor), onlyVisible);
		
		//유효범위를 계산할 때 적용하기 위함(=> 지정범위 계산에는 필요없는 부분)
		int maxReach = range.max;
		if (range.form == RangeForm.Diamond && lowerTileFactor == -1 || ignoreHeight)
			return tiles;
		
		if (range.form == RangeForm.Square || range.form == RangeForm.Diagonal || range.form == RangeForm.AllDirection)
			maxReach *= 2;
		else if (range.form == RangeForm.Sector)
			maxReach = maxReach * 2 - 1;
		return tiles.FindAll(tile => (maxReach + 1) / 2 >= Math.Abs(GetTile(mid).GetHeight() - tile.GetHeight()));
	}
	public static List<Tile> V2ToTiles(List<Vector2Int> input) {
		List<Tile> tiles = input.ConvertAll(v2 => Instance.GetTile(v2));
		return Utility.DeleteNull(tiles);
	}

	static List<Tile> allTiles;
	public List<Tile> GetTilesInGlobalRange(bool onlyVisible = false){
		return onlyVisible ? allTiles.FindAll(tile => tile.IsVisible()) : allTiles;
	}
	public Dictionary<Vector2, Tile> GetAllTiles(){
		return tiles;
	}
	
	private Sprite BloodSprite{get{return Resources.Load<Sprite>("Icon/UI/Blood" + (Configuration.showRealBlood ? "Real" : ""));}}
	public void SetBloodShow(){
		foreach (var tile in GetTilesInGlobalRange())
			tile.blood.sprite = BloodSprite;
	}

	public List<Tile> GetTilesInPositionRange(List<Vector2Int> positionRange, bool onlyVisible = false){
		var tiles = new List<Tile> ();
		foreach (Vector2Int pos in positionRange) {
			var tile = GetTile (pos);
			if (tile != null && (!onlyVisible || tile.IsVisible()))
				tiles.Add (tile);
		}
		return tiles;
	}

	public void PreselectTiles(List<Tile> tiles){
		if (preselectLockOn) return;
		
		foreach (var tile in tiles){
			tile.SetPreSelected (true);
		}
	}

	public void PreselectTilesForTutorial(List<Tile> tiles){
		BattleUIManager.Instance.TurnOffAllActions();
		BattleUIManager.Instance.ActionButtonOnOffLock = true;
		DepreselectAllTiles ();
		PreselectTiles (tiles);
		SetPreselectLock (true);
		SetHighlightTiles(tiles, ShowType.Blink);
	}

	//SetCondition에서는 다른 행동을 막아야 하므로 true, Reset에서는 false
	public void DepreselectTilesForTutorial(bool ifLock){
		SetHighlightTiles(GetTilesInGlobalRange().FindAll(tile => !BattleManager.Instance.escapeSpotTiles.Contains(tile)), ShowType.Off);
		DepreselectAllTiles();
		SetPreselectLock(ifLock);
		BattleUIManager.Instance.ActionButtonOnOffLock = ifLock;
	}

	public void DepreselectTiles(List<Tile> tiles) {
		if (preselectLockOn) {return;}
		else {
			foreach (Tile tile in tiles) {
				tile.SetPreSelected (false);
			}
		}
	}

	public void DepreselectAllTiles(){
		if(preselectLockOn) return;
		
		foreach (var tile in GetTilesInGlobalRange()) 
			tile.SetPreSelected (false);
	}

	public void SetPreselectLock(bool OnOff) {preselectLockOn = OnOff;}

	public Material TileColorMaterialForMove;
	public Material TileColorMaterialForRange1;
	public Material TileColorMaterialForRange2;
	public Material TileColorMaterialForMixedRange;
	public Material TileColorMaterialForChain;
	
	public void PaintTiles(List<Tile> tiles, Material color){
		foreach (var tile in tiles)
			tile.PaintTile(color);
	}
	public void DepaintTiles(List<Tile> tiles, Material color){
		foreach(var tile in tiles)
			tile.DepaintTile(color);
	}
	public void SetHighlightTiles(List<Tile> tiles, ShowType showType){
		tiles.ForEach(tile => tile.SetHighlight(showType));
	}

	public void ClearAllTileColors(){
		foreach (var tile in GetTilesInGlobalRange()) {
			tile.ClearTileColor ();
		}
	}
	public void DepaintAllTiles(Material color){
		DepaintTiles(GetTilesInGlobalRange(), color);
	}
    public IEnumerable<EventLog> CheckAllTraps() {
        foreach (var tile in GetAllTiles().Values) {
            foreach (var statusEffect in tile.GetStatusEffectList()) {
	            if (!statusEffect.IsTypeOf(StatusEffectType.Trap)) continue;
	            EventLog trapOperatedLog = Trap.Update(statusEffect);
	            if(trapOperatedLog != null)
		            yield return trapOperatedLog;
            }
        }
    }
    public List<TileStatusEffect> FindTrapsOnThisTile(Tile tile) {
		List<TileStatusEffect> traps = new List<TileStatusEffect>();
		if (tile == null)
			return traps;
		foreach(var statusEffect in tile.StatusEffectList) {
			if(statusEffect.IsTypeOf(StatusEffectType.Trap))
				traps.Add(statusEffect);
		}
        return traps;
    }
    public void TriggerTileStatusEffectsAtPhaseStart() {
        foreach(var tile in GetAllTiles().Values) {
            foreach(var statusEffect in tile.StatusEffectList) {
                Skill originSkill = statusEffect.GetOriginSkill();
                if(originSkill is ActiveSkill)
                    ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectAtPhaseStart(statusEffect);
            }
        }
    }
    public void UpdateTileStatusEffectsAtActionEnd() {
		Aura.UpdateAllAuraOnTileEffects (allTiles);
		
        foreach (var tile in GetAllTiles().Values) {
            foreach (var statusEffect in tile.GetStatusEffectList()) {
                if (statusEffect.Stack != 0)
                    for (int i = 0; i < statusEffect.actuals.Count; i++)
                        statusEffect.CalculateAmount(i, true);
                else tile.RemoveStatusEffect(statusEffect);
            }
        }
    }

    public void EndPhase(int phase) {
        // Decrease each buff & debuff phase
        foreach (var tile in GetAllTiles())
            tile.Value.UpdateRemainPhaseAtPhaseEnd();
    }

	public readonly static int mapSize = 12;
	void GenerateTiles(){
		for (int x = 1; x <= mapSize; x++)
			for (int y = 1; y <= mapSize; y++)
				GenerateTile(x, y);
	}
	void GenerateTile (int x, int y){
		if (Random.Range(0f, 1f) < 0.05f) return;

		var height = Random.Range(0, 3);
		var location = new Vector2Int(x, y);
		var tile = Instantiate(tilePrefab, CalculateRealTilePosition(location, height), Quaternion.identity).GetComponent<Tile>();
		tile.name = "Tile(" + x + "," + y + ")";
		tile.height = height;
		tile.transform.SetParent(transform);
		tile.SetLocation(location);
		tile.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("TileImage/metal_01");
		tiles.Add(new Vector2Int(x, y), tile);
		tile.APAtStandardHeight = 3;
		tile.fogType = FogType.Black;
	}
	
	public void GenerateMap(){
		GenerateTiles();
		foreach (var tile in tiles.Values)
			tile.FillHeight();

		allTiles = GetAllTiles().Values.ToList();
		SetBloodShow();
	}

	public void UpdateRealTilePositions() {
        foreach(var tile in GetAllTiles()) {
            tile.Value.UpdateRealPosition();
        }
    }
	public static Vector3 CalculateRealTilePosition(Vector2Int tilePos, int height){
        int realX = 0 , realZ = 0;
        switch(BattleData.aspect) {
        case (Aspect.North):
            realX = tilePos.x + tilePos.y;
            realZ = tilePos.y - tilePos.x;
            break;
        case (Aspect.East):
            realX = tilePos.x - tilePos.y;
            realZ = tilePos.y + tilePos.x;
            break;
        case (Aspect.South):
            realX = - tilePos.x - tilePos.y;
            realZ = - tilePos.y + tilePos.x;
            break;
        case (Aspect.West):
            realX = - tilePos.x + tilePos.y;
            realZ = - tilePos.y - tilePos.x;
            break;
        }
		return new Vector3(Setting.tileImageWidth * realX * 0.5f, Setting.tileImageHeight * (realZ+height) * 0.5f, realZ * 0.1f);
	}

	//PC 차례에만 사용할 것!
	public void ShowProperTileColors(){
		Unit actor = BattleData.turnUnit;

		ClearAllTileColors();
		//DepreselectAllTiles();

		if(BattleData.currentState == CurrentState.FocusToUnit && actor.IsMovePossibleState()){
			List<Tile> MovableTiles = actor.MovableTilesWithPath.Keys.ToList().ConvertAll(GetTile);
			PaintTiles(MovableTiles, TileColorMaterialForMove);
			PreselectTiles(MovableTiles);
		}else if(BattleData.currentState == CurrentState.SelectSkillApplyArea){
			List<Tile> firstRange = new List<Tile>();
			firstRange = BattleData.selectedSkill.GetTilesInFirstRange (actor.Pos, actor.GetDir(), onlyVisible : true);
            PaintTiles(firstRange, TileColorMaterialForRange1);
	    	PreselectTiles(firstRange);
		}//else Debug.LogError("잘못된 State에서 호출됨 : ");

		if (BattleUIManager.Instance.unitViewerUI.gameObject.activeSelf)
			BattleUIManager.Instance.unitViewerUI.unit.ShowVigilOrChainArea();
	}
}

public class TileRange{
	public RangeForm form;
	public int min;
	public int max;
	public int width;

    public TileRange(RangeForm form, int min, int max, int width) {
        this.form = form;
        this.min = min;
        this.max = max;
        this.width = width;
    }
	public TileRange(string data, TileRange copy = null){
		if(data == "Same"){ //1차범위와 2차범위가 동일.
			form = copy.form;
			min = copy.min;
			max = copy.max;
			width = copy.width;
		}else{
			StringParser parser = new StringParser(data, '/');
			form = parser.ConsumeEnum<RangeForm>();
			if(parser.origin.Length > 1){
				min = parser.ConsumeInt();
				max = parser.ConsumeInt();
			}
			if(parser.origin.Length > 3) { width = parser.ConsumeInt(); }
		}
	}

	public float ReachDistance{get{
		switch (form){
			case RangeForm.Diamond:
			case RangeForm.Cross:
			case RangeForm.Straight:
			case RangeForm.Triangle:
				return max;
			case RangeForm.AllDirection:
			case RangeForm.Diagonal:
			case RangeForm.Square:
				return max * 1.5f;
			case RangeForm.Front:
				return max + (width - 1) / 4;
			case RangeForm.Global:
				return int.MaxValue;
			case RangeForm.One:
				return 0;
			case RangeForm.Sector:
				return max * 1.5f - 0.5f;
		}
		return 0;
	}}
}
