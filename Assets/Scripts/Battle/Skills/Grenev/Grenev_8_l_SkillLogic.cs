using System;
using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Grenev_8_l_SkillLogic : BaseActiveSkillLogic {
        private bool isHealthRatioSmallEnough(Unit target) {
            return (float)target.GetHP / target.GetMaxHealth() <= 0.35f;
        }
        public override bool MayDisPlayDamageCoroutine(CastingApply castingApply) {
            if (isHealthRatioSmallEnough(castingApply.Target)) {
                return false;
            }
            return true;
        }
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;
            if (isHealthRatioSmallEnough(target)) {
                target.ApplyDamageByNonCasting(target.GetHP, caster, true, -target.GetStat(Stat.Defense), -target.GetStat(Stat.Resistance), true);
            }
        }
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            if(isHealthRatioSmallEnough(castingApply.Target)) {
	            castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 0);
                //castingApply.GetDamage().RelativeModifier = 0;
            }
        }
        public override float ApplyIgnoreDefenceRelativeValueBySkill(float defense, Unit caster, Unit target) {
            if (isHealthRatioSmallEnough(target)) {
                return defense * 0.5f;
            }
            return defense;
        }
    }
}
