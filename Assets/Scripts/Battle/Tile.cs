using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Enums;
using GameData;
using UtilityMethods;

public class Tile : Entity, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
	public int APAtStandardHeight;
	public Vector2Int location;
	public void SetLocation(Vector2Int input) {location = input;}
	public Vector2Int Location{get { return location; }}
	public int height;
	public Vector3 realPosition {
		get { return transform.position; }
	}
    public FogType fogType;
    public GameObject fogOfWarPlane;
	Unit unitOnTile;
	public SpriteRenderer sprite;
	public GameObject highlightWall;
	List<SpriteRenderer> spriteRenderersUnderTile;
	
	public bool IsReachPosition{get{ return highlightWall.activeSelf && BattleData.TutoState != TutorialState.Active; }}
	public bool IsBasicallyPassable{ // 애초에 이동불가 타일인가
		get { return APAtStandardHeight <= 1000; }
	}
	public bool IsActuallyPassable(Unit actor, bool ignoreAlly){
		if (!IsBasicallyPassable) return false;
		if (!IsUnitOnTile()) return true;
		return GetUnitOnTile().IsAllyTo(actor) && ignoreAlly;
	}
	public bool isMouseOver;
	public List<Material> colors;
    List<TileStatusEffect> statusEffectList = new List<TileStatusEffect>();
	bool isPreSelected;
	public TextMesh CostAP;
	public SpriteRenderer tileColorSprite;
	public SpriteRenderer trapImage;
	public SpriteRenderer blood;
	public SpriteRenderer Override;
	public bool isBloody;
	public string displayName;

	public void SetPreSelected(bool input) {
		isPreSelected = input;
		if (input == false) {
			CostAP.gameObject.SetActive (false);
			if (TileManager.Instance.preSelectedMouseOverTile == this)
				TileManager.Instance.preSelectedMouseOverTile = null;
		}
		if (input && TileManager.Instance.mouseOverTile == this)
			TileManager.Instance.preSelectedMouseOverTile = this;
	}
	public bool IsPreSelected() { return isPreSelected; }

	List<Tile> tilesInSightDistance;
	public List<Tile> TilesInSightDistance{get{
		return tilesInSightDistance ?? (tilesInSightDistance = TilesInDistance(5, -1));
	}}

	public void FillHeight(){
		var nearTiles = new List<Tile>{
			TileManager.Instance.GetTile(location + Vector2Int.down),
			TileManager.Instance.GetTile(location + Vector2Int.up),
			TileManager.Instance.GetTile(location + Vector2Int.left),
			TileManager.Instance.GetTile(location + Vector2Int.right)
		};

		int minHeightinNearTiles = 100;
		foreach (var nearTile in nearTiles){
			if(nearTile == null) continue;
			
			minHeightinNearTiles = Math.Min(minHeightinNearTiles, nearTile.GetHeight());
			if (height - minHeightinNearTiles < 3) continue;
			for (int i = minHeightinNearTiles; i < height; i++){
				var item = Instantiate(TileManager.Instance.tileHeightPrefab, transform);
				var itemSR = item.GetComponent<SpriteRenderer>();
				itemSR.sprite = GetComponent<SpriteRenderer>().sprite;
				spriteRenderersUnderTile.Add(itemSR);
				int diff = height - i;
				item.transform.localPosition = new Vector3(0, -0.25f*diff, 0.01f*diff);
			}
		}
	}

    public List<TileStatusEffect> StatusEffectList { get { return statusEffectList; } }
    public List<TileStatusEffect> GetStatusEffectList() { return statusEffectList; }

	public List<TileStatusEffect> GetStatusEffectsOfType(StatusEffectType type){
		return statusEffectList.FindAll(se => se.actuals.Any(actual => actual.statusEffectType == type));
	}

	public void SetStatusEffectList(List<TileStatusEffect> newStatusEffectList) { statusEffectList = newStatusEffectList; }
	public int GetHeight()	{	return height;	}
	public int GetBaseMoveCost(){return APAtStandardHeight;}

	public bool IsUnitOnTile ()	{return unitOnTile != null;}
	public void SetUnitOnTile(Unit unit) {unitOnTile = unit;}
    
    public bool IsVisible() { return fogType != FogType.Black; }

    public void RemoveStatusEffect(TileStatusEffect statusEffect) {
        bool toBeRemoved = true;
        Skill originSkill = statusEffect.GetOriginSkill();
        if (originSkill is ActiveSkill)
            toBeRemoved = ((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectRemoved(this, statusEffect);
        if (toBeRemoved)
            LogManager.Instance.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Remove, 0, 0, 0));
    }
    public void UpdateRemainPhaseAtPhaseEnd() {
        foreach (var statusEffect in statusEffectList) {
            if (!statusEffect.GetIsInfinite())
	            statusEffect.flexibleElem.display.remainPhase--;
            if (statusEffect.Duration() <= 0)
                RemoveStatusEffect(statusEffect);
        }
    }
    public Unit GetUnitOnTile (){
		return unitOnTile;
	}

	public string GetTileName(){
		return displayName;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
        if(!IsVisible()) return;
		
		var TM = TileManager.Instance;
		var UM = BattleUIManager.Instance;
		TM.mouseOverTile = this;
		TM.preSelectedMouseOverTile = isPreSelected ? this : null;
		isMouseOver = true;
		RenewColor ();

		if (IsUnitOnTile () && !unitOnTile.IsHiddenUnderFogOfWar ()) {
			ChainList.ShowChainByThisUnit (unitOnTile);
			ChainList.ShowUnitsTargetingThisTile (this, true);

			TM.mouseOverUnit = unitOnTile;
			UM.UpdateUnitViewer (unitOnTile);
			unitOnTile.SetMouseOverHighlightUnitAndPortrait (true);
		}else
			TM.mouseOverUnit = null;
        
		UM.SetTileViewer(this);
	}
    void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		clickStarted = false;
		CostAP.gameObject.SetActive(false);

		OnMouseExit ();

	    var UIM = BattleUIManager.Instance;
	    var TM = TileManager.Instance;

		if (IsUnitOnTile() && TM.mouseOverUnit == unitOnTile){
			ChainList.HideChainTilesDisplay ();
			ChainList.ShowUnitsTargetingThisTile (this, false);

			TM.mouseOverUnit = null;
			UIM.DisableUnitViewer();
			unitOnTile.SetMouseOverHighlightUnitAndPortrait (false);
		}

        UIM.DisableTileViewerUI();
	}
	public void UpdateRealPosition() {
		transform.position = TileManager.CalculateRealTilePosition(location, height);
	}
	
	bool clickStarted;
	float timeClickStarted;
	public UnityEvent LeftClickEnd;
	public UnityEvent LongLeftClickEnd;
	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData){
		if (pointerData.button == PointerEventData.InputButton.Left) {
			clickStarted = true;
			timeClickStarted = Time.time;
		}
	}
	void IPointerUpHandler.OnPointerUp(PointerEventData pointerData){
		if (!clickStarted || pointerData.button != PointerEventData.InputButton.Left) return;
		clickStarted = false;
		LeftClickEnd.Invoke();
	}
	void Update(){
		if (!clickStarted || Time.time - timeClickStarted <= Setting.longClickDurationThreshold) return;
		clickStarted = false;
		LongLeftClickEnd.Invoke();
	}

	void Awake (){
		colors = new List<Material>();
		spriteRenderersUnderTile = new List<SpriteRenderer>();
		OnMouseExit ();
		InitializeEvents ();
	}

	void InitializeEvents(){
		var BM = BattleManager.Instance;
		UnityAction UserSelectTile= () => {
			Debug.Log("Tile(" + location.x + "," + location.y + ") PreSelected : " + IsPreSelected());
			if (IsPreSelected() && !BattleData.shortClickLock){
				BM.OnMouseDownHandlerFromTile(location);
			}else{
				SoundManager.Instance.PlaySE ("Cannot");
			}
		};
		UnityAction UserLongSelectTile= () => {
			if (isPreSelected && !BattleData.longClickLock)
				BM.OnLongMouseDownHandlerFromTile (location);
			else
				BM.OnMouseDownHandlerFromTile(location);
		};
		LeftClickEnd.AddListener (UserSelectTile);
		LongLeftClickEnd.AddListener (UserLongSelectTile);
	}

    public void SetFogType(FogType fogType) {
	    if(this.fogType == fogType) return;
        this.fogType = fogType;
		ApplyFogType(fogType);
    }
	public void ApplyFogType(FogType fogType){
		// float alpha = Math.Min((int) fogType * 0.75f, 1);
		// fogOfWarPlaneMaterial.SetFloat("TileAlpha", alpha);
		float alpha = (int)fogType * 0.7f;
		float shadow = 1-alpha * 0.9f;
		Color shadowColor = new Color(shadow, shadow, shadow, 1);
		fogOfWarPlane.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
		GetComponent<SpriteRenderer>().color = shadowColor;
		foreach(var sr in spriteRenderersUnderTile)
			sr.color = shadowColor;
		foreach (var sr in highlightWall.GetComponentsInChildren<SpriteRenderer>())
			sr.enabled = fogType != FogType.Black;
	}

	/* Tile painting related */
	void OnMouseExit(){
		TileManager TM = TileManager.Instance;
		TM.mouseOverTile = null;
		TM.preSelectedMouseOverTile = null;
		isMouseOver = false;
		RenewColor ();
	}
	public void SetHighlight(ShowType showType){
		highlightWall.SetActive(showType != ShowType.Off);
		GetComponent<BlinkingObject>().enabled = showType == ShowType.Blink;
	}

	public void PaintTile(Material color) {
		colors.Add(color);
		RenewColor();
	}
	public void ClearTileColor(){
		colors.Clear();
		RenewColor();
	}
	public void DepaintTile(Material color){
		colors.RemoveAll(item => item == color);
		RenewColor();
	}
	public void SetBloody(bool input){
		if(isBloody != input)
			LogManager.Instance.Record(new BloodChangeLog(this, input));
	}
	public void RenewColor(){
		//Debug.Log("Renew TileColor");
		if (colors.Count == 0) tileColorSprite.enabled = false;
		else{
			var TM = TileManager.Instance;
			tileColorSprite.enabled = true;
			Material chainColor = TM.TileColorMaterialForChain;
			if (colors.Contains (chainColor)) {
				tileColorSprite.material = chainColor;
			}else if (colors.Contains(TM.TileColorMaterialForRange1) && colors.Contains(TM.TileColorMaterialForRange2))
				tileColorSprite.material = TM.TileColorMaterialForMixedRange;
			else
				tileColorSprite.material = colors.Last ();
		}

		blood.enabled = isBloody;
	}

	public bool HasTrap(){
		return statusEffectList.Any (se => se.IsTypeOf (StatusEffectType.Trap));
	}

	public void SetTrapImage(Sprite sprite){
		trapImage.sprite = sprite;
	}

	// override object.Equals
	public override bool Equals (object obj){		
		if (obj == null || GetType() != obj.GetType()) {return false;}
		var tileObj = (Tile)obj;
		return Location == tileObj.Location;
	}
	
	// override object.GetHashCode
	public override int GetHashCode(){
		return Location.x * 1000 + Location.y;
	}
}
