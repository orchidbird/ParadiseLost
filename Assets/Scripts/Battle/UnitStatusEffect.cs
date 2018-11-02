using System;
using Enums;
using System.Linq;
using System.Collections.Generic;

public class UnitStatusEffect : StatusEffect{
	public UnitStatusEffect(UnitStatusEffectInfo info, Unit caster, Unit owner = null, Skill originSkill = null, List<float> amounts = null) : base(){
		if (owner == null) owner = caster;
		myInfo = info;
		flexibleElem = new FlexibleElement(this, caster, owner, originSkill);
		((FlexibleElement.DisplayElement)flexibleElem.display).owner = owner;

		for (int i = 0; i < info.actuals.Count; i++){
			actuals.Add(new ActualElement(info.actuals[i]));
			if (amounts == null)
				CalculateAmount(i, false);
			else
				SetAmount(i, amounts [i], true);
		}
	}
	
	public bool IsValid{get{return actuals.Any(actual => actual.amount != 0 || (int)actual.statusEffectType > (int)StatusEffectType.Overload);}}
	public bool IgnoreNewEffect(UnitStatusEffect newUSE){//새로운 효과를 받을지 말지 결정하기 위해 확인
		if (IsStackable || !IsDurationFull) return false;
		for (int i = 0; i < actuals.Count; i++)
			if(Math.Abs(actuals[i].amount) < Math.Abs(newUSE.actuals[i].amount))
				return false;
		return true;
	}

    public new class FlexibleElement : StatusEffect.FlexibleElement {
        public new class DisplayElement : StatusEffect.FlexibleElement.DisplayElement {
        	public Unit owner;

        	public DisplayElement(Unit caster, Unit owner, Skill originSkill, int defaultPhase)
	                : base(caster, originSkill, defaultPhase){
	            this.owner = owner;
    	    }
        }
	    
        public FlexibleElement(UnitStatusEffect statusEffect, Unit caster, Unit owner, Skill originSkill)
                : base(statusEffect, caster, originSkill) {
	        display = new DisplayElement(caster, owner, originSkill, statusEffect.myInfo.defaultPhase);
        }
    }
    
	public bool IsBuff{get { return ((UnitStatusEffectInfo)myInfo).category == StatusEffectCategory.Buff; }}
	public bool IsDebuff{get { return ((UnitStatusEffectInfo)myInfo).category == StatusEffectCategory.Debuff; }}

	public Unit GetOwner() { return ((FlexibleElement.DisplayElement)flexibleElem.display).owner; }
    protected override float GetStatusEffectVar(int i) {
        float statusEffectVar = 0;
        Skill originSkill = GetOriginSkill();
        if (originSkill != null) {
            if (originSkill is ActiveSkill)
                statusEffectVar = ((ActiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
            else if(originSkill is PassiveSkill)
                statusEffectVar = ((PassiveSkill)GetOriginSkill()).SkillLogic.GetStatusEffectVar(this, i, GetCaster(), GetOwner());
        }
        return statusEffectVar;
    }

	public bool IsRestriction{get{return EnumUtil.ccTypeList.Any(IsTypeOf);}}

	public bool IsSourceTrap() {
		var statusEffectList = new List<TileStatusEffectInfo>();
		Skill originSkill = GetOriginSkill();
		if(originSkill is ActiveSkill) 
			statusEffectList = ((ActiveSkill)originSkill).tileStatusEffectList;
		return statusEffectList.Any(tse => tse.actuals.Any(actual => actual.statusEffectType == StatusEffectType.Trap));
	}
}
