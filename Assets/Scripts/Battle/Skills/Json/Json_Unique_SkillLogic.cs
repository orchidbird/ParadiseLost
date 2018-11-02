using System.Collections.Generic;
using Battle.Damage;
using System;
using Enums;
using UtilityMethods;

namespace Battle.Skills {
    class Json_Unique_SkillLogic : BasePassiveSkillLogic {
        public override void OnMyCastingApply(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;

            UnitStatusEffectInfo markSeInfo = passiveSkill.unitStatusEffectList.Find(se => se.displayKor == "표식");
            List<UnitStatusEffect> markStatusEffects = new List<UnitStatusEffect> { new UnitStatusEffect(markSeInfo, caster, target, passiveSkill) };
			
            StatusEffector.AttachAndReturnUSE(caster, markStatusEffects, target, true);
        }

		public override string GetStatusEffectExplanation(StatusEffect statusEffect) {
			string explanation = statusEffect.myInfo.Explanation;
			if(statusEffect.actuals.Count >= 2 && statusEffect.actuals[1].statusEffectType == StatusEffectType.DamageOverPhase)
				explanation += Environment.NewLine + Language.Select("지속 피해 <color=red>AMOUNT1</color>", "Get Damage <color=red>AMOUNT1</color> at the start of the turn.");
			return explanation;
		}
	}
}
