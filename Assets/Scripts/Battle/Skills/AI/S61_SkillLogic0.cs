using Battle.Damage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Skills {
	class S61_SkillLogic0 : AttachOnStart {
        // 8스테이지 몬스터 패시브 "완강한 복족류" 스킬로직

        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            // "완강함"이 아닌 군중제어가 붙을 경우("완강함"도 군중 제어로 판정됨)
            if(statusEffect.IsRestriction && statusEffect.GetOriginSkillName() != passiveSkill.Name){
                UnitStatusEffect stubbornness = target.statusEffectList.Find(se => se.GetOriginSkillName() == passiveSkill.Name);
                target.RemoveStatusEffect(stubbornness);
            }
            return true;
        }
        public override void TriggerOnStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit) {
            // 군중제어가 사라질 경우, 그리고 군중제어인 다른 cc기가 없을 경우
            if (statusEffect.IsRestriction && statusEffect.GetOriginSkillName() != passiveSkill.Name &&
                !unit.statusEffectList.Any(se => (se != statusEffect && se.IsRestriction))){
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(unit, passiveSkill, unit);
            }
        }
    }
}
