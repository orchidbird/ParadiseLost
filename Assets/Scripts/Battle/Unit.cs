using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class Unit : Entity{
	public GameObject damageTexts;
	GameObject recoverTextObject;
	GameObject apChangeTextObject;
	GameObject WillChangeTextObject;
	public GameObject activeArrowIcon;
	GameObject afterimage;
	public HealthViewer healthViewer;
	public TextMesh OrderText;
	GameObject chainAttackerIcon;
	public GameObject faintTurnSkipIcon;
	public HighlightBorder highlightBorder;
	OrderPortraitSlot orderPortraitSlot;

	private SpriteRenderer overrideImage;
	public GameObject modifierIcons;
	public List<GameObject> modIconInstances = new List<GameObject>();
	public GameObject evasionTextObject;
	List<HitInfo> latelyHitInfos;
	public ParticleSystem tileBuffEffect;
	public GameObject fearParticle;
	public SpriteRenderer specialObjectIcon;

	bool isAlreadyBehavedObject; //지형지물(오브젝트)일 때만 의미있는 값. 그 페이즈에 이미 행동했는가
	public bool criticalApplied; //치명타 보상을 이미 받았는지를 의미.
	public int movedTileCount;
	public float previousMoveCost;
	AI _AI;

	// 스킬리스트
	public Dictionary<WillCharacteristic, bool> WillCharacteristics = new Dictionary<WillCharacteristic, bool>();

	public string holdName;
	public string connectedName;
	public string nameKor;
	public Side side;
	public bool isObject;
	
	// 사용한 스킬 정보 저장(쿨타임 산정용)
	Dictionary<string, int> usedSkillDict = new Dictionary<string, int>();
	public Dictionary<Stat, int> actualStats = new Dictionary<Stat, int>();

	public Vector2 size = new Vector2(1, 1);
	Vector2Int pivot; //Variable한 위치 개념. IsLarge일 경우 가장 소속 타일 중 가장 x/y값이 작은 곳을 따름. 아니면 그냥 자기 위치.
	public void SetPivot(Vector2Int position){pivot = position;}
	public void SetInitialLocation(){
		var emptyTiles = TileManager.Instance.GetAllTiles().Values.ToList().FindAll(tile => !tile.IsUnitOnTile());
		if (IsAlly){
			var enemy = UnitManager.GetAllUnits().Find(item => !item.IsAlly);
			emptyTiles = emptyTiles.FindAll(tile => Calculate.Distance(enemy, tile, -1) >= 4); //적으로부터의 거리가 4타일 이상인 곳
		}
		SetPivot(Generic.PickRandom(emptyTiles).Location);
	}

	// 유닛이 해당 페이즈에서 처음 있었던 위치 - 영 패시브에서 체크
	Vector2Int startPositionOfPhase;

	//이 유닛이 이 턴에 움직였을 경우에 true - 큐리 스킬 '재결정'에서 체크
	public int notMovedTurnCount;

	//이 유닛이 이 턴에 스킬을 사용했을 경우에 true - 유진 스킬 '여행자의 발걸음', '길잡이'에서 체크
	bool hasUsedSkillThisTurn;

	Direction direction;	
	public int activityPoint;

	GameObject chargeEffect;
	public GameObject chargeEffectPrefab;
	public CustomWorldText DamageTextPrefab;
	public UnitInfo myInfo;

	public Dictionary<Direction, Sprite> Sprites = new Dictionary<Direction, Sprite>();
	public Dictionary<Direction, Sprite> SubSprites = new Dictionary<Direction, Sprite>();

	public List<HitInfo> GetLatelyHitInfos(){
		return latelyHitInfos;
	}

	public void SetChargeEffect(){
		if (chargeEffect != null) RemoveChargeEffect();
		chargeEffect = Instantiate(chargeEffectPrefab, transform);
		chargeEffect.transform.position = gameObject.transform.position - new Vector3(0, 0, 0.01f);
	}

	public void RemoveChargeEffect(){
		Destroy(chargeEffect);
	}

	public int GetStat(Stat stat){
		if (stat == Stat.Will)
			return Math.Max(actualStats[Stat.Will], 1);
		return actualStats.ContainsKey(stat) ? actualStats[stat] : 0;
	}

	public int GetBaseStat(Stat stat){
		return myInfo.baseStats.ContainsKey(stat) ? myInfo.baseStats[stat] : 0;
	}

	public float GetEvasionChance(){
		return HasStatusEffect(StatusEffectType.EvasionChange) ? CalculateActualAmount(0, StatusEffectType.EvasionChange) : 0;
	}

	public void ShowArrow(){
		activeArrowIcon.SetActive(true);
		highlightBorder.Activate (ColorList.CMY, 1.0f);
	}
	void HideArrow(){
		highlightBorder.InactiveBorder ();
		activeArrowIcon.SetActive(false);
	}

	public void PreviewChainTriggered(bool highlightOn){
		if(highlightBorder == null) return;
		if (highlightOn)	
			highlightBorder.Activate (ColorList.ForChain, 0.33f);
		else{
			highlightBorder.InactiveBorder ();
			DisplayChainIcon(false);
		}
	}

	public void SetMouseOverHighlightBorder(bool onoff){
		if (onoff)
			highlightBorder.HighlightWithBlackAndWhite();
		else 
			highlightBorder.InactiveBorder ();
	}

	public void SetMouseOverHighlightUnitAndPortrait(bool onoff){
		if (BattleData.turnUnit == this) return;
			
		SetMouseOverHighlightBorder (onoff);
		if (orderPortraitSlot != null)
			orderPortraitSlot.HighlightPortrait (onoff);
	}

	public bool IsAlreadyBehavedObject(){
		return isAlreadyBehavedObject;
	}

	public void SetAlreadyBehavedObject(bool input){
		isAlreadyBehavedObject = input;
	}

	public List<ActiveSkill> GetActiveSkillList() {return myInfo.skills.FindAll(skill => skill is ActiveSkill).ConvertAll(skill => (ActiveSkill)skill);}
	public List<PassiveSkill> GetPassiveSkillList(){return myInfo.skills.FindAll(skill => skill is PassiveSkill).ConvertAll(skill => (PassiveSkill)skill);}
	public List<ActiveSkill> ActiveSkillsToThink{get{
		var result = GetActiveSkillList().FindAll(IsThisSkillUsable);
		if (CodeName == "monk")
			result = result.FindAll(skill => skill.IsOffensive());
		return result;
	}}

	public ListPassiveSkillLogic listPassiveSkillLogic;
	public ListPassiveSkillLogic GetListPassiveSkillLogic(){
		return listPassiveSkillLogic ?? (listPassiveSkillLogic = SkillLogicFactory.Get(GetPassiveSkillList()));
	}

	// 효과 리스트
	public SkillAndChainStates.PreviewState preview;
	public int hp;
	public int GetHP{get{
		if (preview != null)
			return (int)Math.Round(preview.unitHp);
		return hp;
	}}
	public List<UnitStatusEffect> statusEffectList = new List<UnitStatusEffect>();
	public List<UnitStatusEffect> VaildStatusEffects{get { return statusEffectList.FindAll(se => se.IsValid); }}
	public UnitStatusEffect GetSEofDisplayNameKor(string input){return statusEffectList.Find(se => se.DisplayName(true) == input);}

	public void SetStatusEffectList(List<UnitStatusEffect> newStatusEffectList){
		statusEffectList = newStatusEffectList;
	}

	public int GetMaxHealth(){
		return actualStats[Stat.MaxHealth];
	}

	public float GetHpRatio(bool subtractRetreat = false){
		if(subtractRetreat)
			return Math.Max((GetHP - RetreatHP) / (float) GetMaxHealth(), 0.001f);
		else
			return GetHP / (float) GetMaxHealth();
	}

	public float GetApRatio(int number){
		float ratio = (float)number / (GetStat(Stat.Agility) + ApDeletePoint);
		return Math.Min(ratio, 1);
	}

	public int GetCurrentActivityPoint(){
		return activityPoint;
	}

	public Tile TileUnderUnit{
		get { return TileManager.Instance.GetTile(pivot); }
	}

	public List<Tile> TilesUnderUnit{get{
		var result = new List<Tile>();
		if (IsLarge){
			for (int x = 0; x < size.x; x++)
				for (int y = 0; y < size.y; y++){
					if (direction == Direction.RightDown || direction == Direction.LeftUp)
						result.Add(TileManager.Instance.GetTile(pivot + new Vector2Int(x, y)));
					else
						result.Add(TileManager.Instance.GetTile(pivot + new Vector2Int(y, x)));
				}
		}else
			result.Add(TileUnderUnit);
		
		if(result.Contains(null)) Debug.LogError(CodeName + "의 위치가 이상함!");
		return result;
	}}

	public int GetHeight()
	{
		return TileUnderUnit.GetHeight();
	}

	public string CodeName{get { return myInfo.codeName; }}

	public string GetNameKor(){
		return nameKor;
	}

	public Side GetSide() {return side;}

	public Side GetEnemySide(){
		return side == Side.Ally ? Side.Enemy : Side.Ally;
	}

	public List<Unit> GetAllies{get { return UnitManager.GetAllUnits().FindAll(unit => unit.IsAllyTo(this)); }}
	public List<Unit> GetEnemies{get{return UnitManager.GetAllUnits().FindAll(unit => unit.IsEnemyTo(this));}}
	
	public bool IsAllyTo(Unit unit) {return side == unit.GetSide();}
	public bool IsEnemyTo(Unit unit){
		return (side == Side.Ally && unit.GetSide() == Side.Enemy) ||
		       (side == Side.Enemy && unit.GetSide() == Side.Ally);
	}

	public bool IsSeenAsEnemyToThisAIUnit(Unit unit)
	{
		return AIUtil.IsSecondUnitEnemyToFirstUnit(unit, this);
	} //은신 상태에선 적으로 인식되지 않음

	public void SetAI(AI _AI)
	{
		this._AI = _AI;
	}

	public AI GetAI()
	{
		return _AI;
	}

	//지형지물은 AI로 분류되지 않으므로 PC인지 확인하려면 !IsAI가 아니라 IsPC(=!isAI && !isObject로 아래에 get 함수로 있음)의 return 값을 받아야 한다
	public bool IsAI{get { return _AI != null; }} 

	public bool IsAlly{get { return myInfo.isAlly; }}
	public bool IsPC{
		get { return !IsAI && (!isObject); }
	}

	public bool IsAllyNPC
	{
		get { return GetSide() == Side.Ally && IsAI; }
	}

	public bool IsObject{get { return isObject; }}
	public ObjectTag objectTag;

	bool IsKillable{get { return !IsObject; }}

	public bool HasAction{
		get { return IsAI || !IsObject; }
	}

	public bool RetreatBefore0HP{
		get { return VolatileData.OpenCheck(Setting.retreatOpenStage) && !IsObject; }
	}

	public Dictionary<WillChangeType, int> willHistoryDict = new Dictionary<WillChangeType, int>();
	public bool HasWillCharacteristic(WillCharacteristic c){
		if (!VolatileData.OpenCheck(Setting.WillCharacteristicOpenStage)) return false;
		
		if (WillCharacteristics.ContainsKey(c))
			return WillCharacteristics[c];

		Debug.Log(name + " doesn't have data of WillCharacteristic " + c);
		return false;
	}

	//public Vector2 Pos{get{return pivot + myInfo.size - new Vector2(1, 1);}}

	public bool IsLarge{get{return size != new Vector2(1, 1);}}

	public Vector3 realPosition{
		get { return transform.position; }
	}

	public Vector2Int GetStartPositionOfPhase()
	{
		return startPositionOfPhase;
	}

	public bool GetHasUsedSkillThisTurn(){
		return hasUsedSkillThisTurn;
	}

	public void SetHasUsedSkillThisTurn(bool hasUsedSkillThisTurn)
	{
		this.hasUsedSkillThisTurn = hasUsedSkillThisTurn;
	}

	public Dictionary<string, int> GetUsedSkillDict()
	{
		return usedSkillDict;
	}

	public Direction GetDir()
	{
		return direction;
	}

	public void SetDirection(Direction direction){
		this.direction = direction;
		UpdateSpriteByDirection();
	}

	public bool HasEnoughAPToUseSkill(ActiveSkill skill){
		return GetCurrentActivityPoint() >= GetActualRequireSkillAP(skill);
	}

	public bool IsMovePossibleState(){
		return (!HasStatusEffect(StatusEffectType.Bind) && !HasStatusEffect(StatusEffectType.Faint));
	}

	public bool HasAnyWayToMove(){
		TileManager tileManager = TileManager.Instance;
		if (!IsMovePossibleState())
			return false;
		bool hasAnyWay = false;
		foreach (var direction in EnumUtil.directions)
		{
			Tile nearbyTile = tileManager.GetTile(Pos + Utility.DirToV2I(direction));
			if (tileManager.isTilePassable(this, nearbyTile, true) &&
			    TileWithPath.NewTileMoveCost(nearbyTile, TileUnderUnit, 0, this) < GetCurrentActivityPoint())
				hasAnyWay = true;
		}

		return hasAnyWay;
	}

	public bool IsSkillUsePossibleState(){
		Tile tileUnderCaster = TileUnderUnit;
		foreach (var tileStatusEffect in tileUnderCaster.GetStatusEffectList()){
			Skill originSkill = tileStatusEffect.GetOriginSkill();
			if (!(originSkill is ActiveSkill)) continue;
			if (!((ActiveSkill) originSkill).SkillLogic.TriggerTileStatusEffectWhenUnitTryToUseSkill(tileUnderCaster,
				tileStatusEffect))
				return false;
		}

		return !(HasStatusEffect(StatusEffectType.Silence) || HasStatusEffect(StatusEffectType.Faint));
	}

	public bool IsThisSkillUsable(ActiveSkill skill)
	{
		return IsSkillUsePossibleState() && HasEnoughAPToUseSkill(skill) && !usedSkillDict.ContainsKey(skill.GetName());
	}

	public bool HasAnySkillToCast(){
		return IsSkillUsePossibleState() && GetActiveSkillList().Any(IsThisSkillUsable);
	}

	public bool HasOnlyFriendlySkill(){
		return GetActiveSkillList().All (skill => skill.IsFriendly ());
	}

	public void ChangePosition(List<Tile> path, bool forced = false, bool charge = false){
		LogManager.Instance.Record(new PositionChangeLog(this, path, forced, charge));
		BreakChain ();
		GetListPassiveSkillLogic().TriggerOnMove(this);
		foreach (var statusEffect in statusEffectList){
			var originPassiveSkill = statusEffect.GetOriginSkill();
			if (originPassiveSkill is PassiveSkill)
				((PassiveSkill) originPassiveSkill).SkillLogic.TriggerStatusEffectsOnMove(this, statusEffect);
		}
	}

	public void RemoveFogsInSight(){
		if (GetSide() != Side.Ally) return;
		foreach (var tile in TilesInSight)
			tile.SetFogType(FogType.None);
	}

	public bool CanBeForcedToMove(){
		return !IsObject && !HasStatusEffect(StatusEffectType.ForceMoveImmune);
	}

	public void ForceMove(List<Tile> path, bool forced = true, bool charge = false){
		//강제이동
		List<Tile> plannedPath = path;
		List<Tile> realPath = new List<Tile>();
		bool trapOperated = false;
		if (!CanBeForcedToMove()) return;
		
		foreach (var tile in plannedPath){
			realPath.Add(tile);
			List<TileStatusEffect> traps = TileManager.Instance.FindTrapsOnThisTile(tile);
			foreach (var trap in traps)
				if (GetListPassiveSkillLogic().TriggerOnSteppingTrap(this, trap))
					trapOperated = true;

			if (trapOperated) break;
		}

		ChangePosition(realPath, forced, charge);
	}

	public void ApplyMove(TileWithPath path){
		//Debug.Log(path.dest.Pos + "로 이동");
		LogManager.Instance.Record(new MoveLog(this, path));

		foreach (var statusEffect in statusEffectList)
			if ((statusEffect.IsTypeOf(StatusEffectType.RequireMoveAPChange) ||
			     statusEffect.IsTypeOf(StatusEffectType.WillChange)) && statusEffect.GetIsOnce())
			{
				RemoveStatusEffect(statusEffect);
			}

		GetListPassiveSkillLogic().TriggerAfterMove(this, TileUnderUnit, path.dest);
		if(path.fullPath.Any(tile => tile.GetHeight() > 0))
			TutorialManager.Instance.Activate("Height");
	}

	public Dictionary<Vector2Int, TileWithPath> MovableTilesWithPath{
		get{
			//유닛의 특성 중에 이동 가능한 경로들을 바꾸는 능력이 있는지 보고, 없다면 이동 가능한 경로들을 원래 방법대로 구한다. 
			var result = GetListPassiveSkillLogic().GetMovablePath(this) ?? PathFinder.CalculatePaths(this, false);
			result.Remove(Pos);
			return result;
		}
	}

	public List<Tile> MovableTiles{get { return MovableTilesWithPath.Keys.ToList().ConvertAll(vec2 => TileManager.Instance.GetTile(vec2)); }}
	public void UpdateStats(){
		float beforePercentage = GetHP / (float) GetMaxHealth();
		foreach (var stat in EnumUtil.statsToUpdate)
			actualStats[stat] = (int)Math.Round(CalculateThroughChangeList(GetBaseStat(stat), GetStatChangeList(stat)));
		hp = (int) Math.Round(GetMaxHealth() * beforePercentage);
	}

	public void UpdateHealthViewer(){
		if (healthViewer == null) return;
		healthViewer.gameObject.SetActive(true);
		healthViewer.UpdatecurrentHealth(GetHP, GetRemainShield(), GetMaxHealth());
		healthViewer.HpBarIcon.sprite = _Sprite.GetIcon(DieAtTurnStart && HasAction ? "AutoDie" : "None");
		CheckAndHideObjectHealth();
	}

	public void UpdateRealPosition(){
		var pos = Utility.AveragePos(TilesUnderUnit);
		pos.z = TilesUnderUnit.Min(tile => tile.transform.position.z) - 0.05f;
		transform.position = pos;
		UpdateSpriteByDirection();
	}

	public void ElapseSkillCoolDown(){
		Dictionary<string, int> newUsedSkillDict = new Dictionary<string, int>();
		foreach (var skill in usedSkillDict)
		{
			int updatedCooldown = skill.Value - 1;
			if (updatedCooldown > 0)
				newUsedSkillDict.Add(skill.Key, updatedCooldown);
		}

		usedSkillDict = newUsedSkillDict;
	}

	void ElapseStatusEffectPhase(){
		foreach (var statusEffect in statusEffectList){
			if (!statusEffect.GetIsInfinite())
				statusEffect.flexibleElem.display.remainPhase--;
			if (statusEffect.Duration() == 0)
				RemoveStatusEffect(statusEffect);
		}
	}

	public bool HasSkillOfKorName(string skillName){
		return GetActiveSkillList().Any(skill => skill.korName == skillName) ||
		       GetPassiveSkillList().Any(skill => skill.korName == skillName);
	}
	// searching certain StatusEffectType
	public bool HasStatusEffect(StatusEffectType statusEffectType){
		bool hasStatusEffect = statusEffectList.Any(se => se.actuals.Any(elem => elem.statusEffectType == statusEffectType));
		return hasStatusEffect;
	}
	public void RemoveStatusEffect(UnitStatusEffect statusEffect){
		if (statusEffect == null) return;
		GetListPassiveSkillLogic ().TriggerOnStatusEffectRemoved (statusEffect, this);
		if (statusEffect.IsAura ())
			Aura.TriggerOnAuraRemoved (this, statusEffect);
		
		var originSkill = statusEffect.GetOriginSkill();
		if (originSkill == null || originSkill is PassiveSkill
			|| ((ActiveSkill) originSkill).SkillLogic.TriggerUnitStatusEffectRemoved(statusEffect, this))
			LogManager.Instance.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Remove, 0, 0, 0));
	}

	// 해당 category의 statusEffect를 num개 까지 제거
	// 성공적으로 제거한 statusEffect들의 개수를 리턴
	public int RemoveStatusEffect(Unit caster, StatusEffectCategory category, int num){
		int count = 0;
		foreach (var statusEffect in statusEffectList){
			if (count == num) break;
			// 자신이 자신에게 건 효과는 스스로 해제할 수 없다 - 기획문서 참조
			if (caster == this && statusEffect.GetCaster() == this || !statusEffect.GetIsRemovable()) continue;
			bool matchIsBuff = (category == StatusEffectCategory.Buff) && statusEffect.IsBuff;
			bool matchIsDebuff = (category == StatusEffectCategory.Debuff) && statusEffect.IsDebuff;
			bool matchAll = (category == StatusEffectCategory.All);
			
			if (!matchIsBuff && !matchIsDebuff && !matchAll) continue;
			RemoveStatusEffect(statusEffect);
			count++;
		}

		return count;
	}

	public class ValueChange{
		public bool isMultiply;
		public float value;
		public string reason;

		public ValueChange(bool isMultiply, float value, string reason = "")
		{
			this.isMultiply = isMultiply;
			this.value = value;
			this.reason = reason;
		}
	}

	public List<ValueChange> GetStatChangeList(Stat statType){
		var result = new List<ValueChange>();
		if (statType == Stat.Will && !VolatileData.OpenCheck(Setting.WillChangeOpenStage)) return result;

		var statusEffectType = EnumConverter.ToStatusEffectType(statType);

		// 효과에 의한 변동값 계산
		foreach (var statusEffect in statusEffectList){
			for (int i = 0; i < statusEffect.actuals.Count; i++){
				if (!statusEffect.IsOfType(i, statusEffectType)) continue;
				var amount = statusEffect.GetAmount(i);
				if (statusEffect.GetIsPercent(i))
					amount = amount / 100;
				result.Add(new ValueChange(statusEffect.GetIsMultiply(i), amount, statusEffect.GetOriginSkillName()));
			}

			// TileStatusEffect로 인한 변동값 계산
			var tile = TileUnderUnit;
			foreach (var tileStatusEffect in tile.GetStatusEffectList()){
				for (int i = 0; i < tileStatusEffect.actuals.Count; i++){
					if (tileStatusEffect.IsOfType(i, statusEffectType))
						result.Add(new ValueChange(tileStatusEffect.GetIsMultiply(i), tileStatusEffect.GetAmount(i),
							tileStatusEffect.DisplayName(false)));
				}
			}
		}

		if(statType == Stat.Will)
			foreach (var kv in willHistoryDict)
				result.Add(new ValueChange(false, kv.Value, Language.Find("WillChangeType_" + kv.Key)));
		return result;
	}

	public float AttackableDistance{get{
		return HasOnlyFriendlySkill() ? 0
			: GetActiveSkillList().FindAll(skill => !skill.IsFriendly()).Max(skill => AIUtil.RangeDistanceOfAttack(skill));
	}}

	public float CalculateActualAmount(float baseValue, StatusEffectType statusEffectType){
		var appliedChangeList = new List<ValueChange>();

		// 특성에 의한 변동값 계산
		var passiveSkillLogics = GetListPassiveSkillLogic();
		if (statusEffectType == StatusEffectType.PowerChange)
			appliedChangeList.Add(new ValueChange(true, passiveSkillLogics.GetAdditionalRelativePowerBonus(this) - 1));
		if (statusEffectType == StatusEffectType.DefenseChange)
			appliedChangeList.Add(new ValueChange(false, passiveSkillLogics.GetAdditionalAbsoluteDefenseBonus(this)));

		// 효과로 인한 변동값 계산
		foreach (var statusEffect in statusEffectList){
			for (int i = 0; i < statusEffect.actuals.Count; i++)
			{
				if (statusEffect.IsOfType(i, statusEffectType))
				{
					float amount = statusEffect.GetAmount(i);
					if (statusEffect.GetIsPercent(i))
						amount = amount / 100;
					appliedChangeList.Add(new ValueChange(statusEffect.GetIsMultiply(i), amount));
				}
			}

			// TileStatusEffect로 인한 변동값 계산
			Tile tile = TileUnderUnit;
			foreach (var tileStatusEffect in tile.GetStatusEffectList())
				for (int i = 0; i < tileStatusEffect.actuals.Count; i++)
					if (tileStatusEffect.IsOfType(i, statusEffectType))
						appliedChangeList.Add(new ValueChange(tileStatusEffect.GetIsMultiply(i), tileStatusEffect.GetAmount(i)));
		}

		return CalculateThroughChangeList(baseValue, appliedChangeList);
	}

	float CalculateThroughChangeList(float baseValue, List<ValueChange> appliedChangeList){
		float totalAdditiveValue = 0.0f;
		float totalMultiplicativeValue = 1.0f;
		foreach (var change in appliedChangeList)
			if (change.isMultiply)
				totalMultiplicativeValue *= 1 + change.value;
			else
				totalAdditiveValue += change.value;

		return baseValue * totalMultiplicativeValue + totalAdditiveValue;
	}

	public int GetRemainShield(){
		float remainShieldAmount = 0;
		foreach (var statusEffect in statusEffectList)
			remainShieldAmount += statusEffect.GetAmountOfType(StatusEffectType.Shield);

		return (int) Math.Round(remainShieldAmount);
	}

	public void UpdateStartPosition(){
		startPositionOfPhase = Pos;
		notMovedTurnCount++;
		hasUsedSkillThisTurn = false;
	}

	public void ApplyDamageByCasting(CastingApply castingApply, bool isHealth, bool duringAIDecision){
		Unit caster = castingApply.Caster;
		bool ignoreShield = SkillLogicFactory.Get(castingApply.GetSkill()).IgnoreShield(castingApply);

		// 대상에게 스킬로 데미지를 줄때 발동하는 공격자 특성
		var passiveSkillsOfAttacker = caster.GetPassiveSkillList();
		SkillLogicFactory.Get(passiveSkillsOfAttacker).TriggerActiveSkillDamageAppliedByOwner(caster, this);

		ApplyDamage(CalculateDamageByCasting(castingApply, isHealth, duringAIDecision), caster, isHealth, ignoreShield, castingApply.GetSkill(), duringAIDecision);
		if (!caster.IsPC) return;
		if(castingApply.GetDamage().relativeModifiers.Any(kv => kv.Key.name == "Direction" && kv.Value > 1))
			TutorialManager.Instance.Activate("Direction");
		if(castingApply.GetDamage().relativeModifiers.Count(kv => (kv.Key.name == "Direction" || kv.Key.name == "Height") && kv.Value > 1) > 1)
			TutorialManager.Instance.Activate("Critical");
		if(castingApply.Target.GetStat(Stat.Defense) > 150)
			TutorialManager.Instance.Activate("Defense");
	}

	public DamageCalculator.AttackDamage CalculateDamageByCasting(CastingApply castingApply, bool isHealth, bool duringAIDecision){
		Unit caster = castingApply.Caster;
		ActiveSkill appliedSkill = castingApply.GetSkill();
		DamageCalculator.AttackDamage damage = castingApply.GetDamage();
		if (isHealth){
			if (!duringAIDecision) // AI는 반사에 의한 피해 경감은 고려하지 않음
				damage.resultDamage -= DamageCalculator.CalculateReflectDamage (damage.resultDamage, this, caster);

			damage.resultDamage = CalculateActualAmount(damage.resultDamage, StatusEffectType.TakenDamageChange);
			float defense = DamageCalculator.CalculateDefense(appliedSkill, this, caster);
			damage.resultDamage = DamageCalculator.ApplyDefense(damage.resultDamage, defense);
		}

		//Debug.Log(castingApply.Caster.CodeName + "이 발동한 공격 피해량은 " + damage.resultDamage);
		return damage;
	}

	public void ApplyDamageByNonCasting(float originalDamage, Unit caster, bool isHealth, float additionalDefense = 0, bool ignoreShield = false){
		int realDamage = CalculateDamageByNonCasting(originalDamage, caster, additionalDefense, isHealth);
		DamageCalculator.AttackDamage damage = new DamageCalculator.AttackDamage(realDamage);
		ApplyDamage(damage, caster, isHealth, ignoreShield, null, false, false);
	}

	public int CalculateDamageByNonCasting(float originalDamage, Unit caster, float additionalDefense, bool isHealth){
		float damage = originalDamage;
		if (isHealth){
			// 피격자의 효과/특성으로 인한 대미지 증감 효과 적용 - 미완성
			damage = CalculateActualAmount(damage, StatusEffectType.TakenDamageChange);
			// 방어력 및 저항력 적용
			float defense = GetStat(Stat.Defense) + additionalDefense;
			damage = DamageCalculator.ApplyDefense(damage, defense);
		}

		return (int) Math.Round(damage);
	}

	//기술 정보를 전달해야 하므로 기존 bool byCasting을 skill != null로 대체함(18.10.04)
	void ApplyDamage(DamageCalculator.AttackDamage damage, Unit caster, bool isHealth, bool ignoreShield, ActiveSkill skill, bool duringAIDecision, bool waitTime = true){
		var LM = LogManager.Instance;
		if (isHealth){
			// 보호막 차감(먼저 적용된 것 우선).
			Dictionary<UnitStatusEffect, int> attackedShieldDict = new Dictionary<UnitStatusEffect, int>();
			List<UnitStatusEffect> removedShields = new List<UnitStatusEffect>();
			if (!ignoreShield){
				foreach (var se in statusEffectList){
					int num = se.actuals.Count;
					for (int i = 0; i < num; i++){
						if (se.GetStatusEffectType(i) != StatusEffectType.Shield) continue;
						
						int shieldAmount = (int) Math.Round(se.GetAmount(i));
						if (shieldAmount >= damage.resultDamage){
							se.SubAmount(i, damage.resultDamage);
							attackedShieldDict.Add(se, (int) damage.resultDamage);
							damage.resultDamage = 0;
						}else{
							se.SubAmount(i, shieldAmount);
							attackedShieldDict.Add(se, shieldAmount);
							removedShields.Add(se);
							RemoveStatusEffect(se);
							damage.resultDamage -= shieldAmount;
						}
					}

					if (damage.resultDamage == 0) break;
				}
			}

			LM.Record(new HPChangeLog(this, caster, damage));
			LM.Record(new DisplayDamageTextLog(this, damage, true));

			if (damage.resultDamage > 0){
				HitInfo hitInfo = new HitInfo(caster, skill, (int) damage.resultDamage);
				LM.Record(new AddLatelyHitInfoLog(this, hitInfo));
				BreakCollecting();
				if(VolatileData.difficulty != Difficulty.Intro)
					BreakChain ();

				// 데미지를 받은 후에 발동하는 피격자 특성
				GetListPassiveSkillLogic().TriggerAfterDamaged(this, (int) damage.resultDamage, caster);
				if (skill != null)
					GetListPassiveSkillLogic ().TriggerAfterDamagedByCasting (this, caster);

				ChangeWill(WillChangeType.Pain, damage.resultDamage * 5 >= GetMaxHealth() ? BigSmall.Big : BigSmall.Small);
			}

			foreach (var kv in attackedShieldDict){
				var statusEffect = kv.Key;
				var originSkill = statusEffect.GetOriginSkill();
				if (originSkill is ActiveSkill)
					((ActiveSkill) originSkill).SkillLogic.TriggerShieldAttacked(this, kv.Value);
				var shieldCaster = statusEffect.GetCaster();
				if (skill != null){
					SkillLogicFactory.Get(shieldCaster.GetPassiveSkillList())
						.TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(caster, shieldCaster, this, kv.Value, duringAIDecision);
				}
			}

			foreach (var shield in removedShields){
				var shieldCaster = shield.GetCaster();
				SkillLogicFactory.Get(shieldCaster.GetPassiveSkillList()).TriggerWhenShieldExhaustedByDamage(this, shieldCaster);
			}
		}else
			ChangeAP(-(int) damage.resultDamage);
		
		if(waitTime)
			LM.Record(new WaitForSecondsLog(Configuration.textObjectDuration));
	}

	public void DeactivateDamageTextObject(){
		float duration = Configuration.textObjectDuration;
		foreach (Transform damageText in damageTexts.transform)
			StartCoroutine(DisappearTextObject(damageText.gameObject, duration, true));
		StartCoroutine(DisappearTextObject(recoverTextObject, duration, false));
	}

	public IEnumerator DisplayDamageText(DamageCalculator.AttackDamage damage, bool remain){
		var damageTextObject = Instantiate(DamageTextPrefab, damageTexts.GetComponent<RectTransform>());
		damageTextObject.GetComponent<Canvas>().overridePixelPerfect = true;
		damageTextObject.GetComponent<CustomWorldText>().text = ((int)damage.resultDamage).ToString();
		damageTextObject.GetComponent<CustomWorldText>().ApplyText(CustomWorldText.Font.DAMAGE, Color.white, damage.TotalRelativeModifier, damage.IsCritical);
		yield return CommonPartOfDisplayText(damageTextObject.gameObject, remain, isDamageText: true);
	}

	public IEnumerator DisplayRecoverText(int recover){
		recoverTextObject.GetComponent<CustomWorldText>().text = recover.ToString();
		recoverTextObject.GetComponent<CustomWorldText>().ApplyText(CustomWorldText.Font.RECOVER, Color.green);

		recoverTextObject.SetActive(true);
		yield return CommonPartOfDisplayText(recoverTextObject);
		recoverTextObject.SetActive(false);
	}

	public IEnumerator DisplayAPChangeText(int amount){
		var CustomText = apChangeTextObject.GetComponent<CustomWorldText>();
		CustomText.text = (amount > 0 ? "+" : "") + amount;
		CustomText.ApplyText(CustomWorldText.Font.AP, Color.white);

		apChangeTextObject.SetActive(true);
		yield return CommonPartOfDisplayText(apChangeTextObject);
		apChangeTextObject.SetActive(false);
	}

	// Text 움직임을 여러 가지 파라미터로 조정할 수 있는 함수
	// Slide, Resize, Fade를 한꺼번에 시키며, 각각의 성질을 파라미터로 조정할 수 있다.
	IEnumerator CommonPartOfDisplayText(GameObject textObject, bool waitTime = true, float duration = -1, bool isDamageText = false,
		/* Slide 관련 parameter */	Vector3? position = null, bool slideFrom =  true, 
		/* resize 관련 parameter*/	Vector2? size = null, bool resizeFrom = true,
		/* fade 관련 parameter  */	bool fade = true, bool fadeIn = true) {
		if(duration < 0)
			duration = Configuration.textObjectDuration;
		if (position == null)
			position = new Vector3(0, -0.2f, 0);
		if (size == null)
			size = new Vector2(0.5f, 0.5f);

		foreach (var character in textObject.GetComponent<CustomWorldText>().characterInstances) {
			if(fade)
				StartCoroutine(Utility.Fade(graphic: character.GetComponent<Image>(), duration: duration / 2, fadeIn: fadeIn));
			StartCoroutine(Utility.Resize(character.GetComponent<RectTransform>(), (Vector2)size, duration / 2,
				from: resizeFrom, relative: true));
			StartCoroutine(UI.SlideObject(character.transform, (Vector3)position, duration / 2, from: slideFrom, relative: true));
		}
		
		if (waitTime) yield return new WaitForSeconds(Configuration.TextDisplayWaitTime);
			
		yield return DisappearTextObject(textObject, duration, isDamageText);
		UpdateHealthViewer();
	}

	IEnumerator DisappearTextObject(GameObject textObject, float duration, bool isDamageText) {
		if (textObject == null || !textObject.activeSelf) yield break;
		
		List<GameObject> characters = textObject.GetComponent<CustomWorldText>().characterInstances;
		foreach (var character in characters){
			var image = character.GetComponent<Image>();
			StartCoroutine(Utility.Fade(graphic: image, duration: duration / 2, fadeIn: false, useMaterial: isDamageText && character.name != "Critical"));
		}
		yield return new WaitForSeconds(duration / 2);
		if (isDamageText){
			Destroy(textObject);
		}else if (textObject != null) //유닛을 파괴할 때 textObject를 삭제하므로 null일 수도 있음.
		{
			textObject.SetActive(false);
		}
	}

	Queue<int> WillChangeAmounts = new Queue<int>();

	public void EnqueueWillChangeText(int amount)
	{
		WillChangeAmounts.Enqueue(amount);
	}

	IEnumerator DisplayNextWillChangeText(int amount)
	{
		Vector3? position = new Vector3(0, 0, 0);
		if (amount > 0)
		{
			WillChangeTextObject.GetComponent<CustomWorldText>().text = "+" + amount;
			position = new Vector3(0, 0.15f, 0);
		}
		else if (amount < 0)
		{
			WillChangeTextObject.GetComponent<CustomWorldText>().text = amount.ToString();
			position = new Vector3(0, -0.15f, 0);
		}

		WillChangeTextObject.GetComponent<CustomWorldText>().ApplyText(CustomWorldText.Font.Will, Color.white);
		WillChangeTextObject.SetActive(true);
		yield return CommonPartOfDisplayText(WillChangeTextObject,  position: position, slideFrom: false, 
							fade: false, size: new Vector2(1, 1));
		yield return CommonPartOfDisplayText(WillChangeTextObject, position: position, slideFrom: false,
							fadeIn: false, resizeFrom: false);

		WillChangeTextObject.SetActive(false);
	}

	public IEnumerator DisplayEvasionText(){
		evasionTextObject.SetActive(true);
		yield return Utility.WaitTime(Configuration.textObjectDuration);
		evasionTextObject.SetActive(false);
	}

	public void RecoverHealth(float amount, Unit healer){
		if (IsObject) return;
		int maxHealth = GetMaxHealth();
		// 회복량 증감 효과 적용
		amount = CalculateActualAmount(amount, StatusEffectType.TakenHealChange);

		// 초과회복시 최대체력까지만 올라감
		int actualAmount = (int) Math.Round(amount);
		if (GetHP + actualAmount > maxHealth)
			actualAmount = maxHealth - GetHP;

		if (actualAmount <= 0) return;
		LogManager.Instance.Record(new HPChangeLog(this, healer, amount: actualAmount));
		LogManager.Instance.Record(new DisplayDamageTextLog(this, new DamageCalculator.AttackDamage(-actualAmount), true));

		if (this == healer) return;
		healer.ChangeWill(WillChangeType.Saviour, BigSmall.None);
		ChangeWill(WillChangeType.Cooperative, BigSmall.None);
	}

	public void ChangeAP(int amount){
		if (IsObject) return;
		LogManager.Instance.Record(new APChangeLog(this, amount));
		LogManager.Instance.Record(new DisplayDamageTextLog(this, new DamageCalculator.AttackDamage(-amount), false));
	}

	public void RegenerateActionPoint(){
		if (!isObject && IsActive)
			activityPoint += GetStat(Stat.Agility);
	}
	public bool IsActive{get{return !IsAI || GetAI().IsActive;}}
	public bool CanAffectChainPlan(List<Unit> targets){//AI의 계산 참고용.
		return IsActive && _String.GeneralName(CodeName) != "holder" && !DieAtTurnStart && (!IsAI || targets.Any(target => GetAI().GetTargetValue(target) < 0));
	}
	bool DieAtTurnStart{get { return TotalDamageOverTime > 0 && GetHP + GetRemainShield() - TotalDamageOverTime <= RetreatHP; }}

	public int GetActualRequireSkillAP(ActiveSkill skill){
		int requireSkillAP = skill.GetRequireAP();
		requireSkillAP = skill.SkillLogic.GetRealAPWithOverload (requireSkillAP, this);

		// 기술 자체에 붙은 행동력 소모 증감효과 적용
		requireSkillAP = SkillLogicFactory.Get(skill).CalculateAP(requireSkillAP, this);

		// 기술의 행동력 소모 증감 효과 적용
		if (HasStatusEffect(StatusEffectType.RequireSkillAPChange))
			requireSkillAP = (int) Math.Round(CalculateActualAmount(requireSkillAP, StatusEffectType.RequireSkillAPChange));

		if (VolatileData.OpenCheck(Setting.WillChangeOpenStage)){
			float Will = GetStat(Stat.Will);
			requireSkillAP = (int) Math.Round(requireSkillAP * (100f / Will));
		}

		// 스킬 시전 유닛의 모든 행동력을 요구하는 경우
		if (skill.GetRequireAP() == 1000)
			requireSkillAP = activityPoint;
		return requireSkillAP;
	}

	// 아래 - AI용 함수들
	public float GeneralTargetValue{
		get{
			var PCFactor = (IsPC && VolatileData.difficulty != Difficulty.Intro) ? 1.5f : 1;
			var HpFactor = VolatileData.difficulty != Difficulty.Intro ? GetHpRatio(true) : 1;
			var PowerFactor = VolatileData.difficulty == Difficulty.Legend ? GetStat(Stat.Power) : 1;
			var ChainFactor = ChainList.GetUnitsTargetingThisTile(TileUnderUnit).Count + 1;
			var ChainBreakFactor = ChainList.GetChainList().Any(chain => chain.Caster == this) ? 2 : 1;
			return PCFactor * PowerFactor * ChainFactor * ChainBreakFactor / HpFactor;
		}
	}

	float CalculateFloatKillNeedCount(Casting casting){
		CastingApply castingApply = new CastingApply(casting, this);
		List<Chain> allTriggeredChains = ChainList.GetAllChainTriggered(casting);
		int chainCombo = allTriggeredChains.Count;
		DamageCalculator.CalculateAttackDamage(castingApply, chainCombo);
		// 여기서도 duringAICalculation을 true로 하면 반사로 인한 피해경감을 생각 안 해서 장애물 부수는데 드는 횟수를 잘못 계산할 것이므로 false로 함
		int damage = (int) CalculateDamageByCasting(castingApply, true, false).resultDamage; 
		int remainHP = GetHP + GetRemainShield();
		if (RetreatBefore0HP)
			remainHP -= RetreatHP;

		damage = Math.Min(damage, remainHP);
		return damage >= 1 ? remainHP / damage : 10000;
	}

	public int CalculateIntKillNeedCount(Casting casting){
		return (int) CalculateFloatKillNeedCount(casting);
	}

	public void CalculateBestCasting(){
		Casting bestCasting = null;
		float maxReward = 0;
		foreach (ActiveSkill skill in GetActiveSkillList()){
			if (!IsThisSkillUsable(skill))
				continue;
			skill.CalculateBestCasting(this, TileUnderUnit);
			if (AI.bestCasting != null){
				float reward = AI.bestCasting.GetReward();
				if (reward > maxReward)
				{
					maxReward = reward;
					bestCasting = AI.bestCasting;
				}
			}
		}

		AI.bestCasting = bestCasting;
	}
	// 위 - AI용 함수들

	public void UseActivityPoint(int amount)
	{
		LogManager.Instance.Record(new APChangeLog(this, -amount));
	}

	public void CollectNearestCollectable(){
		var collecting = new UnitStatusEffect(BattleData.collectingStatusEffectInfo, this, this);
		collecting.SetRemainPhase(BattleData.NearestCollectable.phase + 1); //수집하는 순간 턴을 종료하면서 한 번 차감하므로 시작할 때 +1을 해줘야 함
		collecting.SetObjectBeingCollected(BattleData.NearestCollectable.unit);
		StatusEffector.AttachAndReturnUSE(this, new List<UnitStatusEffect> {collecting}, this, true);
	}

	public void BreakCollecting(){
		UnitStatusEffect collecting = statusEffectList.Find(se => se.GetOwnerOfSkill() == "collecting");
		if (collecting == null) return;
		RemoveStatusEffect(collecting);
	}

	public void BreakChain(){
		ChainList.RemoveChainOfThisUnit (this);
	}

	public void ApplyTriggerOnPhaseStart(int phase){
		GetListPassiveSkillLogic().TriggerOnPhaseStart(this, phase);
		foreach (var statusEffect in statusEffectList){
			Skill originSkill = statusEffect.GetOriginSkill();
			if (originSkill is ActiveSkill)
				((ActiveSkill) originSkill).SkillLogic.TriggerStatusEffectAtPhaseStart(this, statusEffect);
		}
	}

	public void ApplyTriggerOnStageStart(){
		GetListPassiveSkillLogic().TriggerOnStageStart(this);
	}

	public void ApplyTriggerOnPhaseEnd(){
		GetListPassiveSkillLogic().TriggerOnPhaseEnd(this);
	}

	public void UpdateFearDebuff(){
		if (!VolatileData.OpenCheck(Setting.WillChangeOpenStage)) return;
		
		if (GetHpRatio() < 0.5f && (!RetreatBefore0HP || GetHP > RetreatHP)){
			if (statusEffectList.Find (se => se.DisplayName (true) == "두려움") == null){
				var se = StatusEffector.FindUSE ("두려움");
				StatusEffector.AttachAndReturnUSE (this, new List<UnitStatusEffect> { new UnitStatusEffect (se, this, this) }, this, false);
				fearParticle.SetActive (true);
				TutorialManager.Instance.ReserveTutorial("Fear");
			}
		}else{
			RemoveStatusEffect(statusEffectList.Find(x => x.GetOriginSkillName() == "두려움"));
			fearParticle.SetActive(false);
		}
	}

	void TriggerTileStatusEffectAtTurnEnd(){
		foreach (var tile in TileManager.Instance.GetAllTiles().Values){
			foreach (var tileStatusEffect in tile.GetStatusEffectList()){
				Skill originSkill = tileStatusEffect.GetOriginSkill();
				if (originSkill is ActiveSkill){
					((ActiveSkill) originSkill).SkillLogic.TriggerTileStatusEffectAtTurnEnd(this, tile, tileStatusEffect);
				}
			}
		}
	}

	public IEnumerator ShowFaint(){
		var BM = BattleManager.Instance;
		yield return BM.cameraMover.Slide (transform.position, Setting.basicCameraMoveDuration);
		BM.ReadyForUnitAction();
		ShowFaintTurnSkipIcon(true);
		yield return new WaitForSeconds(0.6f);
		ShowFaintTurnSkipIcon(false);
	}
	
	public void StartTurn(){
		isAlreadyBehavedObject = false;
		criticalApplied = false;
		BreakChain (); // 턴이 돌아오면 자신이 건 체인 삭제.

		if(IsPC)
			BattleManager.Instance.ShowUnitTurnStart(this);
		
		var collectingSE = statusEffectList.Find(item => item.IsTypeOf (StatusEffectType.Collect));
		if (collectingSE != null && collectingSE.Duration() <= 1){
			Unit collectedObject = collectingSE.GetObjectBeingCollected ();
			if (collectedObject != null) {
				//collectedObject.GetListPassiveSkillLogic ().TriggerOnDestroyed (collectedObject, this);
				LogManager.Instance.Record(new UnitDestroyLog(collectedObject, TrigActionType.Collect));
			}
			RemoveStatusEffect(collectingSE);
			RemoveChargeEffect();
		}

		GetListPassiveSkillLogic ().TriggerOnMyTurnStart (this);
		ApplyHealOverPhase();
		ApplyDamageOverPhase();
		ApplyBloodEffect();
	}

	void ApplyBloodEffect(){
		if (!IsPC || VolatileData.difficulty == Difficulty.Intro || !VolatileData.OpenCheck(Setting.WillChangeOpenStage)
				  || HasWillCharacteristic(WillCharacteristic.Bloodlust)
		          || HasWillCharacteristic(WillCharacteristic.Detached)) return;
		
		LogManager.Instance.Record(new WillChangeLog(this, -WillDownFormBlood, WillChangeType.Blood));
	}
	
	void ApplyDamageOverPhase(){
		foreach (var se in statusEffectList){
			int actuals = se.actuals.Count;
			for (int i = 0; i < actuals; i++){
				if (!se.IsOfType(i, StatusEffectType.DamageOverPhase)) continue;
				if (!IsHiddenUnderFogOfWar())
					CameraMover.MoveCameraToUnit(this);

				float damage = se.GetAmount(i);
				Unit caster = se.GetCaster();

				ApplyDamageByNonCasting(damage, caster, true, -GetStat(Stat.Defense));
				TutorialManager.subjectQueue.Enqueue("DOT");
			}
		}
	}
	int TotalDamageOverTime{get{
		var result = 0f;
		foreach (var se in statusEffectList){
			int actuals = se.actuals.Count;
			for (int i = 0; i < actuals; i++)
				if (se.IsOfType(i, StatusEffectType.DamageOverPhase))
					result += se.GetAmount(i);
		}
		return (int) result;
	}}

	void ApplyHealOverPhase(){
		float totalAmount = 0.0f;

		if (HasStatusEffect(StatusEffectType.HealOverPhase)){
			foreach (var statusEffect in statusEffectList)
				if (statusEffect.IsTypeOf(StatusEffectType.HealOverPhase))
					totalAmount += statusEffect.GetAmountOfType(StatusEffectType.HealOverPhase);
		}

		if (!(totalAmount > 0)) return;
		
		if (!IsHiddenUnderFogOfWar() && GetHP != GetMaxHealth())
			CameraMover.MoveCameraToUnit(this);
		RecoverHealth(totalAmount, this);
	}
	
	public void EndTurn(){
		if (!UnitManager.GetAllUnits().Contains(this)) return;
		
		if (ApOverflow > 0){
			UseActivityPoint(ApOverflow);
			if(IsPC)
				TutorialManager.Instance.Activate("ApDelete");
		}
		
		TriggerTileStatusEffectAtTurnEnd();
		HideArrow();
		ElapseStatusEffectPhase();
		ElapseSkillCoolDown();
		ChangeWill(WillChangeType.Exhausted, BigSmall.None);
		BattleUIManager.Instance.DisableSelectedUnitViewerUI();
		BattleUIManager.Instance.HideActionButtons();
		UnitManager.Instance.AllPassiveSkillsTriggerOnTurnEnd(this);
	}
	public int ApOverflow{get { return Math.Max(GetCurrentActivityPoint() - ApDeletePoint, 0);} }
	public int ApDeletePoint{get { return GetStat(Stat.Agility) / 2; }}

	public void ChangeWill(WillChangeType type, BigSmall bigSmall){
		if (!IsPC || !VolatileData.OpenCheck(Setting.WillChangeOpenStage)) return;
		LogManager.Instance.Record(new WillChangeLog(this, type, bigSmall));
	}

	public void SetAfterImageAt(Vector2Int pos, Direction direction){
		SpriteRenderer afterimageRenderer = afterimage.GetComponent<SpriteRenderer>();
		afterimageRenderer.enabled = true;

		var realPosDifference = transform.position - TileUnderUnit.transform.position;
		afterimage.transform.position = TileManager.Instance.GetTile(pos).transform.position + realPosDifference;
		Direction faceDirection = (Direction) (((int) direction + (int) BattleData.aspect + 4) % 4);
		afterimageRenderer.sprite = Sprites[faceDirection];
		Color color = afterimageRenderer.color;
		color.a = 0.5f;
		afterimageRenderer.color = color;
	}

	public void HideAfterImage(){
		if (afterimage != null)
			afterimage.GetComponent<SpriteRenderer> ().enabled = false;
	}

	public void UpdateTransparency(){
		if(IsHiddenUnderFogOfWar())
			SetAlpha(0);
		else if(HasStatusEffect(StatusEffectType.Stealth))
			SetAlpha(0.5f);
		else
			SetAlpha(1);
	}

	public void SetAlpha(float alpha){
		var renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
		renderers.Add(GetComponent<SpriteRenderer>());
		Color color;
		foreach (var renderer in renderers){
			color = renderer.color;
			color.a = alpha;
			renderer.color = color;
		}

		ParticleSystem.MainModule main = tileBuffEffect.main;
		ParticleSystem.MinMaxGradient startColor = main.startColor;
		color = startColor.color;
		color.a = alpha;
		startColor.color = color;
		main.startColor = startColor;
		Utility.ChangeParticleColor(tileBuffEffect, new Color(0, 0, 0, alpha), onlyAlpha: true);
	}

	public bool IsHiddenUnderFogOfWar(){
		if (TileUnderUnit == null)
			return false;
		FogType fogType = TileUnderUnit.fogType;
		return fogType == FogType.Black/* || (fogType == FogType.Gray && !IsObject)*/;
	}

	void DisplayChainIcon(bool onoff){
		chainAttackerIcon.SetActive(onoff);
	}

	public void ShowFaintTurnSkipIcon(bool onoff){
		faintTurnSkipIcon.SetActive(onoff);
	}

	public void ApplyInfo(UnitInfo info){
		actualStats.Clear();
		myInfo = info;
		foreach (var skill in info.skills)
			skill.owner = this;
		foreach (var kv in info.baseStats)
			actualStats.Add(kv.Key, kv.Value);
		hp = actualStats[Stat.MaxHealth];
	}

	public void LoadSprites(string spriteName = ""){
		if (spriteName == "") spriteName = CodeName;
		var sprites = VolatileData.GetUnitSprite(spriteName, GetSide() == Side.Ally);
		SetSpriteDict(Sprites, sprites);
		if (spriteName == "unitySwordman" || spriteName == "unitySpearman")
			SetSpriteDict(SubSprites, Resources.LoadAll<Sprite>("UnitImage/soldierOverride"));
		if(spriteName != CodeName)
			UpdateSpriteByDirection();
	}

	void SetSpriteDict(Dictionary<Direction, Sprite> dict, Sprite[] input){
		if (dict.Count == 0){
			dict.Add(Direction.LeftUp, input[0]);
			dict.Add(Direction.RightDown, input[1]);
			dict.Add(Direction.LeftDown, input[2]);
			dict.Add(Direction.RightUp, input[3]);
		}else{
			dict[Direction.LeftUp] = input[0];
			dict[Direction.RightDown] = input[1];
			dict[Direction.LeftDown] = input[2];
			dict[Direction.RightUp] = input[3];	
		}
	}
	
	void UpdateSpriteByDirection(){
		Direction faceDirection = (Direction) (((int) direction + (int) BattleData.aspect + 4) % 4);
		if(Sprites.Count == 0)
			LoadSprites();
		GetComponent<SpriteRenderer>().sprite = Sprites[faceDirection];
		if (SubSprites.Count > 0)
			overrideImage.sprite = SubSprites[faceDirection];
	}

	public void SetOrderPortraitSlot(OrderPortraitSlot slot){
		orderPortraitSlot = slot;
	}
	public OrderPortraitSlot GetOrderPortraitSlot(){
		return orderPortraitSlot;
	}

	void Awake(){
		recoverTextObject = transform.Find("RecoverText").gameObject;
		apChangeTextObject = transform.Find("APChangeText").gameObject;
		WillChangeTextObject = transform.Find("WillChangeText").gameObject;
		chainAttackerIcon = transform.Find("VisualProperty/icons/chain").gameObject;
		overrideImage = transform.Find("VisualProperty/OverrideImage").GetComponent<SpriteRenderer>();
		afterimage = transform.Find("VisualProperty/Afterimage").gameObject;
		highlightBorder = GetComponent<HighlightBorder> ();
	}

	void Start(){
		gameObject.name = CodeName;
		startPositionOfPhase = pivot;
		HideAfterImage();
		hp = GetMaxHealth();
		activityPoint = GetStat(Stat.Agility);

		statusEffectList = new List<UnitStatusEffect>();
		latelyHitInfos = new List<HitInfo>();

		// Awake에서 폰트 스프라이트를 로딩하기 때문
		apChangeTextObject.SetActive(true);
		apChangeTextObject.SetActive(false);
		WillChangeTextObject.SetActive(true);
		WillChangeTextObject.SetActive(false);

		SetSpecialObjectIcon();
		//myInfo.CheckPropertyStage();
		
		CheckAndHideObjectHealth();
		overrideImage.material = new Material(Shader.Find("Custom/HSVRangeShader"));
		overrideImage.material.SetVector("_HSVAAdjust", new Vector4(UnityEngine.Random.Range(0f, 1f), 0, 0, 0));
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.G))
			RegenerateActionPoint();

		/*if (Input.GetKeyDown(KeyCode.L)){
			string log = myInfo.nameKor + "\n";
			foreach (var skill in activeSkillList)
				log += skill.GetName() + "\n";

			Debug.LogError(log);

			string passiveLog = myInfo.nameKor + "\n";
			foreach (var passiveSkill in passiveSkillList)
			{
				passiveLog += passiveSkill.Name + "\n";
			}

			Debug.LogError(passiveLog);
		}*/

		if (WillChangeAmounts.Count > 0 && !WillChangeTextObject.activeSelf)
			StartCoroutine(DisplayNextWillChangeText(WillChangeAmounts.Dequeue()));
	}
	public void ShowVigilOrChainArea(){
		if (!IsActive)
			foreach (var vigilArea in _AI._AIData.info.vigilAreas)
				TileManager.Instance.PaintTiles (vigilArea.area, TileManager.Instance.TileColorMaterialForChain);
		else
			ChainList.ShowChainByThisUnit(this);
	}

	public void AddAI(){
		if(CodeName == "triana_Rest")
			SetAI(gameObject.AddComponent<S31_AI_Triana>());
		else if (CodeName.StartsWith("holder"))
			SetAI(gameObject.AddComponent<S71_AI_Holder>());
		else if (CodeName == "stel-elder")
			SetAI(gameObject.AddComponent<S101_AI_Stel_Elder>());
		else if (CodeName == "schmidt")
			SetAI(gameObject.AddComponent<S141_AI_Schmidt>());
		else if (CodeName == "grenev")
			SetAI(gameObject.AddComponent<NeverMoveAI>());
		else if (CodeName == "citizen")
			SetAI(gameObject.AddComponent<S161_AI_Citizen>());
		else if (CodeName == "monk")
			SetAI(gameObject.AddComponent<S161_AI_Monk>());
		else if (CodeName == "missionary")
			SetAI(gameObject.AddComponent<S181_AI_Missionary>());
		else
			SetAI(gameObject.AddComponent<AI>());
		
		_AI.Initialize(this, GetComponent<AIData>());
	}

	public SpriteRenderer ccIcon;
	IEnumerator ccIconCoroutine;

	public void UpdateCCIcon(){
		UpdateCCIconTo(GetAllCCIcons(this));
	}

	public void PreviewCCIcon(Dictionary<StatusEffectType, int> ccResult){
		if (IsObject) return; // 연산을 최소화하기 위해 지형지물은 건너뛰고 구현
		
		List<Sprite> icons = new List<Sprite>();
		List<bool> isChanging = new List<bool>();
		foreach (var cc in EnumUtil.ccTypeList){
			if (HasStatusEffect(cc)){
				icons.Add(VolatileData.GetIcon(EnumUtil.GetccIcon(cc)));
				isChanging.Add(ccResult[cc] != 0);
			}else{
				if (ccResult[cc] > 0)
					icons.Add(VolatileData.GetIcon(EnumUtil.GetccIcon(cc)));
				isChanging.Add(true);
			}
		}

		if (icons.Count == 0)
			icons.Add(VolatileData.GetIcon(IconSprites.Transparent));
		UpdateCCIconTo(icons, isChanging);
	}

	public void CancelPreviewCCIcon(){
		ccIcon.color = new Color(1f, 1f, 1f);
		UpdateCCIcon();
	}

	void UpdateCCIconTo(List<Sprite> icons, List<bool> isChanging = null){
		if (IsObject) return; // 연산을 최소화하기 위해 지형지물은 건너뛰고 구현
		
		if (ccIconCoroutine != null)
			StopCoroutine(ccIconCoroutine);
		ccIconCoroutine = ChangeCCIcon(icons, isChanging);
		StartCoroutine(ccIconCoroutine);
	}

	IEnumerator ChangeCCIcon(List<Sprite> icons, List<bool> isChanging = null){
		float delay = 0.6f;
		while (true){
			if(ccIcon == null) break; 
			for (int i = 0; i < icons.Count; i++){
				ccIcon.sprite = icons[i];
				if (isChanging == null || !isChanging[i]) ccIcon.color = new Color(1f, 1f, 1f);
				else ccIcon.color = new Color(1f, 1f, 1f, 0.5f);

				if (orderPortraitSlot != null)
					orderPortraitSlot.RestrictionIcon.sprite = icons[i];
				
				yield return new WaitForSeconds(delay);
			}
		}
	}

	public static List<Sprite> GetAllCCIcons(Unit unit){
		List<Sprite> icons = new List<Sprite>();
		foreach (var cc in EnumUtil.ccTypeList)
			if (unit.HasStatusEffect(cc))
				icons.Add(VolatileData.GetIcon(EnumUtil.GetccIcon(cc)));
		if (icons.Count == 0)
			icons.Add(VolatileData.GetIcon(IconSprites.Transparent));
		return icons;
	}

	public void ShowBonusIcons(DamageCalculator.AttackDamage attackDamage){
		if (IsHiddenUnderFogOfWar() || attackDamage == null) return;
		HideBonusIcons();

		foreach(var kv in attackDamage.relativeModifiers)
			if(kv.Value != 1)
				ShowBonusIcon(kv.Key);
		foreach(var kv in attackDamage.absoluteModifiers)
			if (kv.Value != 0)
				ShowBonusIcon(kv.Key);
	}
	void ShowBonusIcon(Sprite sprite){
		var icon = new GameObject(sprite.name, typeof(RectTransform));
		icon.transform.SetParent(modifierIcons.transform);
		var image = icon.AddComponent<Image>();
		image.sprite = sprite;
		icon.GetComponent<RectTransform>().sizeDelta = new Vector2(0.15f, 0.15f);
		icon.GetComponent<RectTransform>().localPosition = Vector3.zero;
		modIconInstances.Add(icon);
	}

	public void HideBonusIcons(){
		foreach(var modIcon in modIconInstances)
			Destroy(modIcon.gameObject);
		modIconInstances.Clear();
	}

	public void SetSpecialObjectIcon() {
		switch (objectTag) {
		case ObjectTag.None:
			specialObjectIcon.sprite = VolatileData.GetIcon(IconSprites.Transparent);
			break;
		case ObjectTag.Collectable:
			specialObjectIcon.sprite = VolatileData.GetIcon(IconSprites.Collect);
			break;
		case ObjectTag.Deathrattle:
			specialObjectIcon.sprite = VolatileData.GetIcon(IconSprites.Deathrattle);
			break;
		}
	}

	public void CheckAndHideObjectHealth(){
		if (IsObject && objectTag == ObjectTag.None && GetSide() == Side.Neutral && GetHP >= GetMaxHealth() && !HasStatusEffect(StatusEffectType.Shield))
			healthViewer.gameObject.SetActive(false);
	}

	public bool CheckEscape(){
		return TileUnderUnit.IsReachPosition && BattleTriggerManager.Instance.ActiveTriggers
			   .Any(trig => trig.action == TrigActionType.Escape &&
			   		BattleTriggerManager.Instance.CheckUnitType(trig, this, TrigUnitCheckType.Actor));
	}

	//자신이 파괴될 예정인지, 맞다면 이유가 무엇인지를 return
	//checkingHP는 Preview상황이라서 체크 기준 체력이 실제 체력과 다를 경우에만 입력
	public TrigActionType? GetDestroyReason(Casting casting = null){
		if (CheckEscape())
			return TrigActionType.Escape;

		//Debug.Log(CodeName + "의 체력 상황: " + GetHP);
		if (GetHP <= 0)
			return IsKillable ? TrigActionType.Kill : TrigActionType.Retreat;

		if (!RetreatBefore0HP) return null;
		var retreatHp = RetreatHP;
		if (casting != null && casting.GetTargets().Contains(this))
			retreatHp = Math.Max(RetreatHP, (int)(casting.Skill.SkillLogic.RetreatHpRatioOfMyTarget()*GetMaxHealth()));
		
		if (GetHP <= retreatHp)
			return TrigActionType.Retreat;
		return null;
	}

	public int RetreatHP{get{
		if (!RetreatBefore0HP) return 1;
		var retreatHPRatio = BattleData.turnUnit == null
			? Setting.retreatHPFloat
			: BattleData.turnUnit.GetListPassiveSkillLogic().GetRetreatHPRatioOfMyTarget(this); //효과로 인한 것도 포함
		var noel = UnitManager.Instance.GetAnUnit("noel");
		if (noel != null && noel.HasStatusEffect(StatusEffectType.Pacifist))
			retreatHPRatio += 0.1f;
		return (int) (GetMaxHealth() * retreatHPRatio);
	}}

	public IEnumerator ShowDestroy(TrigActionType actionType){
		var bloodyScreen = BattleManager.Instance.bloodyScreen.material;
		transform.Find("VisualProperty").gameObject.SetActive(false);
		yield return FadeOutEffect(actionType);
		bloodyScreen.SetFloat("_HP_Percent", 1);
	}
	IEnumerator FadeOutEffect(TrigActionType actionType){
		GetComponent<HighlightBorder>().enabled = false;
		//SetMouseOverHighlightUnitAndPortrait (false);
		var BM = BattleManager.Instance;
		float fadeOutDuration = Setting.unitFadeOutDuration;
		float fadeInDuration = Setting.unitFadeInDuration;
		Vector3 originalCameraPosition = Camera.main.transform.position;
		
		tileBuffEffect.gameObject.SetActive(false);
		if(IsPC && actionType != TrigActionType.Escape) {
			StartCoroutine(BM.cameraMover.Slide(transform.position, fadeOutDuration));
			yield return BM.FadeBloodyScreen(0, fadeOutDuration);
		}

		var move = MoveRetreat();
		if (actionType == TrigActionType.Retreat && !IsObject)
			StartCoroutine(move);

		if (IsKillable && actionType == TrigActionType.Kill)
			yield return DissolveByKill();
		else{
			foreach (var graphic in GetComponentsInChildren<Graphic>())
				StartCoroutine(Utility.Fade(graphic: graphic, duration: fadeOutDuration));
			yield return GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 0), fadeOutDuration).WaitForCompletion();
		}
		
		StopCoroutine(move);
		Debug.Log("Change Material.");
		GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
		GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);

		if (!IsPC || actionType == TrigActionType.Escape) yield break;
		StartCoroutine(BM.FadeBloodyScreen(1, fadeInDuration));
		yield return BM.cameraMover.Slide(originalCameraPosition, fadeInDuration);
	}

	IEnumerator DissolveByKill() {
		GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Custom/2D/Dissolve"));
		yield return GetComponent<SpriteRenderer>().material.DOFloat(1, "_Threshold", Setting.unitFadeOutDuration).WaitForCompletion();
	}

	IEnumerator MoveRetreat(){
		var dir = RandomMoveVector3;
		while (transform != null){
			transform.Translate(dir * Time.deltaTime*1.3f);
			yield return null;
		}
	}

	public IEnumerator HitFeedback(){
		StartCoroutine(MoveHit());
		if(GetLatelyHitInfos().Count == 0) yield break;
		var skill = GetLatelyHitInfos().Last().skill;
		if (skill == null) yield break;
		if(skill.hitSeName != "-")
			SoundManager.Instance.PlaySE(skill.hitSeName);
		if (skill.hitVeName == "-") yield break;
		var VePrefab = VolatileData.GetVisualEffectPrefab(skill.hitVeName);
		var VE = Instantiate(VePrefab, transform.position, VePrefab.transform.localRotation);
		yield return new WaitForSeconds(VE.GetComponentInChildren<ParticleSystem>().main.duration);
		Destroy(VE);
	}

	IEnumerator MoveHit(){
		if(IsObject) yield break;
		var dir = RandomMoveVector3;
		yield return transform.DOMove(transform.position+(dir*0.1f), 0.1f).WaitForCompletion();
		transform.DOMove(transform.position-(dir*0.1f), 0.1f).Play();
	}

	public static Vector3 RandomMoveVector3{get{
		var anyDir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
		return new Vector3(anyDir.x, anyDir.y, -0.05f);
	}}
}
