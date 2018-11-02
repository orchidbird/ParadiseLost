using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Enums;

namespace Battle.Skills{
    public class ListPassiveSkillLogic : BasePassiveSkillLogic{
	    public List<BasePassiveSkillLogic> passiveSkillLogics;

	    public ListPassiveSkillLogic(List<BasePassiveSkillLogic> passiveSkillLogics){
		    this.passiveSkillLogics = passiveSkillLogics;
	    }

		public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit neutralizedUnit, TrigActionType actionType){
			foreach (var skillLogic in passiveSkillLogics){
                skillLogic.TriggerOnNeutralizeByMyHand(hitInfo, neutralizedUnit, actionType);
			}
		}
		public override void TriggerAfterUnitsDestroyed(Unit unit) {
			foreach (var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerAfterUnitsDestroyed (unit);
			}
		}

        public override float GetAdditionalRelativePowerBonus(Unit caster) {
            float totalAdditionalPowerBonus = 1.0f;
            foreach (var skillLogic in passiveSkillLogics) {
                totalAdditionalPowerBonus *= skillLogic.GetAdditionalRelativePowerBonus(caster);
            }
            return totalAdditionalPowerBonus;
        }

        public override float GetAdditionalAbsoluteDefenseBonus(Unit caster) {
            float totalAdditionalDefenseBonus = 0;
            foreach (var skillLogic in passiveSkillLogics) {
                totalAdditionalDefenseBonus += skillLogic.GetAdditionalAbsoluteDefenseBonus(caster);
            }
            return totalAdditionalDefenseBonus;
        }

        public override float ApplyIgnoreResistanceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceRelativeValueByEachPassive(appliedSkill, target, caster, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceAbsoluteValueByEachPassive(appliedSkill, target, caster, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreDefenceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceRelativeValueByEachPassive(appliedSkill, target, caster, defense);
            }
            return defense;
        }

        public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceAbsoluteValueByEachPassive(appliedSkill, target, caster, defense);
            }
            return defense;
        }
			
		public override float ApplyBonusEvasionFromTargetPassive(CastingApply castingApply) {
			float totalBonus = 0;
			foreach (var skillLogic in passiveSkillLogics) {
				totalBonus += skillLogic.ApplyBonusEvasionFromTargetPassive (castingApply);
			}
			return totalBonus;
		}

	    public override void OnCastingAmountCalculation(CastingApply castingApply){
		    foreach (var skillLogic in passiveSkillLogics)
			    skillLogic.OnCastingAmountCalculation(castingApply);
	    }
        public override void OnReceivingAmountCalculation(CastingApply castingApply){
            foreach (var skillLogic in passiveSkillLogics)
                skillLogic.OnReceivingAmountCalculation(castingApply);
        }

        public override void OnMyCastingApply(CastingApply castingApply) {
            foreach(var skillLogic in passiveSkillLogics)
                skillLogic.OnMyCastingApply(castingApply);
        }
	    public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
		    foreach(var skillLogic in passiveSkillLogics)
			    skillLogic.OnAnyCastingDamage(castingApply, chain);
	    }
	    
        public override Dictionary<Vector2Int, TileWithPath> GetMovablePath(Unit unit) {
            Dictionary<Vector2Int, TileWithPath> path = null;
            foreach(var skillLogic in passiveSkillLogics) {
                Dictionary<Vector2Int, TileWithPath> eachPath = skillLogic.GetMovablePath(unit);
                if(eachPath == null)
                    continue;
                if(path == null)
                    path = new Dictionary<Vector2Int, TileWithPath>();
                foreach(var kv in eachPath) {
                    if(!path.ContainsKey(kv.Key))
                        path.Add(kv.Key, kv.Value);
                    else if(path[kv.Key].requireActivityPoint > kv.Value.requireActivityPoint)
                        path[kv.Key] = kv.Value;
                }
            }
            return path;
        }

        public override void TriggerOnEvasionEvent(Unit caster, Unit target){
		    foreach (var skillLogic in passiveSkillLogics)
			    skillLogic.TriggerOnEvasionEvent(caster, target);
	    }

	    public override void TriggerActiveSkillAppliedToOwner(CastingApply castingApply)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerActiveSkillAppliedToOwner(castingApply);
		    }
	    }

        // caster�� passiveSkill
        public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerActiveSkillDamageAppliedByOwner(caster, target);
            }
        }

        public override bool IgnoreCasting(CastingApply apply, int chainCombo){
	        return passiveSkillLogics.Any(logic => logic.IgnoreCasting(apply, chainCombo));
        }

	    //의도는 IgnoreCasting의 NonCasting 버전.
        public override bool TriggerDamagedByNonCasting(Unit caster, float damage, Unit target){
	        return passiveSkillLogics.All(logic => logic.TriggerDamagedByNonCasting(caster, damage, target));
        }

        public override void TriggerAfterDamaged(Unit target, int damage, Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerAfterDamaged(target, damage, caster);
            }
		}
		public override void TriggerAfterDamagedByCasting(Unit target, Unit caster) {
			foreach (var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerAfterDamagedByCasting(target, caster);
			}
		}
		public override void TriggerAfterStatusEffectAttachedByCasting(UnitStatusEffect statusEffect, Unit target, Unit caster){ // 기술로 자신에게 효과가 붙을 때
			foreach(var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerAfterStatusEffectAttachedByCasting (statusEffect, target, caster);
			}
		}

        public override void TriggerBeforeStartChain(List<Chain> chainList, Casting casting) {
            foreach (var skillLogic in passiveSkillLogics)
                skillLogic.TriggerBeforeStartChain(chainList, casting);
        }
	    public override void TriggerOnChain(Chain chain){
		    foreach (var skillLogic in passiveSkillLogics)
			    skillLogic.TriggerOnChain(chain);
	    }
	    
		public override void TriggerTargetPassiveBeforeCast(Casting casting, Unit target) {
			foreach(var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerTargetPassiveBeforeCast(casting, target);
			}
		}
		public override void TriggerExistingUnitPassiveBeforeCast(Casting casting, Unit skillOwner) { // 본인이 맞는 공격이 아니더라도 스테이지에 존재만 하면 영향을 미치는 특성
			foreach(var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerExistingUnitPassiveBeforeCast (casting, skillOwner);
			}
		}
		public override void TriggerExistingUnitPassiveOnDebuffAttach(UnitStatusEffect se, Unit existingUnit) { // 스테이지에 존재만 하면 모든 약화 부착에 영향을 미치는 특성
			foreach(var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerExistingUnitPassiveOnDebuffAttach (se, existingUnit);
			}
		}

		public override void TriggerAfterCast(CastLog castLog) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerAfterCast(castLog);
            }
		}
	    
	    //List.Count == 0인 경우가 있어서 List.Max를 쓰지 않음
	    public override float GetRetreatHPRatioOfMyTarget(Unit target){
		    float retreatHPRatio = Setting.retreatHPFloat;
		    foreach (var skillLogic in passiveSkillLogics) {
			    float ratio = skillLogic.GetRetreatHPRatioOfMyTarget(target);
			    if (ratio > retreatHPRatio)
				    retreatHPRatio = ratio;
		    }

		    var retreatHpChangeEffects =
			    target.statusEffectList.FindAll(item => item.IsTypeOf(StatusEffectType.RetreatHPChange));
		    if (target.preview != null)
			    retreatHpChangeEffects.AddRange(target.preview.newStatusEffects);
		    foreach (var se in retreatHpChangeEffects){
			    var ratio = se.GetAmount(0) / 100;
			    if(ratio > retreatHPRatio)
				    retreatHPRatio = ratio;
		    }

		    return retreatHPRatio;
	    }

        public override void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerAfterMove(caster, beforeTile, afterTile);
            }
        }

        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){
	        return passiveSkillLogics.All(logic => logic.WillReceiveSE(statusEffect, caster, target));
        }

        public override void TriggerOnStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit) {
            foreach (var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerOnStatusEffectRemoved (statusEffect, unit);
            }
        }

        public override void TriggerUsingSkill(Casting casting, List<Unit> targets)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerUsingSkill(casting, targets);
		    }
	    }
		public override void TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount, bool duringAIDecision) {
            foreach(var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(attacker, shieldCaster, target, amount, duringAIDecision);
            }
        }
        public override void TriggerWhenShieldExhaustedByDamage(Unit shieldOwner, Unit shieldCaster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerWhenShieldExhaustedByDamage(shieldOwner, shieldCaster);
            }
        }
        public override void TriggerOnMove(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnMove(caster);
            }
        }

        public override void TriggerApplyingHeal(CastingApply castingApply) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerApplyingHeal(castingApply);
            }
        }

        public override void TriggerOnStageStart(Unit caster) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnStageStart(caster);
            }
        }

        public override void TriggerOnPhaseStart(Unit caster, int phase) {		
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerOnPhaseStart(caster, phase);
		    }
	    }

        public override void TriggerOnPhaseEnd(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnPhaseEnd(caster);
            }
        }

        public override void TriggerOnActionEnd(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnActionEnd(caster);
            }
        }

        public override void TriggerOnRest(Unit caster) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnRest(caster);
            }
        }
		public override void TriggerOnMyTurnStart(Unit caster) {
			foreach (var skillLogic in passiveSkillLogics) {
				skillLogic.TriggerOnMyTurnStart(caster);
			}
		}
        public override void TriggerOnAnyTurnStart(Unit caster, Unit turnStarter) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnAnyTurnStart(caster, turnStarter);
            }
        }
        public override void TriggerOnTurnEnd(Unit caster, Unit turnEnder){
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnTurnEnd(caster, turnEnder);
            }
		}
		public override void TriggerOnDestroyed(Unit actor, TrigActionType destroyType, Unit destroyedUnit){
			foreach (var skillLogic in passiveSkillLogics){
				skillLogic.TriggerOnDestroyed (actor, destroyType, destroyedUnit);
			}
		}
        public override void TriggerStatusEffectsOnRest(Unit target, UnitStatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnRest(target, statusEffect);
            }
        }
        public override void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, UnitStatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnUsingSkill(target, targetsOfSkill, statusEffect);
            }
        }
        public override void TriggerStatusEffectsOnMove(Unit target, UnitStatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnMove(target, statusEffect);
            }
        }
        public override void TriggerStatusEffectAtActionEnd(Unit target, UnitStatusEffect statusEffect) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectAtActionEnd(target, statusEffect);
            }
        }
        public override bool TriggerOnSteppingTrap(Unit caster, TileStatusEffect trap) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerOnSteppingTrap(caster, trap)) {
                    ignored = true;
                }
            }
            return !ignored;
        }
        public override void TriggerOnTrapOperated(Unit unit, TileStatusEffect trap) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnTrapOperated(unit, trap);
            }
        }
    }
}
