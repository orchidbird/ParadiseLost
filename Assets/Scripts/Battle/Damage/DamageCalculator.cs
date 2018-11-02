using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Battle.Skills;
using GameData;

namespace Battle {
    public class DamageCalculator{
		public class AttackDamage {
			public float baseDamage = 0;
			public DirectionCategory attackDirection = DirectionCategory.Front;
			public Dictionary<Sprite, float> relativeModifiers = new Dictionary<Sprite, float>();
			public Dictionary<Sprite, int> absoluteModifiers = new Dictionary<Sprite, int>();

			public void AddModifier(Skill skill, float value, bool absolute = false){
				if(absolute)
					absoluteModifiers.Add(skill.icon, (int)value);
				else
					relativeModifiers.Add(skill.icon, value);
			}
			
			public float resultDamage = 0;

            public AttackDamage(float damage = 0) {
                resultDamage = damage;
            }

			public int CountTacticalBonus{get { return relativeModifiers.Count(kv => (kv.Key.name == "Direction" || kv.Key.name == "Height") && kv.Value > 1); }}
			public bool IsCritical{get{return CountTacticalBonus >= 2;}}
			public bool HasTacticalBonus{get{return CountTacticalBonus > 0;}}
			
			public float TotalRelativeModifier { get {
					float result = 1;
					foreach(var modifier in relativeModifiers)
						result *= modifier.Value;
					return result;
				}
			}
		}

		public static void CalculateAmountOtherThanAttackDamage(CastingApply castingApply) {
			Unit caster = castingApply.Caster;
			AttackDamage attackDamage = castingApply.GetDamage();
			ActiveSkill appliedSkill = castingApply.GetSkill();

			attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
			// 해당 기술의 추가데미지 계산
			SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(castingApply);
            // 타겟의 statusEffect에 의한 추가데미지 계산
			foreach(var statusEffect in castingApply.Target.statusEffectList) {
				Skill originSkill = statusEffect.GetOriginSkill();
				if(originSkill is ActiveSkill)
					((ActiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromTargetStatusEffect(castingApply, statusEffect);
			}
            // 시전자의 statusEffect에 의한 추가데미지 계산
            foreach(var statusEffect in caster.statusEffectList){
                Skill originSkill = statusEffect.GetOriginSkill();
                if(originSkill is PassiveSkill)
                    ((PassiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromCasterStatusEffect(castingApply, statusEffect);
            }

			//특성에 의한 변동
			caster.GetListPassiveSkillLogic().OnCastingAmountCalculation(castingApply);
			castingApply.Target.GetListPassiveSkillLogic().OnReceivingAmountCalculation(castingApply);
			attackDamage.resultDamage = attackDamage.baseDamage * attackDamage.TotalRelativeModifier;
		}
		public static void CalculateAttackDamage(CastingApply castingApply, int chainCombo){
			Unit caster = castingApply.Caster;
			Unit target = castingApply.Target;
			AttackDamage attackDamage = castingApply.GetDamage();
			ActiveSkill appliedSkill = castingApply.GetSkill();

			attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
			attackDamage.relativeModifiers.Add(Resources.Load<Sprite>("Icon/Direction"), DirectionBonus(caster, target));
			attackDamage.relativeModifiers.Add(Resources.Load<Sprite>("Icon/Height"), HeightBonus(caster, target));
			attackDamage.relativeModifiers.Add(Resources.Load<Sprite>("Icon/Chain"), ChainComboBonus(chainCombo));
			attackDamage.attackDirection = AttackDirection(caster, target);
			
			foreach (var smite in caster.statusEffectList.FindAll(se => se.actuals.Any(actual => actual.statusEffectType == StatusEffectType.Smite)))
				attackDamage.absoluteModifiers.Add(smite.GetOriginSkill().icon, (int)smite.GetAmount(0));

			// 해당 기술의 추가데미지 계산
			SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(castingApply);
            // 타겟의 statusEffect에 의한 추가데미지 계산
            foreach (var statusEffect in target.statusEffectList){
				Skill originSkill = statusEffect.GetOriginSkill();
				if (originSkill is ActiveSkill) {
					((ActiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromTargetStatusEffect (castingApply, statusEffect);
				}
			}

            // 시전자의 statusEffect에 의한 추가데미지 계산
            foreach (var statusEffect in caster.statusEffectList) {
                Skill originSkill = statusEffect.GetOriginSkill();
                if (originSkill is PassiveSkill)
                    ((PassiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromCasterStatusEffect(castingApply, statusEffect);
            }

            //공격자 특성에 의한 피해량 변동
			caster.GetListPassiveSkillLogic().OnCastingAmountCalculation(castingApply);
            //피격자 특성에 의한 피해량 변동
            target.GetListPassiveSkillLogic().OnReceivingAmountCalculation(castingApply);

			// 시전자 효과에 의한 추가데미지
			foreach (var statusEffect in caster.statusEffectList){
				for (int i = 0; i < statusEffect.actuals.Count; i++){
					if (!statusEffect.IsOfType(i, StatusEffectType.DamageChange)) continue;
					
					float amount = statusEffect.GetAmount(i);
					if (statusEffect.GetIsPercent(i))
						attackDamage.relativeModifiers.Add(statusEffect.GetSprite(), amount / 100 + 1);
					else
						attackDamage.absoluteModifiers.Add(statusEffect.GetSprite(), (int)amount);
					//appliedChangeList.Add(new ValueChange(statusEffect.GetIsMultiply(i), amount));
				}
			}
			
			attackDamage.resultDamage = attackDamage.baseDamage;
			foreach (var kv in attackDamage.relativeModifiers)
				attackDamage.resultDamage *= kv.Value;
			foreach (var kv in attackDamage.absoluteModifiers)
				attackDamage.resultDamage += kv.Value;
		}

		private static float PowerFactorDamage(ActiveSkill appliedSkill, Unit casterUnit)
		{
			float damage = 0;

			float factor;
			float stat;

			if (appliedSkill.statType == Stat.Power) {
				factor = appliedSkill.GetPowerFactor (Stat.Power);
				stat = casterUnit.GetStat (Stat.Power);
				damage = factor * stat;
			} else {
				factor = appliedSkill.GetPowerFactor (Stat.Agility);
				stat = casterUnit.GetStat (Stat.Agility);
				damage = factor * stat;
			}

			//Debug.Log("baseDamage : " + damage);

			return damage;
		}

		private static float DirectionBonus(Unit caster, Unit target) {
			if(!VolatileData.OpenCheck(Setting.directionOpenStage))
				return 1.0f;
			float directionBonus = Utility.GetDirectionBonus(caster, target);
			//Debug.Log("\tdirectionBonus : " + directionBonus);
			return directionBonus;
		}

		private static DirectionCategory AttackDirection(Unit caster, Unit target)
		{
			float directionBonus = Utility.GetDirectionBonus(caster, target);
			if (directionBonus == Setting.sideAttackBonus)
				return DirectionCategory.Side;
			else if (directionBonus == Setting.backAttackBonus)
				return DirectionCategory.Back;
			else
				return DirectionCategory.Front; 
		}

		private static float HeightBonus(Unit caster, Unit target) {
			float heightBonus = Utility.GetHeightBonus(caster, target);
			return heightBonus;
		}

		private static float ChainComboBonus(int chainCombo) {
			float chainBonus = GetChainDamageFactorFromChainCombo(chainCombo);
			return chainBonus;
		}

		private static float GetChainDamageFactorFromChainCombo(int chainCombo)
		{
			if (chainCombo < 2)	return 1.0f;
			else if (chainCombo == 2) return 1.2f;
			else if (chainCombo == 3) return 1.5f;
			else if (chainCombo == 4) return 2.0f;
			else return 3.0f;  
		}

		private static float SmiteAmount(Unit casterUnit) {
			float smiteAmount = 0;
			smiteAmount = casterUnit.CalculateActualAmount(smiteAmount, StatusEffectType.Smite);
			return smiteAmount;
		}

		public static float CalculateReflectDamage(float attackDamage, Unit target, Unit reflectTarget){
			float reflectAmount = 0;
			foreach (var statusEffect in target.statusEffectList)
				if (statusEffect.IsTypeOf(StatusEffectType.Reflect))
					reflectAmount += attackDamage * statusEffect.GetAmountOfType(StatusEffectType.Reflect)/100;
			return reflectAmount;
		}

		public static float ApplyDefense(float damage, float defense){
			if (defense <= -180) return damage * 10;
			return damage * 200.0f / (200.0f + defense);
		}
		public static float CalculateDefense(ActiveSkill appliedSkill, Unit target, Unit caster) {
			float defense = target.GetStat(Stat.Defense);

			// 기술에 의한 방어 무시 (상대값)
			defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceRelativeValueBySkill(defense, caster, target);

			// 특성에 의한 방어 무시 (상대값)
			List<PassiveSkill> casterPassiveSkills = caster.GetPassiveSkillList();
			defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceRelativeValueByEachPassive(appliedSkill, target, caster, defense); 

			// 기술에 의한 방어 무시 (절대값)
			defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceAbsoluteValueBySkill(defense, caster, target);

			// 특성에 의한 방어 무시 (절대값)
			defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceAbsoluteValueByEachPassive(appliedSkill, target, caster, defense);
			return defense;
		}
	}
}
