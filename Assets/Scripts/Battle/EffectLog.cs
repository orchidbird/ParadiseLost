using System;
using Enums;
using Battle;
using GameData;
using UnityEngine;
using UtilityMethods;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Battle.Damage;

public class EffectLog : Log {
    public EventLog parentEvent;

	public Unit actor{get{
		if (parentEvent == null)
			parentEvent = LogManager.Instance.GetLastEventLog();
		return parentEvent.actor;
	}}

	// 이 effect가 발생한 타일들. AI를 activate시킬 때 사용한다.
	public List<Tile> tiles = new List<Tile>();
    public virtual bool isMeaningless(){
        return false;
    }
}

public class HPChangeLog : EffectLog {
    public Unit target;
	public Unit caster; //연계할 경우 모든 EffectLog가 연계를 터뜨린 유닛의 Casting에 붙으므로 actor가 실제와 달라질 수 있는데 이 때문에 caster를 사용해야 함.
    public int amount { get { return -(int)damage.resultDamage; } }
    public DamageCalculator.AttackDamage damage;

    readonly int originalHealth;
    readonly int originalShield;
    readonly int maxHealth;
    int AfterHealth { get {
        int result = originalHealth + amount;

        if (result > maxHealth) result = maxHealth;
        else if (result < 0) result = 0;
        return result;
    }}

    public HPChangeLog(Unit target, Unit caster, DamageCalculator.AttackDamage damage = null, float amount = 0){
        this.target = target;
	    this.caster = caster;
        this.damage = damage ?? new DamageCalculator.AttackDamage(-amount);

        originalHealth = target.GetHP;
        originalShield = target.GetRemainShield();
        maxHealth = target.GetMaxHealth();
    }
    public override string GetText() {
	    var text = target.GetNameKor() + ": 체력 ";
        if (amount < 0) {
            text += -amount + " 감소";
        } else {
            text += amount + " 증가";
        }
        return text;
    }
    public override IEnumerator Execute() {
		tiles = new List<Tile> { target.TileUnderUnit };
	    target.hp += amount;

	    if (amount >= 0) yield break;

		if (!caster.criticalApplied && damage.IsCritical) {
			caster.criticalApplied = true;
			caster.ChangeAP(caster.GetStat(Stat.Agility) / 2);
			RecordData.critCount++;
		}

		BattleTriggerManager.Instance.CountTriggers(TrigActionType.Damage, caster, target: target, log: this);
	    if (target.IsPC && target.GetHP / (float)target.GetMaxHealth() < Setting.bloodyScreenHPThreshold){
		    var BM = BattleManager.Instance;
		    BM.StartCoroutine(target.GetHP > 0
			    ? BM.FlickerBloodyScreen(Setting.bloodynessWhenHit, Setting.bloodyScreenDurationWhenHit)
			    : BM.FlickerBloodyScreen(1, Setting.unitFadeOutDuration, true));
	    }
	    
	    target.StartCoroutine(target.HitFeedback());
    }
	public override void Rewind() {
		target.hp = originalHealth;
	}
	public override bool isMeaningless() {
        return amount == 0;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(target);
        logDisplay.CreateHealthBar(originalHealth, originalShield, maxHealth, AfterHealth, originalShield, target);
    }
}

public class APChangeLog : EffectLog {
    public Unit unit;
    public int amount;

    int beforeAmount;
	int AfterAmount { get {return Math.Max(beforeAmount + amount, 0); }}

    public APChangeLog(Unit unit, int amount) {
        this.unit = unit;
        this.amount = amount;
    }
    public override string GetText() {
	    var text = unit.GetNameKor() + " : 행동력 ";
        if (amount < 0) {
            text += -amount + " 감소";
        } else { 
            text += amount + " 증가"; 
        }
        return text;
    }
    public override IEnumerator Execute() {
		beforeAmount = unit.activityPoint;
		unit.activityPoint = AfterAmount;
	    //Debug.Log(unit.CodeName + "의 행동력: " + beforeAmount + " -> " + unit.activityPoint);
        yield break;
	}
	public override void Rewind() {
		unit.activityPoint = beforeAmount;
	}
	public override bool isMeaningless() {
        return amount == 0;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(unit);
        logDisplay.CreateText(Language.Select("행동력 ", "AP ") + beforeAmount + " -> " + AfterAmount);
    }
}

public class WillChangeLog : EffectLog{
	List<string[]> valueTable;
	List<string[]> ValueTable{get{ return valueTable ?? (valueTable = Parser.GetMatrixTableFrom("Data/WillChange_ValueTable")); }}

	WillChangeType reason = WillChangeType.Etc;
	Unit unit;
	int amount;

	public WillChangeLog(Unit unit, WillChangeType reason, BigSmall bigSmall){
		this.reason = reason;
		var row = Parser.FindRowOf(ValueTable, reason.ToString());
		amount = int.Parse(row[bigSmall == BigSmall.Big ? 1 : 2]);
		this.unit = unit;

		if (VolatileData.OpenCheck(Setting.WillCharacteristicOpenStage)){
			var characteristics = unit.WillCharacteristics.Where(kv => kv.Value).ToList().ConvertAll(kv => kv.Key);
			foreach (var c in characteristics){
				var index = ValueTable[0].ToList().FindIndex(v => c.ToString() == v);
				//Debug.Log(unit.CodeName + "의 특성 " + c + ": " + index);
				var modifier = row[index];
				bool multiply = false;
				if (modifier == String.Empty) continue;

				if (modifier[0] == '*')
					amount *= int.Parse(modifier.Substring(1));
				else
					amount += int.Parse(modifier);
			}	
		}
		
		ApplyFear();
		//Debug.Log(unit.CodeName + " / 사유: " + reason + " / 변동량: " + amount);
	}
	public WillChangeLog(Unit unit, int amount, WillChangeType reason = WillChangeType.Etc){
		this.unit = unit;
		this.amount = amount;
		if (reason == WillChangeType.Blood && unit.HasWillCharacteristic(WillCharacteristic.Sympathy))
			this.amount *= 2;
		this.reason = reason;
		ApplyFear();
	}

	void ApplyFear(){
		if (amount < 0 && unit.statusEffectList.Exists(se => se.GetOriginSkillName() == "두려움"))
			amount *= 2;
	}
	public override string GetText() {
		var text = unit.GetNameKor() + " : 의지 ";
		if (amount < 0)
			text += -amount + " 감소";
		else 
			text += amount + " 증가"; 

		return text;
	}
	public override IEnumerator Execute() {
		if (unit.willHistoryDict.ContainsKey(reason))
			unit.willHistoryDict[reason] += amount;
		else
			unit.willHistoryDict.Add(reason, amount);

		var afterAmount = unit.GetStat(Stat.Will);
		if(afterAmount <= 50)
			TutorialManager.Instance.ReserveTutorial("Will_Down");
		if (amount != 0)
			unit.EnqueueWillChangeText (amount);
		if(reason == WillChangeType.Pain)
			TutorialManager.Instance.ReserveTutorial("Will_Pain");
		
		yield break;
	}
	public override void Rewind(){
		if (unit.willHistoryDict.ContainsKey(reason))
			unit.willHistoryDict[reason] -= amount;
		else
			Debug.LogError("원인이 등록되어 있지 않아서 의지 변동을 되돌릴 수 없음!");
	}
	public override bool isMeaningless() {
		return amount == 0;
    }
	
    public override void CreateExplainObjects() {
    	logDisplay.CreateUnitImage(unit);
		logDisplay.CreateImage(VolatileData.GetIcon(Stat.Will));
    	logDisplay.CreateText(unit.GetStat(Stat.Will) - amount + " -> " + unit.GetStat(Stat.Will));
    }
}

public class CoolDownLog : EffectLog {
    Unit caster;
    ActiveSkill skill;
    public CoolDownLog(Unit caster, ActiveSkill skill) {
        this.caster = caster;
        this.skill = skill;
    }
    public override string GetText() {
        return caster.GetNameKor() + " : " + skill.GetName() + " 재사용 대기 " + skill.GetCooldown() + "페이즈";
    }

    public override IEnumerator Execute() {
		tiles = new List<Tile> { caster.TileUnderUnit };
		if (!caster.GetUsedSkillDict ().ContainsKey (skill.GetName ()))
			caster.GetUsedSkillDict ().Add (skill.GetName (), skill.GetCooldown ());
        yield return null;
	}
	public override void Rewind() {
		caster.GetUsedSkillDict().Remove(skill.GetName());
	}
	public override bool isMeaningless() {
        return skill.GetCooldown() == 0;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(caster);
        logDisplay.CreateSkillImage(skill, caster);
        logDisplay.CreateText( " 재사용 대기 " + skill.GetCooldown() + "페이즈");
    }
}

public class UnitDestroyLog : EffectLog {
    public Unit target;
    public TrigActionType reason;

    string ActionTypeString {get {
        string text = "";
            switch (reason) {
            case TrigActionType.Kill: text = "사망"; break;
            case TrigActionType.Retreat: text = "퇴각"; break;
            case TrigActionType.Escape: text = "탈출"; break;
	        case TrigActionType.Collect: text = "수집"; break;
            }
        return text;
    }}
    public UnitDestroyLog(Unit unit, TrigActionType reason) {
        target = unit;
        this.reason = reason;
    }

	public override void Initialize(){
		target.GetListPassiveSkillLogic ().TriggerOnDestroyed (actor, reason, target);
	}

	public override string GetText() {
        return target.GetNameKor() + " : " + ActionTypeString;
    }

    public override IEnumerator Execute() {
		tiles = new List<Tile> { target.TileUnderUnit };

        if(reason != TrigActionType.Escape){
            BattleTriggerManager.Instance.CountTriggers(reason, actor, target: target, log: this);
		    BattleTriggerManager.Instance.CountTriggers(TrigActionType.Neutralize, actor, target: target, log: this);
			if (reason == TrigActionType.Kill && !target.IsObject && VolatileData.OpenCheck(Setting.WillChangeOpenStage))
	        	target.TileUnderUnit.SetBloody(true);
        }
	    
	    UnitManager.Instance.DeleteDestroyedUnit(target);
	    target.StartCoroutine(target.ShowDestroy(reason));
	    BattleTriggerManager.Instance.CountTriggers(TrigActionType.UnderCount, actor: target, log: this);

	    if (target.IsPC){
		    var seInfo = StatusEffector.FindUSE("광란");
		    foreach (var ally in UnitManager.GetAllUnits().FindAll(unit => unit.IsPC))
			    if(ally != target)
				    StatusEffector.AttachAndReturnUSE(ally, new List<UnitStatusEffect> { new UnitStatusEffect(seInfo, ally, ally) }, ally, false);
	    }
	    
	    yield break;
    }
	//UnitDetroyLog는 Rewind할 수 없으므로 UnitDestroyLog가 있다면 Rewind하지 못하게 할 것
	public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(target);
        logDisplay.CreateText(ActionTypeString);
    }
}
public class AddMoveOverloadLog : EffectLog {
	Unit unit;
	int beforeMovedTileCount;
	int tileCount;
	float beforeMoveCost;
	float moveCost;
	
	public AddMoveOverloadLog(Unit unit, int tileCount, float moveCost) {
		this.unit = unit;
		this.tileCount = tileCount;
		this.moveCost = moveCost;
	}
	public override string GetText() {
		return unit.GetNameKor() + " : tileCount + " + tileCount + ", moveCost + " + moveCost;
	}
	public override IEnumerator Execute() {
		beforeMovedTileCount = unit.movedTileCount;
		beforeMoveCost = unit.previousMoveCost;
		unit.movedTileCount += tileCount;
		unit.previousMoveCost += moveCost;
		yield return null;
	}
	public override void Rewind() {
		unit.movedTileCount = beforeMovedTileCount;
		unit.previousMoveCost = beforeMoveCost;
	}
}
public class AddChainLog : EffectLog {
    Casting casting;

    public AddChainLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill skill = casting.Skill;
        return caster.GetNameKor() + " : " + skill.GetName() + " 연계 대기열에 추가";
    }

    public override IEnumerator Execute() {
        Chain newChain = new Chain(casting);
        ChainList.GetChainList().Add(newChain);
        ChainList.SetChargeEffectToUnit(casting.Caster);
        yield return null;
	}
	public override void Rewind() {
		Chain chainToRemove = ChainList.GetChainList().Find(chain => chain.Casting == casting);
		ChainList.GetChainList().Remove(chainToRemove);
		casting.Caster.RemoveChargeEffect();
		casting.Caster.PreviewChainTriggered (false);
	}
}
public class RemoveChainLog : EffectLog {
    Unit unit;
	Chain removedChain;
    public RemoveChainLog(Unit unit) {
        this.unit = unit;
    }
    public override string GetText() {
        return unit.GetNameKor() + " : 연계 해제";
    }

    public override IEnumerator Execute() {
		List<Chain> chainList = ChainList.GetChainList();
        removedChain = chainList.Find(x => x.Caster == unit);
        if (removedChain != null) {
            chainList.Remove(removedChain);
            ChainList.RemoveChargeEffectOfUnit(unit);
			unit.PreviewChainTriggered (false);
        }
        yield return null;
	}
	public override void Rewind() {
		ChainList.GetChainList().Add(removedChain);
		ChainList.SetChargeEffectToUnit(removedChain.Caster);
	}
	public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(unit);
        logDisplay.CreateImage(VolatileData.GetIcon(IconSprites.Chain));
        logDisplay.CreateText("해제");
    }
}

public class StatusEffectLog : EffectLog {
    public StatusEffect statusEffect;  //typeof 를 통해 UnitStatusEffect인지 tileStatusEffect인지 알 수 있고,
                                //owner(또는 ownerTile) 변수를 통해 이 statusEffect를 가진 Object를 참조할 수 있음
	public StatusEffect statusEffectSnapShot;
    public StatusEffectChangeType type;
    public int index;
    public float beforeAmount;
    public float afterAmount;
    string PredicateString { get {
            string text = "";
            if (type == StatusEffectChangeType.Remove)
                text += " 해제";
            else if (type == StatusEffectChangeType.Attach)
                text += " 부착";
            else {
                if (type == StatusEffectChangeType.DurationChange)
                    text += "남은 페이즈 ";
                else if (type == StatusEffectChangeType.StackChange)
                    text += " 중첩 ";
				text += (int)beforeAmount + " -> " + (int)afterAmount;
            }
            return text;
        } 
    }
    public StatusEffectLog(StatusEffect statusEffect, StatusEffectChangeType type, int index, float beforeAmount, float afterAmount) {
        this.statusEffect = statusEffect;
        this.type = type;
        this.index = index;
        this.beforeAmount = beforeAmount;
        this.afterAmount = afterAmount;
    }
    public override string GetText() {
        string text = "";
        if (statusEffect is TileStatusEffect) {
            Tile ownerTile = ((TileStatusEffect)statusEffect).GetOwnerTile();
            text += "타일 " + ownerTile.Location;
        }
        else if (statusEffect is UnitStatusEffect) {
            Unit owner = ((UnitStatusEffect)statusEffect).GetOwner();
            text += owner.GetNameKor();
        }
        string name = statusEffect.DisplayName(true);
        text += " : " + name;
        text += PredicateString;
        return text;
    }

    public override IEnumerator Execute() {
		statusEffectSnapShot = new StatusEffect(statusEffect);
        if (type == StatusEffectChangeType.AmountChange || type == StatusEffectChangeType.AmountChange
            || type == StatusEffectChangeType.DurationChange || type == StatusEffectChangeType.StackChange) {
            switch (type) {
            case StatusEffectChangeType.AmountChange:
	            statusEffect.SetAmount(index, afterAmount, true);
                break;
            case StatusEffectChangeType.DurationChange:
                statusEffect.flexibleElem.display.remainPhase = (int)afterAmount;
                break;
            case StatusEffectChangeType.StackChange:
                SetStack(statusEffect, (int)afterAmount);
                break;
            }
        } else {
            if (statusEffect is UnitStatusEffect) {
                UnitStatusEffect unitStatusEffect = (UnitStatusEffect)statusEffect;
                Unit owner = unitStatusEffect.GetOwner();
                switch (type) {
                case StatusEffectChangeType.Remove:
                    List<UnitStatusEffect> newStatusEffectList = owner.statusEffectList.FindAll(se => se != statusEffect);
                    owner.SetStatusEffectList(newStatusEffectList);
	                if(statusEffect.IsTypeOf(StatusEffectType.Collect))
		                owner.RemoveChargeEffect();
                    break;
                case StatusEffectChangeType.Attach:
                    BattleTriggerManager.Instance.CountTriggers(TrigActionType.Effect, unitStatusEffect.GetCaster(), subType: unitStatusEffect.DisplayName(true), target: owner, log: this); 
                    var oldSameEffect = owner.statusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
	                if (oldSameEffect == null){
		                statusEffectSnapShot = null;
		                owner.statusEffectList.Add(unitStatusEffect);
	                }else{
		                statusEffectSnapShot.Clone(oldSameEffect);
		                Renew(oldSameEffect, statusEffect);
	                }
                    break;
                }
            } else if (statusEffect is TileStatusEffect) {
                TileStatusEffect tileStatusEffect = (TileStatusEffect)statusEffect;
                Tile ownerTile = tileStatusEffect.GetOwnerTile();
                switch (type) {
                case StatusEffectChangeType.Remove:
                    List<TileStatusEffect> newStatusEffectList = ownerTile.StatusEffectList.FindAll(se => se != statusEffect);
					ownerTile.SetStatusEffectList(newStatusEffectList);
					if (statusEffect.IsTypeOf (StatusEffectType.Trap))
						ownerTile.SetTrapImage(null);
                    break;
                case StatusEffectChangeType.Attach:
                    TileStatusEffect alreadyAppliedSameEffect = ownerTile.StatusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
					if (alreadyAppliedSameEffect != null) {
						statusEffectSnapShot.Clone(alreadyAppliedSameEffect);
						Renew(alreadyAppliedSameEffect, statusEffect);
					} else {
						statusEffectSnapShot = null;
						if(statusEffect.IsTypeOf(StatusEffectType.Trap)){
							if (!ownerTile.IsUnitOnTile()){
								ownerTile.StatusEffectList.Add(tileStatusEffect);
								ownerTile.SetTrapImage(Resources.Load<Sprite>("TileImage/Trap"));
							}
						}else if (statusEffect.IsTypeOf(StatusEffectType.Aura) && statusEffect.GetOriginSkill().owner.CodeName == "bianca"){
							if (!ownerTile.IsUnitOnTile()){
								ownerTile.StatusEffectList.Add(tileStatusEffect);
								ownerTile.SetTrapImage(Resources.Load<Sprite>("TileImage/MagicCircle"));
							}
						}else
							ownerTile.StatusEffectList.Add(tileStatusEffect);
					}
					break;
                }
            }
        }
	    
	    TutorialManager.Instance.ReserveTutorial("StatusEffect");
	    if(statusEffect.GetOriginSkillName() != "두려움" && statusEffect.IsTypeOf(StatusEffectType.WillChange))
		    TutorialManager.Instance.ReserveTutorial("Will_Skill");
        yield break;
    }
	public override void Rewind() {
		if(statusEffectSnapShot != null)
			statusEffect.Clone(statusEffectSnapShot);
		if (statusEffect is UnitStatusEffect) {
			UnitStatusEffect unitStatusEffect = (UnitStatusEffect) statusEffect;
			Unit owner = unitStatusEffect.GetOwner();
			switch (type) {
			case StatusEffectChangeType.Remove:
				owner.statusEffectList.Add(unitStatusEffect);
				break;
			case StatusEffectChangeType.Attach:
				UnitStatusEffect alreadyAppliedSameEffect = owner.statusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
				if (alreadyAppliedSameEffect != null) {
					if(statusEffectSnapShot != null)
						alreadyAppliedSameEffect.Clone(statusEffectSnapShot);
					else owner.statusEffectList.Remove(alreadyAppliedSameEffect);
				}
				break;
			}
		} else if (statusEffect is TileStatusEffect) {
			TileStatusEffect tileStatusEffect = (TileStatusEffect) statusEffect;
			Tile ownerTile = tileStatusEffect.GetOwnerTile();
			switch (type) {
			case StatusEffectChangeType.Remove:
				ownerTile.StatusEffectList.Add(tileStatusEffect);
				break;
			case StatusEffectChangeType.Attach:
				TileStatusEffect alreadyAppliedSameEffect = ownerTile.StatusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
				if (alreadyAppliedSameEffect != null) {
					if (statusEffectSnapShot != null)
						alreadyAppliedSameEffect.Clone(statusEffectSnapShot);
					else ownerTile.StatusEffectList.Remove(alreadyAppliedSameEffect);
				}
				break;
			}
		}
	}
	void Renew(StatusEffect oldSE, StatusEffect newSE){
        // 지속시간, 수치 초기화. 스택 가능할 경우 1스택 추가
        int num = oldSE.actuals.Count;
		oldSE.flexibleElem.display.remainPhase = statusEffect.Duration();
		if (statusEffect.IsStackable)
			SetStack (oldSE, 1, add: true);
		
		for (int i = 0; i < num; i++){
			float amount = newSE.GetAmount(i);
			//기본적으로 새 값으로 덮어쓰되, Stackable한 보호막일 경우 원래 값에 더해줌 
			oldSE.SetAmount(i, amount + (statusEffect.IsStackable && statusEffect.actuals[i].statusEffectType == StatusEffectType.Shield ? oldSE.actuals[i].amount : 0));
		}
    }

    void SetStack(StatusEffect se, int amount, bool add = false) {
        int result;
        int maxStack = se.myInfo.maxStack;
        if(add)
            result = se.flexibleElem.display.remainStack + amount;
        else result = amount;
        if (result > maxStack) result = maxStack;
        if (result < 0) result = 0;
        se.flexibleElem.display.remainStack = result;
    }

    public Unit GetOwner() {
        if(statusEffect is UnitStatusEffect)   return ((UnitStatusEffect)statusEffect).GetOwner();
        return null;
    }
    public Tile GetOwnerTile() {
        if(statusEffect is TileStatusEffect)    return ((TileStatusEffect)statusEffect).GetOwnerTile();
        return null;
    }
    public float GetShieldChangeAmount(){
	    if(!statusEffect.IsTypeOf(StatusEffectType.Shield)) return 0;
	    
	    float amount = afterAmount - beforeAmount;
	    if (type == StatusEffectChangeType.AmountChange) return amount;
	    if (type == StatusEffectChangeType.Attach){
		    float _beforeAmount = 0;
		    Unit owner = ((UnitStatusEffect)statusEffect).GetOwner();
		    UnitStatusEffect alreadyAppliedSameEffect = owner.statusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
		    if (alreadyAppliedSameEffect != null)
			    _beforeAmount = alreadyAppliedSameEffect.GetAmountOfType(StatusEffectType.Shield);
		    return statusEffect.GetAmountOfType(StatusEffectType.Shield) - _beforeAmount;
	    }if (type == StatusEffectChangeType.Remove)
		    return -statusEffect.GetAmountOfType (StatusEffectType.Shield);
	    
	    return 0;
    }
	public int GetCCRemainPhaseChangeAmount(StatusEffectType CCtype){
		if (statusEffect is UnitStatusEffect && (statusEffect.IsTypeOf(CCtype))) {
			int amount = (int)afterAmount - (int)beforeAmount;
			if (type == StatusEffectChangeType.DurationChange) {
				return amount;
			} else if (type == StatusEffectChangeType.Attach) {
				return statusEffect.Duration ();
			} else if (type == StatusEffectChangeType.Remove) {
				return -statusEffect.Duration ();
			}
		}
		return 0;
	}
    public override void CreateExplainObjects() {
        if(statusEffect is UnitStatusEffect)
            logDisplay.CreateUnitImage(GetOwner());
        else if(statusEffect is TileStatusEffect)
            logDisplay.CreateTileImage(GetOwnerTile());

		if (type != StatusEffectChangeType.Attach) {
			logDisplay.CreateStatusEffectImage(statusEffect);
			logDisplay.CreateText(PredicateString);
		} else {
			logDisplay.CreateImage(VolatileData.GetIcon(IconSprites.LeftArrow));
			logDisplay.CreateStatusEffectImage(statusEffect);
		}
    }
}

/*public class TileChangeLog : EffectLog {
	public Tile tile;
	Sprite beforeSprite;
	public string tileInfoString;
	string beforeTileInfoString;
	public TileChangeLog(Tile tile, string tileInfoString) {
		this.tile = tile;
		this.tileInfoString = tileInfoString;
		beforeSprite = tile.sprite.sprite;
	}
	public override IEnumerator Execute() {
		tiles = new List<Tile> { tile };
		tile.ApplyTileInfo(new TileInfo(tile.location, tileInfoString, height: tile.height, fogType: tile.fogType));
		yield return null;
	}
	public override void Rewind() {
		tile.ApplyTileInfo(new TileInfo(tile.location, beforeTileInfoString, height: tile.height, fogType: tile.fogType));
	}
	public override void CreateExplainObjects() {
		logDisplay.CreateImage(beforeSprite);
		logDisplay.CreateImage(VolatileData.GetIcon(IconSprites.RightArrow));
		logDisplay.CreateTileImage(tile);
	}
}*/

public class TileAttachLog : EffectLog{ //현재는 "파편" 특성 구현을 위해서만 존재하며, 다른 상황에서 쓸 경우 코드 내용에 대한 검토가 필요
	Tile tile;
	int costChangeAmount;
	
	public TileAttachLog(Tile tile, int costChange){
		this.tile = tile;
		costChangeAmount = costChange;
	}
	
	public override IEnumerator Execute(){
		tile.Override.enabled = true;
		tile.APAtStandardHeight += costChangeAmount;
		yield break;
	}
}

public class BloodChangeLog : EffectLog{
	public Tile tile;
	public bool isBloody;
	
	public BloodChangeLog(Tile tile, bool isBloody){
		this.tile = tile;
		this.isBloody = isBloody;
	}
	
	public override IEnumerator Execute(){
		tile.isBloody = isBloody;
		tile.RenewColor();
		if(isBloody && VolatileData.difficulty != Difficulty.Intro && VolatileData.OpenCheck(Setting.WillChangeOpenStage))
			TutorialManager.subjectQueue.Enqueue("BloodEffect");
		yield break;
	}
	public override void Rewind(){
		tile.isBloody = !isBloody;
		tile.RenewColor();
	}
}

public class PositionChangeLog : EffectLog {
    public Unit target;
    public List<Tile> path; // path.First() : 시작 타일, path.Last() : 마지막 타일
    Vector2Int beforePos;
    public Vector2Int afterPos;
	public bool forced;
	bool charge;

	//Rewind를 위한 변수
	Direction beforeDir;
	int notMovedTurnCount;

	public PositionChangeLog(Unit unit, List<Tile> path, bool forced = false, bool charge = false) {
        target = unit;
        this.path = path;
        beforePos = path.First().location;
		beforeDir = unit.GetDir();
        afterPos = path.Last().location;
		this.forced = forced;
		this.charge = charge;
    }
    public override string GetText() {
        return target.GetNameKor() + " : " + beforePos + "에서 " + afterPos + "(으)로 위치 변경";
    }
    public override IEnumerator Execute(){
	    target.HideAfterImage();
		tiles = path;
		TileManager tileManager = TileManager.Instance;
        Tile destTile = tileManager.GetTile(afterPos);
        if (!target.IsHiddenUnderFogOfWar() && (!forced || Utility.CheckShowMotion())){
            if(target == BattleData.turnUnit && target.IsPC)
                BattleUIManager.Instance.TurnOffAllActions();
            float duration = Setting.moveDuration;
			if(!Utility.CheckShowMotion())
				duration /= 3;
            float time = 0;
	        //Debug.Log("돌진 여부: " + charge + " / 강제 여부: " + forced);
            while (true) {
                time += Time.deltaTime;
                if (time > duration) break;

				if (charge) { // 경로상 타일이 없는 구멍이 사이에 껴있는 경우도 가능해서
					target.transform.position = Calculate.Lerp (path[0].transform.position, path.Last().transform.position, 
						6 * (path.Count - 1) * time * time * (duration / 2 - time / 3) / (duration * duration * duration));
				} else {
					float x = 6 * (path.Count - 1) * time * time * (duration / 2 - time / 3) / (duration * duration * duration);
					if (x >= path.Count - 1)
						x = path.Count - 1.001f;
					Vector3 pos1 = path [(int)x].transform.position;
					Vector3 pos2 = path [(int)x + 1].transform.position;
					Vector3 position = Calculate.Lerp (pos1, pos2, x - (int)x);
					position.z = Mathf.Min (pos1.z, pos2.z) - 0.05f;
					target.transform.position = position;
					//Debug.Log("x: " + x + ", " + path[(int)x].Pos + " -> " + path[(int)x+1].Pos);
					if (!forced)
						target.SetDirection (_Convert.Vec2ToDir(path [(int)x + 1].Location - path [(int)x].Location));
				}
	            
				if (Utility.CheckShowMotion())
					BattleManager.Instance.cameraMover.MoveCameraToPosition (target.transform.position);
                yield return null;
            }

            if (target == BattleData.turnUnit && target.IsPC)
                BattleUIManager.Instance.ActionButtonOnOffLock = false;
	        target.UpdateRealPosition();
        }
        // 유닛A를 타일1->타일2로, 유닛B를 타일2->타일3으로 옮기는 일이 연달아 일어날 경우(ex : 카샤스티의 '셔플 불릿')
        // 일시적으로 타일2 위의 유닛이 둘이 될 수 있음. 
        // 그럴 경우에는 unitOnTile을 null이 아니라 유닛A로 해줘야 함
		MoveUnit(target, destTile);
		notMovedTurnCount = target.notMovedTurnCount;
        target.notMovedTurnCount = 0;
	    
	    UnitManager.Instance.CheckVigilAreaForAI();
    }
	public override void Rewind() {
		MoveUnit(target, path.First());
		target.SetDirection(beforeDir);
		target.notMovedTurnCount = notMovedTurnCount;
	}

	void MoveUnit(Unit unit, Tile destTile){
		List<Unit> unitsOnTile = UnitManager.GetAllUnits().FindAll(_Unit => _Unit.Pos == unit.Pos);
		unit.TileUnderUnit.SetUnitOnTile(unitsOnTile.Count <= 1 ? null : unitsOnTile.Find(iUnit => iUnit != unit));
		unit.SetPivot(destTile.Location);
		unit.UpdateRealPosition();
		if (unit != BattleData.turnUnit){
			Debug.Log(unit.CodeName + "(이)가 " + destTile + "로 위치 변경");
			Debug.Log("타일 위치: " + destTile.transform.position);
			Debug.Log("유닛 위치: " + unit.transform.position);
		}
		destTile.SetUnitOnTile(unit);
	}

	public override bool isMeaningless() {
        return beforePos == afterPos;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(target);
        logDisplay.CreateTileImage(path.First());
        logDisplay.CreateImage(VolatileData.GetIcon(IconSprites.RightArrow));
        logDisplay.CreateTileImage(path.Last());
    }
}

public class DirectionChangeLog : EffectLog {
	public Unit unit;
	Direction beforeDir;
	Direction afterDir;

	public DirectionChangeLog(Unit unit, Direction beforeDir, Direction afterDir) {
		this.unit = unit;
		this.beforeDir = beforeDir;
		this.afterDir = afterDir;
	}
	public override string GetText() {
		return unit.GetNameKor() + " : " + beforeDir + "에서 " + afterDir + "(으)로 방향 변경";
	}
	public override IEnumerator Execute() {
		unit.SetDirection(afterDir);
		yield return null;
	}
	public override void Rewind() {
		unit.SetDirection(beforeDir);
	}
	public override bool isMeaningless() {
		return beforeDir == afterDir;
	}
}

public class EvadeLog : EffectLog {
    public Unit caster;
    public Unit target;

    public EvadeLog(Unit caster, Unit target) {
        this.caster = caster;
        this.target = target;
    }
    public override string GetText() {
        return target.GetNameKor() + "이 " + caster.GetNameKor() + "의 공격을 회피";
    }
    public override IEnumerator Execute() {
		tiles = new List<Tile> { target.TileUnderUnit };
		target.GetListPassiveSkillLogic().TriggerOnEvasionEvent(caster, target);
        yield return null;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(target);
        logDisplay.CreateText("회피");
    }
}

public class DisplayScriptLog : EffectLog {
    string scriptName;

    public DisplayScriptLog(string scriptName) {
        this.scriptName = scriptName;
    }
    public override IEnumerator Execute(){
	    Debug.Log("대화 " + scriptName + " 실행");
	    if(RecordData.alreadyReadDialogues.Contains(scriptName)) yield break;

        VolatileData.progress.dialogueName = scriptName;
        DialogueManager.Instance.gameObject.SetActive(true);
        yield return DialogueManager.Instance.Initialize();
        DialogueManager.Instance.gameObject.SetActive(false);
	    RecordData.alreadyReadDialogues.Add(scriptName);
    }

	public override string GetText(){
		return "대화 " + scriptName + " 출력";
	}
}
	
public class PaintTilesLog : EffectLog {
	List<Tile> paintedTiles;
	Material color;
	public PaintTilesLog(List<Tile> tiles, Material color) {
		this.paintedTiles = tiles;
		this.color = color;
	}
	public override IEnumerator Execute() {
		TileManager.Instance.PaintTiles(paintedTiles, color);
		yield return null;
	}
	public override bool isMeaningless() {
		return paintedTiles == null || paintedTiles.Count == 0;
	}
}	

public class DepaintTilesLog : EffectLog {
	Material color;
	public DepaintTilesLog(Material color) {
		this.color = color;
	}
	public override IEnumerator Execute() {
		TileManager.Instance.DepaintAllTiles(color);
		yield return null;
	}
	public override bool isMeaningless() {
		return false;
	}
}

public class AISetActiveLog : EffectLog {
    Unit unit;

    public AISetActiveLog(Unit unit) {
        this.unit = unit;
    }
    public override string GetText() {
        return unit.GetNameKor() + ": 활성화";
    }
    public override IEnumerator Execute() {
		tiles = new List<Tile> { unit.TileUnderUnit };
		unit.GetComponent<AIData>().isActive = true;
        yield return null;
    }
	public override void Rewind() {
		unit.GetComponent<AIData>().isActive = false;
	}
	public override bool isMeaningless() {
        return unit.GetComponent<AIData>().isActive;
    }
    public override void CreateExplainObjects() {
        logDisplay.CreateUnitImage(unit);
        logDisplay.CreateText("활성화");
    }
}
public class ZoomLog : EffectLog {
	float size = 0;
	bool relative = false;
	float duration = 0;
	bool zoomOutOrIn = false;
	bool zoomOut;

	public ZoomLog(bool zoomOut, float duration) {
		this.zoomOutOrIn = true;
		this.zoomOut = zoomOut;
		this.duration = duration;
	}

	public ZoomLog(float size, float duration, bool relative = false) {
		this.size = size;
		this.duration = duration;
		this.relative = relative;
	}
	public override IEnumerator Execute() {
		if (zoomOutOrIn) {
			if (zoomOut)
				yield return BattleManager.Instance.cameraMover.ZoomOutCameraToViewMap(duration);
			else yield return BattleManager.Instance.cameraMover.ZoomInBack(duration);
		} else {
			if (size != 0) {
				yield return BattleManager.Instance.cameraMover.Zoom(size, duration, relative);
			} else yield return new WaitForSeconds(duration);
		}
	}
}

public class CameraMoveLog : EffectLog {
	public Vector2? pos;
    public float duration;
    bool isTilePos;

	public CameraMoveLog(Vector2? pos, float duration, bool isTilePos = false) {
		this.pos = pos;
        this.duration = duration;
        this.isTilePos = isTilePos;
	}
    public override string GetText() {
        if(pos != null)
            return (Vector2)pos + "로 카메라 이동";
        else return duration + " 대기";
    }
    public override IEnumerator Execute() {
        if (pos != null) {
            Vector2 realPos = (Vector2)pos;
            if(isTilePos)
                realPos = TileManager.Instance.GetTile(new Vector2Int((int)realPos.x, (int)realPos.y)).realPosition;
            yield return BattleManager.Instance.cameraMover.Slide(realPos, duration);
        } else yield return new WaitForSeconds(duration);
	}
}

public class SoundEffectLog : EffectLog {
    ActiveSkill skill;
    string name;

    public SoundEffectLog(ActiveSkill skill) {
        this.skill = skill;
    }
    public SoundEffectLog(string name) {
        this.name = name;
    }
    
    public override string GetText() {
        if(skill != null)
            return "음향 효과 : " + skill.GetName();
        return "음향 효과 : " + name;
    }
    public override IEnumerator Execute() {
        if(skill != null)
            EffectPlayer.ApplySoundEffect(skill);
        else SoundManager.Instance.PlaySE(name);
        yield return null;
    }
}

public class VisualEffectLog : EffectLog {
    Casting casting;
	Unit unit;
	GameObject effectPrefab;
	float duration;

    public VisualEffectLog(Casting casting) {
        this.casting = casting;
    }
	public VisualEffectLog(Unit unit, GameObject visualEffect, float duration){
		this.unit = unit;
		effectPrefab = visualEffect;
		this.duration = duration;
	}
    public override string GetText() {
        return "시각 효과 : " + (casting == null? effectPrefab.name : casting.Skill.GetName());
    }
    public override IEnumerator Execute(){   
		if (casting != null){
			if (casting.RealRange.Count == 0 || casting.Skill.castVeName == "-" || !Utility.CheckShowMotion())
				yield break;
			yield return EffectPlayer.ApplyVisualEffect(casting);
		}else if(unit != null){
			GameObject effect = MonoBehaviour.Instantiate(effectPrefab);
			effect.transform.position = unit.realPosition - new Vector3(0, 0, 0.01f);
			yield return Utility.WaitTime(duration);
			MonoBehaviour.Destroy(effect);
		}
    }
}

public class DisplayDamageTextLog : EffectLog {
    public Unit unit;
	DamageCalculator.AttackDamage damage;
    bool isHealth;
    bool isEvasion;

    public DisplayDamageTextLog(Unit unit, DamageCalculator.AttackDamage damage, bool isHealth, bool isEvasion = false){
        this.unit = unit;
		this.damage = damage;
        this.isHealth = isHealth;
        this.isEvasion = isEvasion;
    }
    public override string GetText(){
	    if(isHealth)
        	return unit.GetNameKor() + " : 피해량 표시(" + -damage.resultDamage + ")";
	    else
		    return unit.GetNameKor() + " : 행동력 변동(" + -damage.resultDamage + ")";
    }
    public override IEnumerator Execute(){
	    //Debug.Log(GetText() + "를 실행");
		int amount = -(int)damage.resultDamage;
		if (isEvasion)
			yield return unit.DisplayEvasionText ();
		else if (amount <= 0) {
			bool remain = true;
			if (parentEvent is CastLog)
				remain = ((CastLog)parentEvent).IsInitiator;
			if (isHealth) {
				yield return unit.DisplayDamageText (damage, remain);
			} else {
				yield return unit.DisplayAPChangeText (amount);
			}
		} else if (amount > 0) {
			if (isHealth) {
				yield return unit.DisplayRecoverText (amount);
			} else {
				yield return unit.DisplayAPChangeText (amount);
			}
		}
    }
}

public class AddLatelyHitInfoLog : EffectLog {
    Unit unit;
    HitInfo hitInfo;

    public AddLatelyHitInfoLog(Unit unit, HitInfo hitInfo) {
        this.unit = unit;
        this.hitInfo = hitInfo;
    }
    public override string GetText() {
        return unit.GetNameKor() + " : latelyHitInfo 추가 (" + hitInfo.caster.GetNameKor() +", " + hitInfo.skill + ", " + hitInfo.finalDamage + ")";
    }
    public override IEnumerator Execute() {
        unit.GetLatelyHitInfos().Add(hitInfo);
        yield return null;
    }
	public override void Rewind() {
		unit.GetLatelyHitInfos().Remove(hitInfo);
	}
	public override bool isMeaningless() {
        return hitInfo == null;
    }
}

public class WaitForSecondsLog : EffectLog{
    float seconds;

    public WaitForSecondsLog(float second){
        this.seconds = second;
    }

    public override string GetText(){
        return "WaitForSeconds(" + seconds + ")";
    }

    public override IEnumerator Execute(){
        yield return Utility.WaitTime(seconds);
    }
}
