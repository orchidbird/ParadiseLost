using Enums;
using Battle.Damage;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerUsingSkill(Casting casting, List<Unit> targets) {
            Unit caster = casting.Caster;
            bool allTargetsHaveSameElement = true;
            bool firstTarget = true;
            Element element = Element.None;

            UnitStatusEffect originalStatusEffect = caster.statusEffectList.Find(se => se.GetOriginSkillName() == "정제");

            foreach (var target in targets) {
                if (firstTarget == true) {
                    firstTarget = false;
                    element = target.GetElement();
                    continue;
                }
                if (target.GetElement() != element) {
                    allTargetsHaveSameElement = false;
                    break;
                }
            }
            if(targets.Count == 0) allTargetsHaveSameElement = false;
            if (allTargetsHaveSameElement && originalStatusEffect != null && element == originalStatusEffect.GetElement())
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
            else if (allTargetsHaveSameElement) {
                if (originalStatusEffect != null)
                    caster.RemoveStatusEffect(originalStatusEffect);
                List<UnitStatusEffect> statusEffects = StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
                if (statusEffects.Count >= 1)
                    statusEffects[0].flexibleElem.display.element = element;
            } else if(originalStatusEffect != null)
                caster.RemoveStatusEffect(originalStatusEffect);
        }
    }
}