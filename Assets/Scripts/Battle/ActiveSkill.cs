using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Skills;
using Battle.Damage;
using Battle.Turn;
using UnityEngine;

public class ActiveSkill : Skill{	
	int requireAP;
	int cooldown;
	
	public Stat statType; //계수에 곱할 능력치의 종류
	public float powerFactor; //(보통)공격력에 곱해서 기술 위력을 결정하는 계수
	
	// reach & range
	RangeType rangeType;
	public TileRange firstRange;
	public TileRange secondRange; //자동형의 경우 반드시 1차범위 == 2차범위!
		
	SkillApplyType skillApplyType; // DamageHealth,DamageAP,HealHealth,HealAP,Buff,Debuff,Move,Tile,Etc
	
	// 시각적 이펙트
	public string castVeName;
	public bool castVeArea;
	public bool castVeMove;
	public bool castVeDir;
	public string hitVeName;
	public string hitSeName;

	// 효과음
	public string castSeName;

	public BaseActiveSkillLogic SkillLogic {
		get { return SkillLogicFactory.Get (this); }
	}

	public ActiveSkill(string skillData){
		StringParser parser = new StringParser(skillData, '\t');
		
		GetCommonSkillData(parser);
		requireAP = parser.ConsumeInt();
		cooldown = parser.ConsumeInt();

		//이하는 계수 산출. 앞에 A를 붙이면 민첩성 비례, 아니면 공격력 비례.
		string aboutStat = parser.ConsumeString();
		if(aboutStat.StartsWith("A")){
			statType = Stat.Agility;
			aboutStat = aboutStat.Substring(1);
		}else 
			statType = Stat.Power;
		powerFactor = Single.Parse(aboutStat);
		
		rangeType = parser.ConsumeEnum<RangeType>();
		firstRange = new TileRange(parser.ConsumeString());
		secondRange = new TileRange(parser.ConsumeString(), firstRange);

		skillApplyType = parser.ConsumeEnum<SkillApplyType>();
		castVeName = parser.ConsumeString();
		var castVeInfo = parser.ConsumeString();
		castVeArea = castVeInfo[0] == 'T';
		castVeMove = castVeInfo[1] == 'T';
		castVeDir = castVeInfo[2] == 'T';
		castSeName = parser.ConsumeString ();
		hitVeName = parser.ConsumeString();
		hitSeName = parser.ConsumeString();

        GetCommonSkillExplanationText(parser);
	}

	public List<Tile> GetTilesInFirstRange(Vector2Int casterPos, Direction direction, bool onlyVisible = false) {
		var result = TileManager.Instance.TilesInRange(firstRange, casterPos, direction, -1, onlyVisible, rangeType == RangeType.Route);
		//투사체 스킬이면 직선경로상에서 유닛이 가로막은 지점까지를 1차 범위로 함. 범위 끝까지 가로막은 유닛이 없으면 직선 전체가 1차 범위
		return rangeType == RangeType.Route ? TileManager.GetRouteTiles(result) : result;
	}

	public Tile GetRealTargetTileForAI(Vector2Int casterPos, Direction direction, Tile targetTile){
		if (rangeType != RangeType.Route) return targetTile;
		List<Tile> firstRange = GetTilesInFirstRange (casterPos, direction);
		return TileManager.GetRouteEndForAI (firstRange);
	}

	public Dictionary<Vector2, Color> RangeColorsForSecondRangeDisplay(int rowNum){
		Vector2Int center = new Vector2Int ((rowNum - 1) / 2, (rowNum - 1) / 2);
		Dictionary<Vector2, Color> rangeColors = new Dictionary<Vector2, Color> ();

		for (int x = 0; x < rowNum; x++)
			for (int y = 0; y < rowNum; y++)
				rangeColors [new Vector2 (x, y)] = Color.white;
		
		var redRange = new List<Vector2Int> ();
		if(secondRange.form == RangeForm.Global){
			for (int x = 0; x < rowNum; x++)
				for (int y = 0; y < rowNum; y++)
					redRange.Add (new Vector2Int (x, y));
		}else
			redRange = Utility.GetRange (secondRange, center, Direction.RightUp, 0);

		foreach (var pos in redRange)
			rangeColors [pos] = Color.red;
		
		return rangeColors;
	}

	public void CalculateBestCasting(Unit caster, Tile virtualCasterTile){
		Casting bestCasting = null;
		float maxReward = 0;

		var castings = GetPossibleCastings(caster, virtualCasterTile);

        foreach (var casting in castings){
	        float reward = casting.GetReward();
            if (reward > maxReward) {
                bestCasting = casting;
                maxReward = reward;
            }
        }

		AI.bestCasting = bestCasting;
	}

	public List<Casting> GetPossibleCastings(Unit caster, Tile virtualCasterTile){
		var result = new List<Casting>();

		//Point이면 지정 가능한 타일마다, 아니면 각 방향마다 캐스팅해서 생성
		if(rangeType == RangeType.Point){
			List<Tile> targetTiles = GetTilesInFirstRange (virtualCasterTile.Location, caster.GetDir ());
			foreach(Tile targetTile in targetTiles){
				var location = new SkillLocation (virtualCasterTile, targetTile, Utility.GetDirectionToTarget (virtualCasterTile.Location, targetTile.Location ));
				Casting casting = new Casting (caster, this, location);
				if (SkillAndChainStates.CheckApplyPossibleToTargetTilesForAI (casting))
					result.Add(new Casting(caster, this, location));
			}
		}else{
			foreach(Direction dir in EnumUtil.directions){
				Tile targetTile = GetRealTargetTileForAI(virtualCasterTile.Location, dir, virtualCasterTile);
				var location = new SkillLocation(virtualCasterTile, targetTile, dir);
				Casting casting = new Casting (caster, this, location);
				if (SkillAndChainStates.CheckApplyPossibleToTargetTilesForAI (casting)) {
					result.Add(new Casting(caster, this, location));
				}
            }
		}
		
		return result;
	}

	public void Apply(Casting casting, int chainCombo, bool duringAIDecision, bool isPreview = false) { // duringAIDecision과 isPreview는 값이 다를 수 있음
		var caster = casting.Caster;
		var realEffectRange = casting.RealRange;
        casting.SetTargets();
		var targets = casting.GetTargets();
		var passiveSkillLogicsOfCaster = caster.GetListPassiveSkillLogic();

		caster.BreakChain ();

		SkillLogic.ActionBeforeMainCasting(casting);

        LogManager.Instance.Record(new SoundEffectLog(this));
        LogManager.Instance.Record(new VisualEffectLog(casting));

		foreach(var target in targets)
			target.GetListPassiveSkillLogic().TriggerTargetPassiveBeforeCast(casting, target);
		foreach (var unit in UnitManager.GetAllUnits())
			unit.GetListPassiveSkillLogic ().TriggerExistingUnitPassiveBeforeCast (casting, unit);

		targets.ForEach(target => {
			// AI 유닛에게 활성화를 깨울 만한 기술(공격/디버프/이동기)이 날아오면, 그 유닛이 기술 날아온 순간 활성화되는 AI인지 확인하고 맞다면 활성화시킨다
			if(IsAwaking() && target.IsAI){
				AIData _AIData = target.GetAI()._AIData;
				if (_AIData != null && !isPreview && IsAwaking()) _AIData.SetActive(_AIData.info.actOnExternal);
			}

			CastingApply castingApply = new CastingApply(casting, target);

			//회피 등의 이유로 기술을 무시하는 경우 AI 활성화 외의 효과 없음
			if (IsOffensive() && !isPreview && CheckEvasion(castingApply)
			    || target.GetListPassiveSkillLogic().IgnoreCasting(castingApply, chainCombo)) return;
			// 효과 외의 부가 액션 (AP 감소 등)
			SkillLogic.ActionInDamageRoutine(castingApply);
			passiveSkillLogicsOfCaster.OnMyCastingApply(castingApply);
			
			// 데미지 적용
			if(SkillLogic.MayDisPlayDamageCoroutine(castingApply)) {
				if (skillApplyType == SkillApplyType.DamageHealth){
					ApplyDamage(castingApply, chainCombo, duringAIDecision);
				} else {
					DamageCalculator.CalculateAmountOtherThanAttackDamage(castingApply);
					float amount = castingApply.GetDamage().resultDamage;
					if (skillApplyType == SkillApplyType.DamageAP) {
						target.ChangeAP(-(int)amount);
					} else if (skillApplyType == SkillApplyType.HealHealth) {
						target.RecoverHealth(amount, caster);
						passiveSkillLogicsOfCaster.TriggerApplyingHeal(castingApply);
					} else if (skillApplyType == SkillApplyType.HealAP) {
						target.ChangeAP((int)amount);
					}
				}
			}
			// 기술이 붙이는 상태효과는 기술이 적용된 후에 붙인다.
			if(unitStatusEffectList.Count > 0){
				bool ignored = false;
				foreach (var tileStatusEffect in target.TileUnderUnit.GetStatusEffectList()) {
					Skill originSkill = tileStatusEffect.GetOriginSkill();
					if (originSkill is ActiveSkill && 
						!((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(castingApply, tileStatusEffect))
							ignored = true;
				}
				if(!ignored)
					StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets (castingApply, target, chainCombo);
			}
			target.GetListPassiveSkillLogic().TriggerActiveSkillAppliedToOwner(castingApply);
		});

		realEffectRange.ForEach(tile => StatusEffector.FindAndAttachTileStatusEffects(caster, this, tile));

		// 기술 사용 시 적용되는 특성
		passiveSkillLogicsOfCaster.TriggerUsingSkill(casting, targets);
		foreach(var statusEffect in caster.statusEffectList) {
			Skill originPassiveSkill = statusEffect.GetOriginSkill();
			if(originPassiveSkill is PassiveSkill)
				((PassiveSkill)originPassiveSkill).SkillLogic.TriggerStatusEffectsOnUsingSkill(caster, targets, statusEffect);
		}

		// 시전 후 일부에게만(예: 시전자에게 붙는 부가 효과(예: 10스테이지 장로가 아군 버프 후 본인 기절)
		SkillLogic.OnCast (casting);

		// 공격스킬 시전시 관련 효과중 1회용인 효과 제거 (공격할 경우 - 공격력 변화, 데미지 변화, 강타)
		if (skillApplyType != SkillApplyType.DamageHealth) return;

		var statusEffectsToRemove = caster.statusEffectList.FindAll(x => x.GetIsOnce() &&
		                            									 (x.IsTypeOf(StatusEffectType.PowerChange) ||
		                                                                  x.IsTypeOf(StatusEffectType.DamageChange) ||
		                                                                  x.IsTypeOf(StatusEffectType.Smite) ||
		                                                                  x.IsTypeOf(StatusEffectType.RequireSkillAPChange)));
		foreach (var statusEffect in statusEffectsToRemove)
			caster.RemoveStatusEffect (statusEffect);
		
	}

	private static bool CheckEvasion(CastingApply castingApply) {
		Unit caster = castingApply.Caster;
		Unit target = castingApply.Target;

		float totalEvasionChance = target.GetEvasionChance() + target.GetListPassiveSkillLogic().ApplyBonusEvasionFromTargetPassive(castingApply);
		float randomNumber = UnityEngine.Random.Range(0, 1.0f);

		// 회피에 성공했는지 아닌지에 상관 없이 일회성 회피 효과 해제
		List<UnitStatusEffect> statusEffectsToRemove =  target.statusEffectList.FindAll(x => x.IsTypeOf(StatusEffectType.EvasionChange)
                && x.GetIsOnce());
		foreach(var statusEffect in statusEffectsToRemove)
			target.RemoveStatusEffect(statusEffect);

		if (totalEvasionChance > randomNumber) {
            LogManager.Instance.Record(new EvadeLog(caster, target));
            LogManager.Instance.Record(new DisplayDamageTextLog(target, new DamageCalculator.AttackDamage(0), true, isEvasion: true));
			return true;
		} else {
			return false;
		}
	}

	private static void ApplyDamage(CastingApply castingApply, int chainCombo, bool duringAIDecision){
		Unit caster = castingApply.Caster;
		Unit target = castingApply.Target;

		DamageCalculator.CalculateAttackDamage(castingApply, chainCombo);
		foreach (var unit in UnitManager.GetAllUnits())
			unit.GetListPassiveSkillLogic().OnAnyCastingDamage(castingApply, chainCombo);
		
		DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		bool canReflect = target.HasStatusEffect(StatusEffectType.Reflect);
		float reflectAmount = 0;
		if (canReflect && !duringAIDecision) {
			reflectAmount = DamageCalculator.CalculateReflectDamage(attackDamage.resultDamage, target, caster);
			attackDamage.resultDamage -= reflectAmount;
		}
        
		target.ApplyDamageByCasting(castingApply, true, duringAIDecision);
		if(canReflect && !duringAIDecision)  reflectDamage(caster, target, reflectAmount);
	}
	static void reflectDamage(Unit caster, Unit target, float reflectAmount) {
		caster.ApplyDamageByNonCasting(reflectAmount, target, true);

		foreach (var statusEffect in target.statusEffectList) {
			if (!statusEffect.IsTypeOf(StatusEffectType.Reflect)) continue;
			
			Skill originSkill = statusEffect.GetOriginSkill();
			if (originSkill is ActiveSkill)
				((ActiveSkill)originSkill).SkillLogic.TriggerStatusEffectAtReflection(target, statusEffect, caster);
			if (statusEffect.GetIsOnce())
				target.RemoveStatusEffect(statusEffect);
		}
	}

	public IEnumerator AIUseSkill(Casting casting, bool chain = false){
        var LM = LogManager.Instance;
		var caster = casting.Caster;
		BattleData.selectedSkill = casting.Skill;
		SkillAndChainStates.RenewHealthPreview(caster, casting.Location.TargetTile, casting.Location.Dir);

		if (chain){
			LM.Record(new ChainLog(casting));
			SkillAndChainStates.WaitChain(casting);
		}else
			LM.Record(new CastLog(casting));
		
		LM.Record(new DirectionChangeLog(caster, caster.GetDir(), casting.Location.Dir));

        if(!caster.IsHiddenUnderFogOfWar())
            CameraMover.MoveCameraToUnit (caster);
        SetSkillNamePanelUI ();

        if(!Utility.CheckShowMotion()){
            LM.Record(new PaintTilesLog(casting.FirstRange, TileManager.Instance.TileColorMaterialForRange1));
            LM.Record(new WaitForSecondsLog(Configuration.NPCBehaviourDuration));
            LM.Record(new DepaintTilesLog(TileManager.Instance.TileColorMaterialForRange1));
        }

		SkillAndChainStates.BeforeCastingRoutine (casting);

		LM.Record(new PaintTilesLog(casting.RealRange, TileManager.Instance.TileColorMaterialForRange2));
		if(!chain)
			yield return SkillAndChainStates.ApplyAllTriggeredChains (casting);
		LM.Record(new DepaintTilesLog(TileManager.Instance.TileColorMaterialForRange2));

		HideSkillNamePanelUI ();
	}

	public void SetSkillNamePanelUI(){
		BattleUIManager.Instance.SetSkillNamePanelUI(Name);
	}
	public void HideSkillNamePanelUI(){
		BattleUIManager.Instance.HideSkillNamePanelUI ();
	}

    // 적대적 기술
	public bool IsOffensive(){
		return skillApplyType == SkillApplyType.DamageHealth
		|| skillApplyType == SkillApplyType.DamageAP
		|| skillApplyType == SkillApplyType.Debuff;
	}
	// 이로운 기술
	public bool IsFriendly(){
		return skillApplyType == SkillApplyType.Buff
		|| skillApplyType == SkillApplyType.HealHealth
		|| skillApplyType == SkillApplyType.HealAP;
	}
	// AI를 활성화시키는 기술타입
	bool IsAwaking() {return IsOffensive() || skillApplyType == SkillApplyType.Move;}
  
	public string GetName() {return korName;}
    public void SetRequireAP(int requireAP) { this.requireAP = requireAP;}
	public int GetRequireAP() {return requireAP;}
	public int GetCooldown() {return cooldown;}
    public void SetCooldown(int cooldown) {this.cooldown = cooldown;}
	public float GetPowerFactor(Stat status) {
        if(status == statType)
            return powerFactor;
        else return 0;
    }
	public RangeType GetRangeType() {return rangeType;}
	public SkillApplyType GetSkillApplyType() {return skillApplyType;}
}
