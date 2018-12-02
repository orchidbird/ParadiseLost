using System;
using Enums;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using GameData;
using Battle.Skills;
using UtilityMethods;

public class StatusEffect{
	public StatusEffectInfo myInfo;
    public FlexibleElement flexibleElem;
	public List<ActualElement> actuals = new List<ActualElement>();
	
	public class ActualElement{
		public StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류 
		// var * coef + base
		public Formula formula;
		public readonly bool isPercent;
		public readonly bool isMultiply;
		public float amount; // 영향을 주는 실제 값

		public ActualElement(StatusEffectType statusEffectType, Formula formula, bool isPercent, bool isMultiply){
			this.statusEffectType = statusEffectType;
			this.formula = formula;
			this.isPercent = isPercent;
			this.isMultiply = isMultiply;
		}

		public ActualElement(ActualElement PrefabToClone){
			statusEffectType = PrefabToClone.statusEffectType;
			formula = PrefabToClone.formula;
			isPercent = PrefabToClone.isPercent;
			isMultiply = PrefabToClone.isMultiply;
			amount = PrefabToClone.amount;
		}

		public ActualElement(StatusEffectType statusEffectType, float amount) {
			this.statusEffectType = statusEffectType;
			this.amount = amount;
		}
	}

    public class FlexibleElement {
        public DisplayElement display;
        public class DisplayElement {
            public Unit caster; // 시전자
            public Skill originSkill;
            public int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
            public int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
            public Element element; // StatusEffect의 속성. 큐리 패시브 등에 사용
            
            public Unit objectBeingCollected = null;  // 수집 statusEffect인 경우에, 수집되고 있는 유닛을 기억.

            public DisplayElement(Unit caster, Skill originSkill, int defaultPhase) {
                this.originSkill = originSkill;
                this.caster = caster;
                remainStack = 1;
                remainPhase = defaultPhase;
            }
        }

        public FlexibleElement(StatusEffect statusEffect, Unit caster, Skill originSkill){
	        Debug.Assert(statusEffect != null);
	        Debug.Assert(statusEffect.myInfo != null);
	        Debug.Assert(caster != null);
            display = new DisplayElement(caster, originSkill, statusEffect.myInfo.defaultPhase);
        }
    }

	protected StatusEffect(){/*base 할당을 위한 무의미한 코드*/}
	public StatusEffect(StatusEffect statusEffect){
		myInfo = statusEffect.myInfo;
		foreach (var actual in statusEffect.actuals)
			actuals.Add(new ActualElement(actual));
		
		flexibleElem = new FlexibleElement(this, statusEffect.GetCaster(), statusEffect.GetOriginSkill());
		Clone(statusEffect);
	}

	public void Clone(StatusEffect statusEffect){
		for (int i = 0; i < statusEffect.actuals.Count; i++)
			SetAmount(i, statusEffect.actuals[i].amount, true);
		flexibleElem.display.element = statusEffect.flexibleElem.display.element;
		flexibleElem.display.objectBeingCollected = statusEffect.flexibleElem.display.objectBeingCollected;
		flexibleElem.display.remainPhase = statusEffect.flexibleElem.display.remainPhase;
		flexibleElem.display.remainStack = statusEffect.flexibleElem.display.remainStack;
	}

    public string GetOwnerOfSkill() { return myInfo.ownerOfSkill; }
    public string GetOriginSkillName() { return myInfo.originSkillName; }

	public string DisplayName(bool fixKor){
		if (!fixKor){
			var name = myInfo.DisplayName;
			return name == "SAME" ? GetOriginSkill().Name : name;	
		}
		
		var korName = myInfo.displayKor;
		return korName == "SAME" ? GetOriginSkill().korName : korName;
	}
    public bool GetIsInfinite() { return myInfo.defaultPhase == 99; }
    public bool IsStackable{get { return myInfo.maxStack > 1; }}
	public int Stack{get { return flexibleElem.display.remainStack; }}
	public bool IsDurationFull{get { return myInfo.defaultPhase == Duration(); }}
	public void RefillDuration(){flexibleElem.display.remainPhase = myInfo.defaultPhase;}
	public int Duration() { return flexibleElem.display.remainPhase; }
	public bool GetIsOnce() { return myInfo.isOnce; }
    public bool GetAmountToBeUpdated() { return myInfo.amountToBeUpdated; }
    public bool GetIsRemovable() { return myInfo.isRemovable; }
    public Skill GetOriginSkill() { return flexibleElem.display.originSkill; }
    public Unit GetCaster() { return flexibleElem.display.caster; }

	public Element GetElement() { return flexibleElem.display.element; }
    public Unit GetObjectBeingCollected() { return flexibleElem.display.objectBeingCollected; }
    public void SetObjectBeingCollected(Unit obj) { flexibleElem.display.objectBeingCollected = obj;}
    public StatusEffectType GetStatusEffectType(int index) { return actuals[index].statusEffectType; }
    public bool GetIsPercent(int index) { return actuals[index].isPercent; }
    public bool GetIsMultiply(int index) { return actuals[index].isMultiply; }
	
    public void DecreaseRemainPhase(int phase) { SetRemainPhase(flexibleElem.display.remainPhase - phase); }
    public void SetRemainPhase(int phase) {
        int beforePhase = flexibleElem.display.remainPhase;
        LogManager.Instance.Record(new StatusEffectLog(this, StatusEffectChangeType.DurationChange, 0, beforePhase, phase));
    }
    public void AddRemainStack(int stack) { SetRemainStack(flexibleElem.display.remainStack + stack); }
    public void DecreaseRemainStack() { DecreaseRemainStack(1); }
    public void DecreaseRemainStack(int stack) { SetRemainStack(flexibleElem.display.remainStack - stack); }
    public void SetRemainStack(int stack) {
        int beforeStack = flexibleElem.display.remainStack;
        LogManager.Instance.Record(new StatusEffectLog(this, StatusEffectChangeType.StackChange, 0, beforeStack, stack));
    }
    public bool isMaxStack() {
        return flexibleElem.display.remainStack == myInfo.maxStack;
    }
	
	public bool IsRenewable(StatusEffect statusEffect) {	// this와 statusEffect가 같은 종류라고 가정
		for(int i = 0; i < actuals.Count; i++) {
			if(GetAmount(i) != statusEffect.GetAmount(i))
				return true;
		}

		return !(isMaxStack() && Duration() == statusEffect.Duration());
	}
    
    int? FindIndexOfType(StatusEffectType statusEffectType){
        for (int i = 0; i < actuals.Count; i++)
	        if (statusEffectType.Equals(GetStatusEffectType(i)))
		        return i;
	    return null;
    }
	
	public float GetAmount(int index){
		var actual = actuals[index];
		return actual.amount * (IsStackable && !IsAura() && actual.statusEffectType != StatusEffectType.Shield ? Stack : 1);
	}
    public float GetAmountOfType(StatusEffectType statusEffectType){
	    var index = FindIndexOfType(statusEffectType);
	    var result = index == null ? 0 : GetAmount((int)index);
	    return result;
    }
    
    public void SetAmount(int index, float amount, bool withoutLog = false){
        float beforeAmount = actuals[index].amount;
	    if (withoutLog)
		    actuals[index].amount = amount;
	    else
		    LogManager.Instance.Record(new StatusEffectLog(this, StatusEffectChangeType.AmountChange, index, beforeAmount, amount));
    }
	public void SubAmount(int index, float amount) { SetAmount(index, actuals[index].amount - amount); }
    public void SetAmountOfType(StatusEffectType statusEffectType, float amount) {
        var index = FindIndexOfType(statusEffectType);
	    if (index == null) return;
	    SetAmount((int)index, amount);    
    }
    
    public bool IsTypeOf(StatusEffectType statusEffectType) {
        bool isOfType = false;
        for(int i = 0; i < actuals.Count; i++) {
            if (statusEffectType.Equals(GetStatusEffectType(i))) {
                isOfType = true;
            }
        }
        return isOfType;
    }
	public bool IsAura(){
		StatusEffectType type = GetStatusEffectType (0);
		return type == StatusEffectType.Aura || type == StatusEffectType.EnemyAura || type == StatusEffectType.AllyAura;
	}

    public bool IsOfType(int index, StatusEffectType statusEffectType) {
        return statusEffectType.Equals(GetStatusEffectType(index));
    }
    
    public bool IsSameStatusEffect(StatusEffect anotherStatusEffect) {
        return (GetOriginSkillName().Equals(anotherStatusEffect.GetOriginSkillName()) &&
                    DisplayName(false).Equals(anotherStatusEffect.DisplayName(false)));
    }
    public string GetExplanation() {
        string text = "";
	    var originSkill = GetOriginSkill();
        if(originSkill is PassiveSkill)
            text = ((PassiveSkill)originSkill).SkillLogic.GetStatusEffectExplanation(this);
		if (text == "")
			text = myInfo.Explanation;
	    if (text == "SAME")
		    return originSkill.Explaination;
	    
        for (int i = 0; i < actuals.Count; i++) {
            string minusAmountString = (-(int)GetAmount(i)).ToString();
            string amountString = ((int)GetAmount(i)).ToString();
            if (actuals[i].isPercent) amountString += "%";
            text = text.Replace("-AMOUNT" + i, minusAmountString);
            text = text.Replace("AMOUNT" + i, amountString);
        }
        return text;
    }
    public Sprite GetSprite() {
	    Skill originSkill = GetOriginSkill();
        if (originSkill != null)
            return originSkill.icon;
	    if(myInfo.displayEng == "Fear")
		    return VolatileData.GetIcon(IconSprites.Fear);
	    
	    foreach (var statusEffectType in EnumUtil.ccTypeList)
		    if (IsTypeOf(statusEffectType))
			    return VolatileData.GetIcon((IconSprites)Enum.Parse(typeof(IconSprites), statusEffectType.ToString()));
	    
        return VolatileData.GetIcon(IconSprites.Black);
    }
    public void CalculateAmountManually(int i, float statusEffectVar){
        float result = (float)actuals[i].formula.Substitute(statusEffectVar);
        LogManager.Instance.Record(new StatusEffectLog(this, StatusEffectChangeType.AmountChange, i, actuals[i].amount, result));
    }
    public void CalculateAmount(int i, bool isUpdate) {
	    if (!GetAmountToBeUpdated() && isUpdate) return;
	    
	    Formula formula = actuals[i].formula;
	    float? amount = formula.Result(GetCaster(), this);
	    SetAmount(i, amount ?? (float)formula.Substitute(GetStatusEffectVar(i)), true);
    }

    protected virtual float GetStatusEffectVar(int i) {
        return 0;
    }
}
